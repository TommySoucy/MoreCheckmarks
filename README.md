# MoreCheckmarks v2.3.0 - SPT 4.0 Update

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

_Almost_ all settings are now accessible through BepInEx's F12 configuration menu! No more editing config files manually.

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
| **Show Hideout Checkmarks**       | Show checkmark and tooltip for hideout areas this item is needed for. Default on. When disabled, no hideout checkmark or "Needed for area" tooltip section is shown.         |
| **Fulfilled Any Can Be Upgraded** | When TRUE, shows fulfilled checkmark when AT LEAST ONE hideout module can be upgraded. When FALSE, shows fulfilled only when ALL modules requiring this item can be upgraded. |
| **Show Future Module Levels**     | Show requirements for future hideout module levels instead of only the next one.                                                                                              |
| **Only Show Hideout Checkmark On FIR Items** | When enabled, hideout needs only drive the checkmark for Found In Raid (FIR) items. Non-FIR items get no hideout checkmark (quests, wishlist, barters, and crafts can still show one). The "Needed for area" tooltip lines are still shown. |

### Quest Settings

| Setting                                  | Description                                                                                                                                                                                                                                                                                            |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Show Quest Checkmarks**                | Show checkmark and tooltip for quests this item is needed for. Default on. When disabled, no quest checkmark or quest tooltip section is shown.                                                                                                                                                          |
| **Include Future Quests**                | Consider future quests when checking which quests an item is required for. If false, behaves like vanilla.                                                                                                                                                                                              |
| **Show Prerequisite Count**              | Show the number of prerequisite quests needed before each quest becomes available, with color coding and sorting.                                                                                                                                                                                       |
| **Show Quest Checkmarks for Non-FIR Items** | When enabled, quest checkmarks appear on items even if they aren't found in raid. Useful if your SPT is configured to accept non-FIR items for quest turn-ins. This is about whether *your stored item* is FIR.                                                                                       |
| **Only Show FiR-Required Quests**        | When enabled, quest checkmarks only appear for quests that **require** the item to be Found In Raid. Quests that accept non-FIR items (e.g. Ragman's *Hot Delivery*) won't show a quest checkmark. This is about whether the **quest** requires FIR — separate from the option above. Default off; hardcore players should leave it off. |

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

### Server Configuration (`config.json`)

Some quest-hiding options live in a server-side config file rather than the F12 menu, since they require server data. The file is created automatically with defaults on first server start at:

`SPT/user/mods/MoreCheckmarksBackend/config.json`

| Setting                    | Default | Description                                                                                                                                                                  |
| -------------------------- | ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`hideInactiveEventQuests`** | `true`  | Hide checkmarks for inactive seasonal/event quests (Christmas, Halloween, etc.) that aren't currently active. Set to `false` to show them like before.                       |
| **`excludedQuestIds`**     | `[]`    | A list of quest IDs to completely ignore (no checkmarks). Useful for quests you never do (e.g. *Compensation For Damage*).                                                    |

**Finding quest IDs:** On every server start, the mod writes a readable lookup file next to the config at `SPT/user/mods/MoreCheckmarksBackend/quest-id-reference.txt`, listing every quest as `Quest Name [Trader] = questId`. Copy the IDs you want to hide into `excludedQuestIds`, for example:

```json
{
  "hideInactiveEventQuests": true,
  "excludedQuestIds": ["5c0bd94186f7747a727f09b2"]
}
```

Changes to `config.json` take effect after a server restart.

---

## Compatibility

- **SPT Version**: 4.0.x
- **Required**: BepInEx (included with SPT)

---

## Credits

Original mod by **TommySoucy**. SPT 4.0 port and new features by TommySoucy & Bewa.

---

## Changelog

### v2.3.0

- Stash tooltip counts are now live - moving an item updates the count on the next hover instead of needing a menu switch. Previously the count read a cached snapshot.
- Added an "ON YOU" tooltip line showing how many of an item you're carrying (equipment/pockets/secure container). Shown only when you have one or more on you; otherwise the tooltip is unchanged.
- Known limitation: in-raid, already-displayed checkmarks/counts don't refresh until you reopen the inventory (the newly looted item shows the right count, existing ones update on reopen). Out of raid is unaffected.

### v2.2.0

- The "Needed for area" tooltip count is now FIR-aware: when "Only Show Hideout Checkmark On FIR Items" is enabled, the possessed number reflects only your Found In Raid items instead of your total count.
- Clearer STASH tooltip wording - now reads "X found in raid / Y total" instead of "X/Y".
- Added "Show Quest Checkmarks" option (Quests section) - toggle quest checkmarks and the quest tooltip section on or off. Default on.
- Added "Show Hideout Checkmarks" option (Hideout section) - toggle hideout checkmarks and the "Needed for area" tooltip section on or off. Default on.
- Added "Only Show Hideout Checkmark On FIR Items" option (Hideout section) - hideout needs only drive the checkmark for Found In Raid items; non-FIR items get no hideout checkmark (other reasons still apply). Default on.
- Added "Only Show FiR-Required Quests" option (Quests section) - hides quest checkmarks for items whose quest doesn't require Found In Raid (e.g. Ragman's Hot Delivery). Default off, so existing behavior is unchanged unless enabled.
- Added server-side `config.json` with `hideInactiveEventQuests` (default on) to hide inactive seasonal/event quests, and `excludedQuestIds` to hide specific quests you never do. A `quest-id-reference.txt` lookup file is generated on server start to help find quest IDs for the exclusion list.

### V2.1.1
- Refactor codebase to improve maintainability.

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
