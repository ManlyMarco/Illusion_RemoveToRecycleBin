# RemoveToRecycleBin for Koikatu! / EmotionCreators
Plugin for Koikatu! and EmotionCreators that moves removed/overwritten cards/scenes to the recycle bin to prevent accidentally losing your work. You can restore the overwritten and deleted cards back to the original folder then. This works for all files with .png extension, so cards, scenes, coordinates etc.

[Preview video](https://www.youtube.com/watch?v=FAkZmNJ-1Gs)

### How to use
1. KK version requires latest version of [BepInEx v4](https://github.com/BepInEx/BepInEx) and [BepisPlugins](https://github.com/bbepis/BepisPlugins). EC version requires a latest version of [BepInEx v5](https://github.com/BepInEx/BepInEx) and [EC_CorePlugins](https://github.com/ManlyMarco/EC_CorePlugins).
2. Download the latest release for your game.
3. Place the .dll inside your `BepInEx` folder for KK or `BepInEx\plugins` for EC. Overwrite if asked.
4. Start character maker, save a new card and then delete it. Check if it appears in your recycle bin.

*WARNING:* Do not rely on this plugin in your normal daily use! It relies on the recycle bin, which sometimes can get cleaned by other applications, or can be briefly inaccessible in which case the card will be removed permanently.
