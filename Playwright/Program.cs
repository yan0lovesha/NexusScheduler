// See https://aka.ms/new-console-template for more information
using Microsoft.Playwright;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

[DllImport("User32.dll", CharSet = CharSet.Unicode)]
static extern int MessageBox(IntPtr h, string m, string c, int type);

Console.WriteLine("Start checking availabilities!");

// Load the settings from the appsettings.json file.
var settingsContent = File.ReadAllText("appsettings.json");
var settings = JsonDocument.Parse(settingsContent)!;
var username = settings.RootElement.GetProperty("UserName").GetString()!;
var password = settings.RootElement.GetProperty("Password").GetString()!;
var numberOfAppointments = settings.RootElement.GetProperty("NumberOfAppointments").GetInt32();
var alertBeforeDate = settings.RootElement.GetProperty("AlertBeforeDate").GetDateTime();
var refreshFrequencyInSecond = settings.RootElement.GetProperty("RefreshFrequencyInSecond").GetInt32();

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = false
});

// Try load the state from the previous run to avoid the MFA process.
var stateExists = File.Exists("state.json");
var context = stateExists switch
{
    true => await browser.NewContextAsync(new BrowserNewContextOptions
    {
        StorageStatePath = "state.json",
        ScreenSize = new() { Width = 2560, Height = 1440 },
        ViewportSize = new() { Width = 2560, Height = 1440 }
    }),
    false => await browser.NewContextAsync(new BrowserNewContextOptions
    {
        ScreenSize = new() { Width = 2560, Height = 1440 },
        ViewportSize = new() { Width = 2560, Height = 1440 }
    })
};

var page = await context.NewPageAsync();

await page.GotoAsync("https://ttp.cbp.dhs.gov/");
await page.GetByRole(AriaRole.Button, new() { Name = "Log In" }).ClickAsync();
await page.GetByRole(AriaRole.Button, new() { Name = "CONSENT & CONTINUE" }).ClickAsync();

var signInButton = page.GetByRole(AriaRole.Button, new() { Name = "Sign in" });
var dashboardTitle = page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" });
var needSignIn = (await WaitForEitherTask(signInButton.WaitForAsync(), dashboardTitle.WaitForAsync()) == 0) ? true : false;
if (needSignIn)
{
    await SignIn(page, username, password);
    await dashboardTitle.WaitForAsync();
}


// Save the state to avoid the MFA process next time.
await context.StorageStateAsync(new()
{
    Path = "state.json"
});

// I already have an appointment. So, I wait for the reschedule button to show up. 
// You can edit this logic to fit your own situation.
await page.GetByRole(AriaRole.Button, new() { Name = "reschedule" }).ClickAsync();

// Set the number of appointments to the desired number.
await page.GetByLabel("# of appts.").SelectOptionAsync(numberOfAppointments.ToString());
await page.Locator("#centerDetailsUS70").ClickAsync();
await page.GetByRole(AriaRole.Button, new() { Name = "Choose This Location" }).ClickAsync();

await page.GetByLabel("# of appts.").SelectOptionAsync(numberOfAppointments.ToString());

// Keep apply the filter to refresh the availability.
while (true)
{
    await page.GetByRole(AriaRole.Button, new() { Name = "Apply" }).ClickAsync();
    var responseForOne = page.WaitForResponseAsync(new Regex(@"slots\?orderBy=soonest&limit=1"));
    var responseForAll = page.WaitForResponseAsync(new Regex(@"slots\?startTimestamp"));
    await Task.WhenAll(responseForOne, responseForAll);

    var responseContent = await responseForOne.Result.BodyAsync();
    var responseString = Encoding.UTF8.GetString(responseContent);
    var responseJson = JsonSerializer.Deserialize<JsonElement>(responseString);
    var availabaility = responseJson.EnumerateArray().First().GetProperty("startTimestamp").GetDateTime();
    if (availabaility < alertBeforeDate)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        
        // The message box will show up and block the code execution until you close it.
        MessageBox((IntPtr)0, availabaility.ToString(), "It's available", 0);
    }
    Console.WriteLine($"The first availability is on {availabaility}");
    Console.ResetColor();

    await page.WaitForTimeoutAsync(refreshFrequencyInSecond);
    await context.StorageStateAsync(new()
    {
        Path = "state.json"
    });
}

static async Task SignIn(IPage page, string username, string password)
{
    await page.GetByLabel("Email address").ClickAsync();
    await page.GetByLabel("Email address").ClickAsync();
    await page.GetByLabel("Email address").FillAsync(username);
    await page.GetByLabel("Email address").ClickAsync();
    await page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
    await page.GetByLabel("Password", new() { Exact = true }).FillAsync(password);
    await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();
}

static async Task<int> WaitForEitherTask(Task task1, Task task2)
{
    Task completedTask = await Task.WhenAny(task1, task2);

    if (completedTask == task1)
    {
        return 0;
    }
    else
    {
        return 1;
    }
}
