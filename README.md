# About WiitarThing
WiitarThing lets you use Wii Guitar Hero guitars (a.k.a. "Wiitars") and Wii Guitar Hero drums (a.k.a. "Wiidrums") **wirelessly** on Windows PCs with high performance (built with Clone Hero in mind). 
To use Wiitars or Wiidrums with a wired connection on your PC without a Wiimote or bluetooth, simply order the [Raphnet adapter](https://www.raphnet-tech.com/products/wusbmote_1player_adapter_v3/index.php) and plug it in to your PC. This software is not needed in that case.

Built upon [WiinUSoft and WiinUPro's codebase](https://github.com/keypuncher/wiinupro), but not forked because the changes are too significant and messy. 
All credit for connecting Wiimotes in general and most of the UI goes to Justin Keys.

http://www.wiinupro.com/

# SETUP GUIDE

### Introductory Notes
* This tool is ONLY for connecting Wiitars and Wiidrums **wirelessly** on **Windows PCs**. ***This tool does NOT apply to Mac or Linux or for connecting wii remotes over a wired connection.***
* A Wiimote is required for this method of connection. Plug the guitar into the Wiimote in the same way you would connect a Nunchuck or Classic Controller (Pro).
* This way of connecting instruments works best on Windows 10 using the Microsoft bluetooth stack, though it is possible on windows 7 (but only very rarely works).
* Third-party Wiimotes are not recommended *as they do not work 99% of the time.*
  * This is because they cut corners, only providing the data packets that an actual Wii console needs to interact with the Wii Remote. On the other hand, official Wii Remotes go the full way providing everything a PC needs to connect to them, despite the Wii not needing any of that functionality.
* This tutorial is for 5-fret Guitar Hero guitars and Guitar Hero drums only. Rock band instruments use different methods of connecting to your computer and will not with using this method.
  * For Wii Rock Band guitars or Wii Rock Band drums, you connect them with the USB dongle in the exact same manner as a PS3 Rock Band guitar. None of this nonsense is needed.

* You can connect up to 4 Wii instruments using this method.
* WiitarThing will not work out of the box for the Guitar Hero 3, Guitar Hero: Aerosmith, or Guitar Hero: World Tour games for PC.
  * This is because WiitarThing makes the guitar show up as a **regular Xbox 360 gamepad**. These official PC releases of these games expect **an Xbox 360 gamepad flagged as a guitar**.
  * To get around this, you can use [x360ce](https://www.x360ce.com/) to emulate a guitar-flagged controller.
  * I have tried to make WiitarThing flag the virtual Xbox 360 gamepad as a guitar but I wasn't able to figure out how.

### Getting Started
* **1.** Remove conflicting software.
  * **1.1.** Make sure you do NOT have HIDWiimote installed as it completely overrides the Wiimote's drivers and makes WiitarThing unable to communicate with them. [HIDWiimote Uninstallation Instructions](https://www.julianloehr.de/educational-work/hid-wiimote/)  (scroll down to "Uninstall Instructions")
  * **1.2.** Make sure you do NOT have vJoy installed as it may interfere with the virtual gamepads WiitarThing creates to send inputs to Clone Hero. [vJoy Uninstallation Instructions](http://vjoystick.sourceforge.net/site/index.php/77-vjoy/102-removing-vjoy)
  * **1.3.** Ok so you don't need to *remove* Steam, but make sure that Xbox controller configuration is DISABLED:
     * **1.3.1.** Go to the main `Settings` screen on Steam.
     * **1.3.2.** Go to the `Controller` tab.
     * **1.3.3.** Click the `GENERAL CONTROLLER SETTINGS` button.
     * **1.3.4.** **UNCHECK** the `Xbox Configuration Support` checkbox.
     * **1.3.5.** **UNCHECK** the `Generic Gamepad Configuration Support` checkbox.
     
* **2.** Setup the required virtual Xbox gamepad driver.
  * **2.1.** Download the [SCP Driver](https://github.com/Meowmaritus/WiitarThing/releases/download/v2.7/WiitarThing_SCP_Driver.zip).
  * **2.2.** Extract it.
  * **2.3.** Run the ScpDriver.exe program **as an administrator**.
  * **2.4.** **CHECK** the `Configure Service` checkbox at the bottom.
  * **2.5.** **UNCHECK** the `Bluetooth Driver` checkbox at the bottom.
  * **2.6.** **CHECK** the `Force Install` checkbox at the bottom.
  * **2.7.** Click the "Install" button and wait for installation to finish.
* **3.** Get WiitarThing
  * **3.1.** Visit [the "Releases" tab](https://github.com/Meowmaritus/WiitarThing/releases). 
  * **3.2.** Download the `WiitarThing.<version>.zip` file for the latest version released.
  * **3.3.** Extract the ZIP anywhere.

### Connecting Wiitars and Wiidrums (WITH A STANDARD BLUETOOTH ADAPTER WITH MICROSOFT BLUETOOTH STACK)
* **1.** Open WiitarThing *AS AN ADMINISTRATOR.*
* **2.** Plug the Wiimote into the peripheral before continuing.
* **3.** Click the REMOVE ALL WIIMOTES button to make sure you do not have any Wiimotes connected to your computer (click yes if prompted)
* **4.** Press the SYNC Button in the top left corner of WiitarThing
* **5.** Press the red SYNC button on the inside of your Wiimote's battery cover (1+2 may also work if this is giving you issues.)
* **6.** **IMPORTANT**: If Windows 10 has a popup in the bottom-right corner of the screen that says "Tap to set up your Nintendo-RVL-CNT-01", then you must [disable this option](https://i.imgur.com/bSJYH9M.png).
* **7.** Be Patient. Some errors may display in Wiitarthing while connecting, ignore them.
* **8.** If your Wiimote's LEDs stop flashing, simply hit the red SYNC button again (or 1+2)
* **9.** Click OK when the message box pops up telling you that you need to BE PATIENT as the drivers install.
* **10.** If it has been an entire minute and the Wiimote has not connected, press "Refresh connected device list". If it still doesn't show up, restart the program and connection process.
* **11.** Eventually the Wiimote should appear in the list on the left side of the window.
* **12.** Before continuing disconnect any XInput-compatible devices such as Xbox 360 controllers/guitars, Xbox One controllers, PS3/PS4 controllers, etc.
* **13.** Click the CONNECT button on the menu next to the Wiimote you want to use and then choose Player 1, 2, 3, or 4.
* **14.** Press 1+2 on the Wiimote you want to use and click OK on th prompt telling you to do so.
* **15. If you wish, you may now reconnect any XInput-compatible devices like Xbox 360 guitars for multiplayer.
* The SYNC only needs to be performed on Wiimotes which have not been connected in this way before. If you have a previous connected Wiimote, simply click CONNECT and press 1+2 on the Wiimote to connect it. If your Wiimote does not show up in the left side, pressing 1+2 may cause it to appear.

### Connecting Wiitars (WITH A DOLPHINBAR)
* **This method works flawlessly on Windows 10 and may work well on Windows 7/8/8.1 but has not been tested much on those operating systems. This method was not tested for Wiidrums.**
* **1.** Buy a DolphinBar [here](https://www.amazon.com/Mayflash-W010-Wireless-Sensor-DolphinBar/dp/B00HZWEB74) if you don't already have one.
* **2.** Plug the DolphinBar into a USB port on your PC and click the MODE button on the DolphinBar until it goes to MODE 4.
* **3.** Make sure to disconnect any XInput-compatible devices, including Xbox 360 controllers/guitars, Xbox One controllers, PS3/PS4 controllers, etc.
* **5.** If you have NOT previously synced the Wiimote to the DolphinBar, press SYNC on the DolphinBar, then press SYNC on your Wiimote. If you have, then skip this step.
* **6.** Close WiitarThing if it was already running.
* **7.** Run WiitarThing AS AN ADMINISTRATOR.
* **8.** 4 Wiimotes will show up in the list on the left (even if there aren't 4 Wiimotes connected.)
* **9.** Click the ID button on each Wiimote in the list until the one you want to connect vibrates for a second and click CONNECT on that Wiimote in the list.
* **10.** Click OK on the prompt telling you to press 1+2 (on DolphinBar you don't actually need to press 1+2)
* **11.** If you wish, you may now reconnect any XInput-compatible devices like Xbox 360 guitars for multiplayer.
* The SYNC only needs to be performed on Wiimotes which have not been connected in this way before. If you have a previous connected Wiimote, simply click CONNECT and press 1+2 on the Wiimote to connect it. If your Wiimote does not show up in the left side, pressing 1+2 may cause it to appear.

### Connecting Wiitars (WITH A STANDARD BLUETOOTH ADAPTER USING THE TOSHIBA BLUETOOTH STACK)
* **Not reccommended except as a last resort as it does not work on Windows 10 and often doesn't work on Windows 7 for no reason. This method was not tested for Wiidrums.**
* **1.** If your bluetooth adapter is official Toshiba brand, then skip to step 3.
* **2.** For non-Toshiba bluetooth dongles, you must follow [**this long list of instructions**](http://www.wiinupro.com/tutorials/toshiba-stack) (driver test mode required etc) to install the Toshiba bluetooth drivers and software on non-Toshiba adapters.
* **3.** Connect the Wiimote via Toshiba bluetooth (no specific methods required, it just works like any other bluetooth device)
* **4.** Close WiitarThing if it was already running.
* **5.** Run WiitarThing AS AN ADMINISTRATOR.
* **6.** Make sure to disconnect any XInput-compatible devices, including Xbox 360 controllers/guitars, Xbox One controllers, PS3/PS4 controllers, etc.
* **7.** Click the CONNECT button on the menu next to the Wiimote you want to use and then choose Player 1, 2, 3, or 4.
* **8.** Press 1+2 on the Wiimote you want to use and click OK on the prompt telling you to do so.
* **9.** Move the whammy bar all the way down, then back up. This needs to be done in order to calibrate the whammy bar.
* **10.** Lay the guitar flat with the buttons on top and press 1 on the Wiimote, then stand the guitar up with the neck pointing directly upward similar to activating star power and press 2 on the Wiimote. This will calibrate tilt.
* **11.** Re-connect any XInput-compatible devices, if desired (e.g. Xbox 360 guitar for multiplayer)

### Calibrating Your Wiitar Before Playing
* This must be done because Wiimotes and Wiitars all have their own unique value ranges that cannot be programmed into the app itself.
* **1.** Calibrate the Whammy Bar range, by moving the Whammy Bar all the way down, then back up.
* **2.** Calibrate the tilt functionality.
  * **2.1.** Lay the guitar flat with the frets on top and the neck pointing to your left.
  * **2.2.** Press the `1` button on the Wiimote.
  * **2.3.** Stand the guitar up with the neck pointing directly upward and the frets in front, facing toward the screen, sort of like you're activating Star Power.
  * **2.4.** Press the `2` button on the Wiimote.
* **3.** Calibrate the joystick range by pressing it all the way toward the outer edge, then sliding it all along the outer edge in one circular motion, like walking in a circle in most third person video games.

### Closing Notes
* If you wish to disconnect your Wiimote and stop playing, click the DISCONNECT button on the menu next to the remote you want to disconnect, then hold down the POWER button on the face of the Wiimote for a couple seconds to turn off your Wiimote.
  * Be careful not to bump the wiitar as the B button will get pressed onto the backplate, turning your Wiimote back on and draining your battery very stupidly.
* You can enable auto-connect on Wiimotes if you click the properties button on it and choose that option.
  * Auto-connect *might* only work on the first 3 Wiimotes listed. More testing needs to be done and I do not have the necessary amount of Wiimotes.
* Wiimotes send data at around 100Hz, in case you were wondering how responsive Wiitars are with this method.
  * A bad bluetooth adapter or bluetooth signal interference (way more of a problem than you might realize!) may cause packets to be dropped resulting in less than 100 updates per second actually reaching WiitarThing.
* This application supports the Classic Controller and Classic Controller Pro extensions as well. When using these, the buttons are mapped to the corresponding Xbox 360 gamepad buttons.
* **The touch bar is not supported in this application because the Wii touch bars use even worse technology than the Xbox 360 touch bars and nobody likes them and Clone Hero doesn't support them.**
* The tilt functionality actually comes **from the Wiimote** on Wiitars so changing Wiimotes will actually change your tilt sensitivity and responsiveness as Wiimote accelerometers are very inconsistently produced.
* This application maps Wiidrums orange and green pads to a same Xbox 360 gamepad button, since Clone Hero currently only supports 4-lane drums. If Clone Hero adds Rock Band Pro Drums support, or Guitar Hero 5-lane drums support, this application will we updated to accommodate to those changes.

### Getting More Help If Needed
Consult the `#help-line` channel in the [official Clone Hero server on Discord](https://discordapp.com/invite/Hsn4Cgu) if you need help following the instructions, advice on what to buy, or any other questions.