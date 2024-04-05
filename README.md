# NexusScheduler
This tool automatically refreshes the Nexus scheduler to help you find an earlier interview slot.

You might need to change some code to make it work for you. It was implemented pretty quick for my own scenario.

I already have appointment booked. I just want to reschedule it to be sooner if any availibilities. 

So, the code was trying to click the reschedule button to get into the scheduler page.

My interview has been done. So, I am not able to use my account to inspect the page to make the code more general.

 **If you don't have any appointment booked, you might need to change the code for your scenario. Or you could book a slot first, then use this tool.**

This tool is built on [Playwright](https://playwright.dev/dotnet/docs/intro]) for the browser automation. You need to install the required broswer driver before running the tool. You can also go to the Playwright document page to learn how to customize the code for your scenario.

#### Prerequisites
- dotnet sdk. Suggesnt 8.0 or later. Download it from [here](https://dotnet.microsoft.com/en-us/download)

#### How to use the tool
1. Build the project locally by using Visual Studio or running the following command in the root folder:
```
dotnet build
```
There will be a powershell file generated in the output folder. The path should looks like NexusScheduler\Playwright\bin\Debug\net8.0\playwright.ps1. If you are using a different dotnet SDK, the net8.0 folder name will be different The file name is `playwright.ps1`.

2. Install the browser driver by running the following command:
```
playwright.ps1 install
```

#### How to run the tool
1. Open the `appsettings.json` file and update the following fields:
   - `UserName`: Your Nexus username.
   - `Password`: Your Nexus password.
   - `NumberOfAppointments`: The number of appointments you want to book. This is not that important because even there is only one appointment, you probably still want to take it.
   - `AlertBeforeDate`: This could help you ignore the availibilities after this date. If there is a slot available before this date, the tool will popup an alert window and show some text in the console window in green. The date format is `yyyy-MM-dd`.
   - `RefreshFrequencyInSecond`: How many seconds the tool will wait before refreshing the page. The default value is 5 seconds. **I don't suggest to set this value too low because it may cause the tool to be blocked by the Nexus server.**

2. Run the Playwright.exe file in the Playwright output folder(NexusScheduler\Playwright\bin\Debug\net8.0\).
3. The tool will open a browser window and start from signing in by using the credential you configured in the `appsettings.json` file. For the first time signing in, **you will see a page like blow that ask you input a code** from text message to your phone number. Please manually input it. Next time when you run the tool again, this step will be skipped.

![image](https://github.com/yan0lovesha/NexusScheduler/assets/28606510/7b213c11-af54-43cb-a981-bf41a75ef719)

4. After the tool start to refresh the page, you could just let it go and do your own work on your computer. When there is an earlier slot available, the tool will popup an alert window and show some green text in the console window in green.
The popup alert window may be covered by your other windows. But you should see a new icon shows up on your task bar. You need to pay attention to it.

5. Once you saw the alert, you can go to the refreshing browser window and continue book the slot manually.

6. After you booked the slot, you could close the browser window. Update the settings again to book a even ealier slot.
**Do not expect to get a slot on tomorrow from your first try. You should alway try to just get a slot earlier than your current one. Then keep using the tool to get a even earlier slot.**

7. You might need to re-run the tool when you get a chance, because sometimes your credential will be expired after maybe 30 minutes. Even the tool detect a slot, you need to sign-in again to book it. The sign-in process may cause you lost the slot if some other took it earlier than you. I have to sing-in again for 3 times, but eventually I still got the slots after I sign-in again. So, don't be frastrated on re-run the tool. If you have programming knowledge, you can change the code to auto sign-in again every 30 minutes.


