# WiitarThing
Lets you use Wii Guitar Hero guitars on PC with high performance (built with Clone Hero in mind).

Built upon [WiinUSoft and WiinUPro's codebase](https://github.com/keypuncher/wiinupro), but not forked because the changes are too significant and messy. 
All credit for connecting Wiimotes in general and most of the UI goes to Justin Keys.

http://www.wiinupro.com/




# Disclaimer(s)

* This is for traditional **5\-fret Guitar Hero guitars only**; Guitar Hero Live guitars and Rockband guitars use completely different technology that is incompatible with this method.
* This method of connecting Wii guitars **works best on Windows 10**.
* **Third\-party Wiimotes** 99&#37; of the time ***DO NOT WORK AT ALL ON PC***. Some may work if you're extremely extremely lucky, but they most likely will not.
   * **NYKO** Wiimotes ***DO NOT WORK***.

# Introduction

**WiinUSoft** is an open source application which is used for connecting Wii/Wii U peripherals to PC quickly and easily. The only problem with the original software was that it had **no guitar support**. So, I simply **programmed the support in myself**. I also made a few changes to the GUI in order to make it easier to understand.

This modified version of the app, named **WiitarThing**, allows your Wii guitar controller to appear just like an Xbox 360 one as far as Clone Hero is aware.

# Download:

## [Click here to download WiitarThing V2.6.0](https://drive.google.com/file/d/1TgNwR1wsegOkKXeGpu5utC2a0y4Qh1eI/view?usp=drivesdk)

## [Click here to download WiitarThing V2.6.0 \- LIGHT VERSION](https://drive.google.com/file/d/1w3VOM8Ryi8Z2dVzW4jzQpY0CCPOtOO1O/view?usp=drivesdk)

* Note: LIGHT VERSION uses **much less bluetooth bandwidth** at the cost of **no tilt or touch strip support** \(good for people with terribly laggy bluetooth adapters\).

# Source Code:

[https://github.com/Meowmaritus/WiitarThing](https://github.com/Meowmaritus/WiitarThing)

# FIRST TIME SETUP:

* **You must FULLY uninstall HID Wiimote's driver if you previously installed it \(if you don't do this and you've used HID Wiimote in the past, YOU WILL NOT BE ABLE TO USE WIITARTHING\).**

1. *Permanently* Delete \(Shift\+Delete key\)  HID Wiimote's files from your PC
2. *Permanently* Delete \(Shift\+Delete key\) `C:\Windows\System32\drivers\HIDWiimote.sys` if the file is presen**t \(N**ote: Deleting driver files 100&#37; requires super ultra admin privileges to do, so don't be alarmed if User Account Control pops up asking for admin privileges\)
3. Restart your computer

* ***\>\>\>\>Make sure you have the included SCP Driver installed!\<\<\<\<***

1. Run the ScpDriver.exe application included in the folder named SCP\_Driver included with the download.
2. Check ONLY the "Configure Service" checkbox on the bottom left.
3. Click the "Install" button and wait for it to install.

* Choose **DolphinBar**, **Toshiba Bluetooth Stack**, or **Microsoft Bluetooth Stack** instructions below.

# DolphinBar Instructions (100% works on Windows 7, 8, 8.1, and 10):

1. [**Buy a DolphinBar**](https://www.amazon.com/Mayflash-W010-Wireless-Sensor-DolphinBar/dp/B00HZWEB74) if you don't already have one.
2. Plug it into a USB port on your PC.
3. Click the MODE button on the DolphinBar until it goes to MODE 4.
4. If you've previously synced the Wiimote to the DolphinBar, **press 1\+2** to connect it and **skip to step 7**.
5. Press SYNC on the DolphinBar.
6. Press SYNC on the Wiimote.
7. Make sure to **disconnect any XInput\-compatible devices**, including Xbox 360 controllers/guitars, Xbox One controllers, PS3/PS4 controllers, etc.
8. **Close WiitarThing if it was already running**.
9. Run WiitarThing ***AS AN ADMINISTRATOR***.
10. Four Wiimotes will show up in the list on the left \(even if there aren't 4 Wiimotes connected\).
11. Make sure to **disconnect any XInput\-compatible devices**, including Xbox 360 controllers/guitars, Xbox One controllers, PS3/PS4 controllers, etc.
12. Click CONNECT on the **~~first~~** ~~Wiimote listed and connect it to~~ **~~Player 1~~**~~, the~~ **~~second~~** ~~to~~ **~~Player 2~~**~~, etc.~~ Update: Sometimes the Wiimotes are listed way out of order on WiitarThing, so just **click the ID button** on each Wiimote in the list until **the one you want to connect vibrates for second** and click CONNECT on that Wiimote in the list.
13. Click OK on the prompt telling you to press 1\+2 \(on DolphinBar you don't actually need to press 1\+2\).
14. **Move the whammy bar all the way down, then back up.** This needs to be done in order to calibrate the whammy bar.
15. \(Optional\) Re\-connect any XInput\-compatible devices, if desired \(e.g. Xbox 360 guitar for multiplayer\)
16. \(Optional\) See the NOTES section to find out how to calibrate your TILT sensor if it feels off.

# TOSHIBA BLUETOOTH STACK Instructions (100% works on Windows 7, 8, 8.1, and 10)

1. If your bluetooth adapter is **official Toshiba brand**, then **skip to step 3**.
2. For non\-Toshiba bluetooth dongles, you must follow this long list of instructions \(driver test mode required etc\) to install the Toshiba bluetooth drivers and software on **non\-Toshiba adapters**: [http://www.wiinupro.com/tutorials/toshiba\-stack](http://www.wiinupro.com/tutorials/toshiba-stack)
3. Connect the Wiimote via Toshiba bluetooth \(no specific methods required, it just works like any other bluetooth device\)
4. **Close WiitarThing if it was already running**.
5. Run WiitarThing ***AS AN ADMINISTRATOR***.
6. Make sure to **disconnect any XInput\-compatible devices**, including Xbox 360 controllers/guitars, Xbox One controllers, PS3/PS4 controllers, etc.
7. Click the **CONNECT button** on the menu next to the Wiimote you want to use and then choose Player 1, 2, 3, or 4.
8. Press 1\+2 on the Wiimote you want to use and click OK on the prompt telling you to do so.
9. **Move the whammy bar all the way down, then back up.** This needs to be done in order to calibrate the whammy bar.
10. \(Optional\) Re\-connect any XInput\-compatible devices, if desired \(e.g. Xbox 360 guitar for multiplayer\)
11. \(Optional\) See the NOTES section to find out how to calibrate your TILT sensor if it feels off.

# MICROSOFT BLUETOOTH STACK Instructions (Does NOT work on Windows 7, Sometimes doesn't work even on Windows 10):

1. **Close WiitarThing if it was already running**.
2. Run WiitarThing ***AS AN ADMINISTRATOR***.
3. **Plug the Wiimote into the guitar peripheral before continuing**.
4. Click the **REMOVE ALL WIIMOTES** button to make sure your PC has no knowledge of the Wiimotes before continuing \(and **click Yes** when prompted\).
5. Press the **SYNC button** on the **top left corner of WiitarThing**.
6. Press the **red SYNC button** on the inside of the Wiimote's battery cover \(1\+2 also works but your Wiimote might be lazy and turn itself off, cancelling the pairing process, so use SYNC if you have that issue\).
7. **IMPORTANT**: If Windows 10 has a thing popup in the bottom\-right corner of the screen that says "*Tap to set up your Nintendo\-RVL\-CNT\-01*", then you must** disable this option**: [**https://i.imgur.com/bSJYH9M.png**](https://i.imgur.com/bSJYH9M.png)
8. **BE PATIENT**
9. **IF THERE ARE SOME ERRORS, IGNORE THEM**
10. **If the Wiimote's LEDs stop flashing, simply hit 1\+2 or SYNC again.**
11. **Click OK** when the message box pops up telling you that you need to **BE PATIENT** as the drivers install.
12. If it has been **an entire minute** and the Wiimote has not connected, restart the process.
13. Eventually the Wiimote should appear in the list on the left side of the window.
14. Make sure to **disconnect any XInput\-compatible devices**, including Xbox 360 controllers/guitars, Xbox One controllers, PS3/PS4 controllers, etc.
15. Click the **CONNECT button** on the menu next to the Wiimote you want to use and then choose Player 1, 2, 3, or 4.
16. Press 1\+2 on the Wiimote you want to use and click OK on the prompt telling you to do so.
17. **Move the whammy bar all the way down, then back up.** This needs to be done in order to calibrate the whammy bar.
18. \(Optional\) Re\-connect any XInput\-compatible devices, if desired \(e.g. Xbox 360 guitar for multiplayer\)
19. \(Optional\) See the NOTES section to find out how to calibrate your TILT sensor if it feels off.

# Notes

* You can connect up to 4 Wii guitars, following the same instructions for each, but choosing a different player number.
* The SYNC thing only needs to be performed on Wiimotes which have not been connected in this way before.
   * If you have a previously\-connected remote, simply click CONNECT to connect it.
* If you wish to **disconnect your Wiimote** and stop playing click the **DISCONNECT button** on the menu next to the remote you want to disconnect, then hold down the POWER button on the face of the Wiimote for a couple seconds to turn your Wiimote off \(sorry Warriors of Rock guitar users, but you'll have to remove that cover each time you wanna turn it off\).
* You can enable auto\-connect on Wiimotes if you click the properties button on it and choose that option.
* If the star power tilting feels off, try recalibrating the tilt sensor:

1. Lay the guitar flat with **the buttons on top** and press **\(1\)** on the Wiimote.
2. Stand the guitar up with **the neck pointing directly upward** similar to activating star power and press **\(2\)** on the Wiimote.

* You can enable/disable the touch strip by pressing \+/\- on the Wiimote \(not the guitar\), respectively.
* When the touch strip is enabled, you can hold down fret buttons and tap the touch strip to strum, similar to the official GH titles.
* **Wiimotes send data at 100 Hz, in case you ever wondered how responsive Wii guitars were**.
   * PS3 controllers: 100 Hz \(not counting the lag caused by the bad dongles for PS3 guitars\)
   * Xbox 360 controllers: 125 Hz
* This application supports the Classic Controller and Classic Controller Pro extensions as well. When using these, the buttons are mapped to Xbox 360 gamepad buttons.

# Changelog

## Version 2.6.0

* Added **WiitarThing LIGHT VERSION** as a separate download. This version is exactly the same except it uses **much less bluetooth bandwidth** at the cost of **no tilt or touch strip support** \(good for people with terribly laggy bluetooth adapters\).
* Improved joystick sensitivity and made diagonals harder to accidentally hit and easier to intentionally hit \(sounds ridiculous but just believe me\).
* Discovered a **hardware defect** in some guitar controllers \(such as my GH5 guitar\) where **applying too much pressure to the joystick causes it to stop sending joystick data**.
   * In this version of the application, when the defect is triggered and it stops sending data, it **recenters** rather than inputting South\-West.
* Fixed bug where pressing d\-pad on Wiimote itself would have 10 extra milliseconds of input delay.
* Added a lot of debugging features on my debug version \(which I only supply privately when trying to debug problems with others\)

## Version 2.5.0

* ~~\*\*\*Bug where the app randomly closes for no reason should HOPEFULLY be fixed.\*\*\*~~ Doesn't seem to be fixed...?! I can't reproduce the bug so I dunno how to fix it...
* No longer asks for confirmation when clicking the **X** to close the app if there are *no controllers connected*.
* Added new button to open the gamepad test menu \(*joy.cpl*\).
* Moved buttons around and otherwise cleaned up UI.
* **SYNC** window should no longer freak out and repeatedly spit out various errors if your Wiimote is already synced. Instead, it will repeatedly spit out that it is already synced.
* **SYNC** window looks much nicer and important text stands out more than unimportant text.
* **SYNC** window now warns you that leaving it open causes input lag ingame and that you should close it.
* Fixed bug where **SYNC** window output box wouldn't scroll to the bottom as new text is written.
* **REMOVE ALL WIIMOTES** function now displays a simple looping progress bar to show that it is doing stuff.
* **REMOVE ALL WIIMOTES** function now automatically restarts WiitarThing after finishing.
* Various misc. UI changes.

## Version 2.4.0

* Defaults to touch strip DISABLED upon connecting a wiitar. Press \(\+\) on the Wiimote to enable it if you wish to use it \(this is because touch strip support seems to have occasional stray fret button presses; every couple of minutes it tries to press blue for 1 frame sometimes randomly\).
* D\-Pad on Wiimote now acts as a D\-Pad ingame because why not.
* \(A\) button on Wiimote now presses SELECT ingame \(because why not\)

## Version 2.3.0

* WiitarThing no longer minimizes to the tray; it simply minimizes to the taskbar.
* Fixed the *"WiitarThing is already running"* message box being buried underneath other windows when trying to open a second instance of WiitarThing and made it bring WiitarThing into focus *before* telling the user this.
* WiitarThing now asks you to confirm upon trying to exit, saying *"Are you sure you want to close WiitarThing? ALL connected controllers will STOP WORKING!"*, making it more clear to the user that **WiitarThing must be open for the controllers to work** and also preventing the user from accidentally closing WiitarThing on accident.

## Version 2.2.0

* Added **REMOVE ALL WIIMOTES** button. This very forcefully removes all Wiimotes from the Bluetooth Devices list for you.

## Version 2.1.0 (Very Important Update)

* Looks better.
* Some features not useful for Wii guitars specifically have been removed \(such as "profiles"\) etc.
* The "ID" function now vibrates the controller for a much shorter amount of time, reducing the amount of wiimote\-vibrating\-while\-inside\-of\-controller\-noise\-induced cringe.
* Removed space between "Wiitar" and "Thing" in the title bar.
* Changed tray icon to that of WiitarThing \(before it showed WiinUSoft's icon in the tray, oops\)
* Changed the "WiinUSoft is already running!" message to say "WiitarThing is already Running. Press OK to bring the existing WiitarThing instance into focus."

## Version 2.0.3

* Made SYNC window output errors in sentences rather than error codes. If you see an error CODE such as 0x00000105 etc after this update, please let me know so I can add a corresponding sentence to it.

## Version 2.0.2

* Shows up as "WiitarThing" in Task Manager now.

## Version 2.0.1

* Should actually connect now.

## Version 2.0.0

* Changed name of application to WiitarThing.
* ***Made application scan for Wiimotes extremely aggressively and relentlessly, hopefully allowing those of you who were unable connect before to connect now.***
* Moved stuff around the UI, added some prompts, changed some wording, etc. to make it easier to understand and nicer looking.
* Added small credit to original programmer of WiinUSoft on bottom\-right corner of application.
* Note: If this version of the application locks up on you and becomes unresponsive, end the process in Task Manager and re\-open the app. This bug is **very rare** so don't be too concerned if it happens to you once; that's likely the only time it will ever happen.