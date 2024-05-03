# RemoveToRecycleBin for games by Illusion
Plugin for games by Illusion that moves removed/overwritten cards and scenes to the recycle bin to prevent accidentally losing your work. You can restore the overwritten and deleted cards back to the original folder then. This works for all files with .png extension, so cards, scenes, coordinates etc.

All of the different game variants in releases are identical. The plugin might work in other games as well.

### How to use
1. Install latest version of [BepInEx v5.x](https://github.com/BepInEx/BepInEx) (at least v5.4.22).
2. Download the latest release for your game.
3. Place the .dll inside your `BepInEx\plugins` folder. Overwrite if asked.
4. Start character maker, save a new card and then delete it. Check if it appears in your recycle bin.

*WARNING:* Do not rely on this plugin in your normal daily use! It relies on the recycle bin, which sometimes can get cleaned by other applications, or can be briefly inaccessible in which case the card will be removed permanently.
