# MoreCheckmarks v2.1.0 - SPT 4.0 Update

> ⚠️ **Note:** This is a complete port to SPT 4.0. As with any major update, there may be bugs or unexpected behavior. Please be patient while we iron out any kinks, and don't hesitate to report any issues you encounter!

> **⚠️ Important:** Version 2.1.0 is tested with SPT version 4.0.11. It may not work with other 4.0 versions. It will **definitely not** work with SPT versions prior to 4.0.0.

---

## Known Issues

- ~~On brand new profiles, checkmarks may not appear until you accept a quest; a game restart after accepting can also be required.~~ - Should be fixed in version 2.0.1 !
- ~~Barters are not being shown in checkmarks/tooltips~~ - Should be fixed in version 2.1.0 !

---

## Overview

A mod for SPT that shows colored checkmarks on items needed for hideout upgrades, quests, wishlist, barters, and crafting recipes.

The tooltip displays detailed information about what each item is needed for, including counts and specific module/quest/recipe names.

---

## Features

### Checkmark Colors

- **RED** - Item is needed for hideout but you need **more** to fulfill the requirement of at least one module
- **GREEN** - Item is needed for hideout and you have **enough** to fulfill the requirements (behavior configurable - see `Fulfilled Any Can Be Upgraded` setting)
- **YELLOW** - Item is needed for a quest/task
- **BLUE** - Item is on your wishlist
- **MAGENTA** - Item is needed for a trade/barter
- **CYAN** - Item is needed for a crafting recipe

All colors are fully customizable in the config.

### Tooltip Information

- Shows "found in raid count" / "total count in stash"
- Individual color coding for each module (green, red, or blue) based on whether you have enough for that specific module
- Shows counts: (current count in stash / total count needed by all modules)
- Lists all quests, hideout modules, barters, and crafts the item is needed for

### "Take" Action Color Coding

If the "Take" option is available when picking up loose loot, it will be color coded to match the checkmark system.

### Quest Item Handling

If an item is needed for a quest or is found in raid (so would already have a checkmark), it will show the appropriate color but still display the quest information in the tooltip.

---

## What's New in v2.0.0

### 🎮 F12 In-Game Configuration Menu

All settings are now accessible through BepInEx's F12 configuration menu! No more editing config files manually.

- **Live color pickers** with RGB sliders
- **Organized categories**: Hideout, Quests, Barter & Craft, Priority, Colors
- **Hoverable descriptions** explaining each setting
- Changes apply after switching menus (e.g., leave stash → main menu → return)

### 🎯 Quest Prerequisite Display

When enabled, the tooltip now shows how many prerequisite quests you need to complete before each quest becomes available:

- **Color-coded status**:
  - 🟢 **Green** `(0 prereqs)` - Quest is available now
  - 🟡 **Yellow** `(1-9 prereqs)` - Quest is close to being unlocked
  - ⚪ **Gray** `(10+ prereqs)` - Quest is far away (e.g., Collector)
- **Smart sorting** - Quests are sorted by prerequisite count, so items needed for soon-to-be-available quests appear first
- Can be disabled in settings if you prefer the classic view

---

## Installation

1. Download and extract the zip file into your SPT game folder
2. You should end up with:
   - `BepInEx/plugins/MoreCheckmarks/MoreCheckmarks.dll`
   - `BepInEx/plugins/MoreCheckmarks/MoreCheckmarksAssets`
   - `SPT/user/mods/MoreCheckmarksBackend/MoreCheckmarksBackend.dll`

---

## Configuration

Press **F12** in-game to access all settings. Below are the available options:

### Hideout Settings

| Setting                           | Description                                                                                                                                                                   |
| --------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Fulfilled Any Can Be Upgraded** | When TRUE, shows fulfilled checkmark when AT LEAST ONE hideout module can be upgraded. When FALSE, shows fulfilled only when ALL modules requiring this item can be upgraded. |
| **Show Future Module Levels**     | Show requirements for future hideout module levels instead of only the next one.                                                                                              |

### Quest Settings

| Setting                     | Description                                                                                                       |
| --------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| **Include Future Quests**   | Consider future quests when checking which quests an item is required for. If false, behaves like vanilla.        |
| **Show Prerequisite Count** | Show the number of prerequisite quests needed before each quest becomes available, with color coding and sorting. |

### Barter & Craft Settings

| Setting               | Description                                                              |
| --------------------- | ------------------------------------------------------------------------ |
| **Show Barter**       | Show checkmark and tooltip for barter/trades this item is needed for.    |
| **Show Craft**        | Show checkmark and tooltip for crafting recipes this item is needed for. |
| **Show Future Craft** | Show crafting recipes for hideout areas of higher level than current.    |

### Priority Settings

These settings decide which checkmark color to display when an item is needed for multiple things. Higher number = higher priority.

| Setting               | Description                      |
| --------------------- | -------------------------------- |
| **Quest Priority**    | Priority for quest checkmarks    |
| **Hideout Priority**  | Priority for hideout checkmarks  |
| **Wishlist Priority** | Priority for wishlist checkmarks |
| **Barter Priority**   | Priority for barter checkmarks   |
| **Craft Priority**    | Priority for craft checkmarks    |

### Color Settings

All colors can be customized using RGB sliders. Default colors:

| Setting             | Default     |
| ------------------- | ----------- |
| **Need More Color** | Light Red   |
| **Fulfilled Color** | Light Green |
| **Wishlist Color**  | Light Blue  |
| **Barter Color**    | Magenta     |
| **Craft Color**     | Cyan        |

---

## Compatibility

- **SPT Version**: 4.0.x
- **Required**: BepInEx (included with SPT)

---

## Credits

Original mod by **TommySoucy**. SPT 4.0 port and new features by TommySoucy & Bewa.

---

## Changelog

### v2.1.0

- Fixed barter checkmarks not displaying (server was returning empty data)
- Added "Show Quest Checkmarks for Non-FIR Items" option in F12 menu (Quests section) - enables quest checkmarks on items that aren't found in raid, useful for SPT configurations that accept non-FIR items for quest turn-ins

### v2.0.0

- Full port to SPT 4.0
- Added F12 in-game configuration menu (replaces Config.json)
- Added quest prerequisite count display with color coding
- Added smart sorting of quests by prerequisite count
- Fixed quest filtering to properly include future quests
- Various bug fixes and improvements
