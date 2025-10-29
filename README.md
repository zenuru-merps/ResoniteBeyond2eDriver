# BsB2eDriver
### (Bigscreen Beyond 2e Driver)
### ((For eye tracking in Resonite))

This is a **[plugin](https://wiki.resonite.com/Plugins)** for [Resonite](https://resonite.com/) that enables eye
tracking on the Bigscreen Beyond 2e via the official Bigscreen eye tracking software.

## Instructions
> [!IMPORTANT]  
> This is a *Plugin* for Resonite, NOT a mod. Using this won't prevent you from joining other sessions.

0. Prerequisite: [Install and set up the Beyond 2e's Native Eyetracking software](https://store.bigscreenvr.com/blogs/beyond/beyond-2e-eyetracking-setup-guide-with-vrchat).
1. [Download the latest version](https://github.com/zenuru-merps/ResoniteBeyond2eDriver/releases/latest) of the plugin, and place it in the `Libraries` folder within your Resonite installation directory.
2. Modify Resonite's launch arguments in Steam and append the following: `-LoadAssembly Libraries/BsB2eDriver.dll`
3. Before starting Resonite, ensure the Beyond Eyetracking software is running, and __enable the VRCFT recipient checkbox.__
> [!NOTE]
> VRCFT is not needed for this plugin to work. The VRCFT data stream from Beyond Eyetracking is read directly by this plugin.
4. Launch Resonite. If the eye tracking data stream can be read, then eye tracking should start automatically.
> [!TIP]
> If tracking does not start, you'll need to enable it within the Resonite settings tab under the Devices category.
> ![Screenshot of settings](https://github.com/user-attachments/assets/7eb41071-2503-49e6-b975-b37ba0a0814a)

## How does this work?
The Beyond Eyetracking application exposes a shared memory data structure that is intended to be read by the
[VRCFT-Beyond module](https://github.com/BigscreenVR/VRCFT-Beyond) for VRCFT. Since the data format published in that
shared memory is very similar to the data needed by FrooxEngine for eye tracking, this plugin can read the data directly
from the shared memory and turn it into a source of eye tracking data for Resonite.

## But why a plugin?
Because I believe that plugins are more appropriate than mods for hardware integration. Dunno, might turn this into a
mod later.

## Special thanks
Thank you to Talii and AuroraDerg for beta-testing this plugin.