# WiitarThing

A program that lets you connect Wii Guitar Hero instruments to a Windows PC wirelessly using a wiimote and Bluetooth.

## Table of Contents

- [Setup](#setup)
  - [Install WiitarThing](#install-wiitarthing)
  - [Connect Your Wiimote](#connect-your-wiimote)
  - [Calibrating Guitars](#calibrating-guitars)
- [Features](#features)
  - [Supported Devices](#supported-devices)
  - [Guitar Touch Bar](#guitar-touch-bar)
- [Troubleshooting](#troubleshooting)
  - ["My wiimote doesn't sync!"](#my-wiimote-doesnt-sync)
  - ["My guitar is moving my mouse around!"](#my-guitar-is-moving-my-mouse-around)
- [For Further Assistance](#for-further-assistance)
- [Other Setup](#other-setup)
  - [Using a Dolphinbar](#using-a-dolphinbar)
  - [Using with Guitar Hero 3/Aerosmith/World Tour PC](#using-with-guitar-hero-3aerosmithworld-tour-pc)
- [Credits](#credits)

## Setup

### Install WiitarThing

1. Download and install [ViGEmBus](https://github.com/ViGEm/ViGEmBus/releases).
2. Download WiitarThing from [the "Releases" tab](https://github.com/Meowmaritus/WiitarThing/releases), and extract it into a new folder.

#### A Note About Older Versions

If you're updating from v2.7.0 or earlier, you should uninstall the ScpDriver that it required, as it is no longer necessary. It is not required to be uninstalled however, if you have other programs that use it then you can leave it installed.

### Connect Your Wiimote

1. Start up WiitarThing, then hit the Sync button in the top-left.
2. Sync your wiimote by pressing either the red sync button underneath the battery cover, or both 1+2 at the same time.
   - Be patient during this step, it may take a few tries.
3. Once your wiimote is synced, close the Sync menu, then hit the Connect button on the entry that appears on the left side of the main menu.
4. You can now connect your wiimote extension.

Once your wiimote has been synced, you shouldn't have to sync it again, and can simply power it on and hit Connect. Some Bluetooth receivers don't handle this properly though, and may not correctly save the pairing.

Please note that third-party wiimotes do not work 99% of the time. This is not something WiitarThing can solve, as a majority of third-party wiimotes cut corners and only implement enough of the Bluetooth stack to connect to a Wii, and cannot connect to a PC in any capacity.

### Calibrating Guitars

Guitars must be calibrated before use, otherwise your tilt or whammy may not work correctly. Calibration can be done at any time by simply following these instructions, there is no specific menu you need to go to for it to work.

1. Lay the guitar flat with the frets facing up and neck pointing left, then press the `1` button on the wiimote.
2. Stand the guitar up with the neck pointing directly up, then press the `2` button on the wiimote.
3. Move your whammy bar all the way down and up a few times.
4. Move the joystick around in a few full circles.

## Features

### Supported Devices

The following wiimote extensions are supported:

- Guitar Hero Guitar
- Guitar Hero Drumkit
- DJ Hero Turntable
- Nunchuk
- Classic Controller
- Classic Controller Pro

Wiimotes can also be used standalone, currently with mappings that are intended for using a wiimote like you would a gamepad with Gamepad Mode enabled.

The Wii U Pro Controller is also supported, and can be synced the same way as a wiimote can.

### Guitar Touch Bar

On World Tour/GH5 guitars, the touch bar can be enabled and disabled by pressing the + and - buttons on the wiimote, respectively. When enabled, the touchbar will be mapped to the regular frets.

- Please note that the touchbar is not the best and may be very finnicky! WiitarThing has no ability to fix this, as it just simply takes the data it gets and translates it directly.

## Troubleshooting

### "My wiimote doesn't sync!"

Make sure you do NOT have HID Wiimote installed, as it completely overrides the Wiimote's drivers and makes WiitarThing unable to communicate with them. [Uninstallation instructions may be found here](https://www.julianloehr.de/educational-work/hid-wiimote/) (scroll down to "Uninstall Instructions").

If you do not have HID Wiimote installed, then your Bluetooth receiver is most likely to blame. Some receivers don't play well with wiimotes, and there just isn't anything that WiitarThing can do to fix it. Try to avoid cheaper receivers, and check user reviews before purchasing one.

### "My guitar is moving my mouse around!"

This is most likely being caused by Steam's controller configuration settings. Go to the Settings menu inside Steam and click on the Controller tab, then click the General Controller Settings button and uncheck the Xbox controller configuration support option. If you play a game that requires this setting, then you can enable it for that specific game by going to its properties and enabling Steam Input in the Controller tab.

Alternatively, you can instead remove the right stick mapping inside of the Desktop Configuration menu. By default, Steam's desktop configuration maps the right stick of gamepad controllers to the mouse. Removing this mapping will stop the mouse from being controlled by the guitar.

## For Further Assistance

If you have any questions or issues not addressed in this readme, join the [official Clone Hero server on Discord](https://discordapp.com/invite/Hsn4Cgu) and ask in the `#help-line` channel.

## Other Setup

### Using a Dolphinbar

1. Press the mode button on your Dolphinbar until it goes into mode 4, then sync your wiimote to it.
2. Open WiitarThing. 4 wiimotes will show up on the left, regardless of how many are connected to the Dolphinbar.
3. Click the ID button on each entry until your wiimote vibrates, then click Connect on it.

### Using with Guitar Hero 3/Aerosmith/World Tour PC

WiitarThing will not work directly for the PC versions of Guitar Hero 3, Aerosmith, or World Tour. This is because WiitarThing emulates a standard Xbox 360 *gamepad*, and cannot currently emulate an Xbox 360 *guitar* directly. To work around this, you can use [xinputemu](https://github.com/sanjay900/xinputemu) alongside WiitarThing to make the game see an Xbox 360 guitar instead.

## Credits

WiitarThing is built upon [WiinUSoft and WiinUPro](https://github.com/KeyPuncher/WiinUPro), but not forked because the changes are too significant and messy. All credit for connecting Wiimotes in general and most of the UI goes to [KeyPuncher](https://github.com/KeyPuncher).

Original ViGEmBus support implementation was done by [MWisBest in their fork/issue](https://github.com/Meowmaritus/WiitarThing/issues/9).

All other contributions are logged in the Git history.
