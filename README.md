# BsB2eDriver
### (Bigscreen Beyond 2e Driver)
### ((For eye tracking in Resonite))

This is a **[plugin](https://wiki.resonite.com/Plugins)** for [Resonite](https://resonite.com/) that enables eye
tracking on the Bigscreen Beyond 2e via the official Bigscreen eye tracking software.

## Instructions
> [!IMPORTANT]  
> This is a *Plugin* for Resonite, NOT a mod. Using this won't prevent you from joining other sessions.

0. Prerequisite: [Install and set up the Beyond 2e's Native Eyetracking software](https://store.bigscreenvr.com/blogs/beyond/beyond-2e-eyetracking-setup-guide-with-vrchat).
1. [Download the latest version](https://github.com/zenuru-merps/ResoniteBeyond2eDriver/releases/latest) of the plugin, and place it in the `Libraries` folder within your Resonitezinstallation directory.
2. Modify Resonite's launch arguments in Steam and append the following: `-LoadAssembly Libraries/BsB2eDriver.dll`
3. Before starting Resonite, ensure the Beyond Eyetracking software is running, and __enable the VRCFT recipient checkbox.__
> [!NOTE]
> VRCFT is not needed for this plugin to work. The VRCFT data stream from Beyond Eyetracking is read directly by this plugin.
4. Launch Resonite. If the eye tracking data stream can be read, then eye tracking should start automatically.
> [!TIP]
> If tracking does not start, you'll need to enable it within the Resonite settings tab under the Devices category.
> ![Screenshot of settings](https://github.com/user-attachments/assets/d9e049ee-17f8-4fbf-b4a5-03caae0159be)

> [!WARNING]  
> If you are using Gale/r2modman/Thunderstore to launch Resonite, you must still follow the directions above. Do not
> attempt to add the launch arguments in your mod manager, they must be added to Steam launch arguments. The mod manager
> should still pick up the arguments from Steam and use them to load the plugin.

## How does this work?
The Beyond Eyetracking application exposes a shared memory data structure that is intended to be read by the
[VRCFT-Beyond module](https://github.com/BigscreenVR/VRCFT-Beyond) for VRCFT. Since the data format published in that shared memory is very similar to the data
needed by FrooxEngine for eye tracking, this plugin can read the data directly from the shared memory and turn it into a
source of eye tracking data for Resonite.

## Tracking quality is poor/eye openness isn't working
This is a limitation of using Bigscreen's native software and eyetracking model. The plugin does no additional
processing to the data received from the Bigscreen Eyetracking application, besides converting it into a format that is
understood by FrooxEngine.

You can complain about this to Bigscreen and hope it improves, or you may use other community-maintained eyetracking
applications such as [Baballonia](https://github.com/Project-Babble/Baballonia); Many users have reported that third-party applications produce better results.

## But why a plugin?
Because I believe that plugins are more appropriate than mods for hardware integration. Dunno, might turn this into a
mod later.

## Special thanks
Thank you to Talii and AuroraDerg for beta-testing this plugin.