# More Checkmarks

A mod for SPT-AKI that shows a colored checkmark on items needed to upgrade hideout modules and items that are on the player's wish list.
Also shows a tooltip with the list of modules the item is needed for when cursor hovers over the checkmark.
If the item is also needed for a quest or is found in raid (so should already have a checkmark) it will have the different color but will still show found in raid or the quest in the tooltip.

- A **_RED_** checkmark means the item is needed in hideout but more is needed to fulfill the requirement of at least one of the modules you need it for.
- A **_GREEN_** checkmark means the item is needed in hideout and fulfills the required amount of all the modules that need it. Note that this does not mean you have enough to upgrade all of the modules that need it. Say if you need 2 wires to upgrade a module and 3 wires to upgrade another, and you have 3 wires in total inside your stash, the checkmark will be blue but if you upgrade one of the modules, you wont have enough to upgrade the other and the checkmark will go back to red. This though, can now be changed in the config using the fulfilledAnyCanBeUpgraded setting. See config section below.
- A **_BLUE_** checkmark means the item is on the wish list.
- A **_MAGENTA_** checkmark means the item is needed for a trade/barter.
- The tooltip will show individual color for each module, green, red, or blue, depending on whether you have enough of the item for the specific module or if it's on the wish list.
- The tooltip will first show "found in raid cound"/"total count in stash"
- The tooltip will also show the counts for specific hideout area requirements: (current count of the item in the stash / total count needed by all modules)
- If the "Take" option is available when trying to pick up loose loot, it will be color coded as well.
- The colors can be changed in the config. The ones above are default.

![alt text](https://github.com/TommySoucy/MoreCheckmarks/blob/main/hub/example0.png "Example")
![alt text](https://github.com/TommySoucy/MoreCheckmarks/blob/main/hub/example1.png "Example")
![alt text](https://github.com/TommySoucy/MoreCheckmarks/blob/main/hub/example2.png "Example")
![alt text](https://github.com/TommySoucy/MoreCheckmarks/blob/main/hub/example3.png "Example")
![alt text](https://github.com/TommySoucy/MoreCheckmarks/blob/main/hub/example4.png "Example")

## Installation

1. Download latest from [releases](https://github.com/TommySoucy/MoreCheckmarks/releases)
2. Download and install the latest (**_not pre-release_**) version of [BepinEx](https://github.com/BepInEx/BepInEx/releases)
3. Extract zip file into the game folder (Inside you game folder you should end up with /BepinEx/plugins/MoreCheckmarks and /user/mods/MoreCheckmarksBackend)

## Config

All settings are accessible via the **BepInEx F12 in-game menu**. Press F12 while in-game to open the configuration manager and find "MoreCheckmarks" in the list.

**Note:** Changes apply after switching menus (e.g., go to main menu, then back to stash).

### Hideout Settings

- **_Fulfilled Any Can Be Upgraded_**: When **true**, shows fulfilled checkmark when AT LEAST ONE hideout module can be upgraded. When **false**, shows fulfilled only when ALL modules can be upgraded.

- **_Show Future Modules Levels_**: Show requirements for future hideout module levels instead of only the next one.

### Quest Settings

- **_Include Future Quests_**: Consider future quests when checking which quests an item is required for. If false, behaves like vanilla.

- **_Show Prerequisite Count_**: Show the number of prerequisite quests needed before each quest becomes available. Quests are sorted by prerequisite count with color coding:
  - **Green** `(0 prereqs)` - Quest is available now
  - **Yellow** `(1-9 prereqs)` - Quest is close to being available
  - **Gray** `(10+ prereqs)` - Quest is far away
    Set to **false** to disable and show only quest names.

### Barter & Craft Settings

- **_Show Barter_**: Show checkmark and tooltip for barters/trades this item is needed for.

- **_Show Craft_**: Show checkmark and tooltip for crafting recipes this item is needed for.

- **_Show Future Craft_**: Show crafting recipes that are not yet unlocked.

### Priority Settings

These determine which checkmark color takes precedence when an item is needed for multiple things. Higher number = higher priority (range 0-10).

- **_Quest Priority_** (default: 4)
- **_Hideout Priority_** (default: 3)
- **_Wishlist Priority_** (default: 2)
- **_Barter Priority_** (default: 1)
- **_Craft Priority_** (default: 0)

### Color Settings

All colors can be customized using RGB sliders in the F12 menu:

- **_Need More Color_** - Light red by default, appears when you need more of an item
- **_Fulfilled Color_** - Light green by default, appears when requirements are fulfilled
- **_Wishlist Color_** - Light blue by default, for wishlist items
- **_Barter Color_** - Magenta by default, for barter items
- **_Craft Color_** - Cyan by default, for craft items

## Used libraries

- Harmony
- Bepinex
