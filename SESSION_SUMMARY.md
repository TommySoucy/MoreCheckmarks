# MoreCheckmarks SPT 4.0 Fix - Session Summary

**Date:** November 30, 2025  
**Goal:** Fix quest checkmarks not appearing in tooltips for SPT 4.0

## Problem Description

The MoreCheckmarks mod was partially ported to SPT 4.0, but quest/task checkmarks were not working. Items needed for quests were not showing the quest name in the tooltip.

## Root Causes Identified

### Bug #1: Array Append Doesn't Work

In `Server/MoreCheckmarksBackend.cs`, the code was using:

```csharp
Quest[] quests = [];
// ...
_ = quests.Append(quest);  // BROKEN: Returns new IEnumerable, doesn't modify original
```

This resulted in an empty array always being returned.

### Bug #2: Incorrect Quest Filtering Logic

The original logic only included quests with status `AvailableForStart`, `Started`, or `AvailableForFinish`. This excluded `Locked` quests (future quests where prerequisites aren't met), which defeated the purpose of the `includeFutureQuests` feature.

## Fixes Applied

### Server/MoreCheckmarksBackend.cs (Main Fix)

**Changed array to List:**

```csharp
// Before
Quest[] quests = [];

// After
var quests = new List<Quest>();
```

**Changed append to add:**

```csharp
// Before
_ = quests.Append(quest);

// After
quests.Add(quest);
```

**Fixed return statement:**

```csharp
// Before
return quests;

// After
return quests.ToArray();
```

**Inverted quest filtering logic (lines ~102-120):**

```csharp
// Before: Only included active quests (excluded future quests)
if (questStatus == QuestStatusEnum.AvailableForStart
    || questStatus == QuestStatusEnum.Started
    || questStatus == QuestStatusEnum.AvailableForFinish)
{
    quests.Add(quest);
}

// After: Include ALL quests not yet completed (including future quests)
if (questStatus != QuestStatusEnum.Success
    && questStatus != QuestStatusEnum.Fail
    && questStatus != QuestStatusEnum.FailRestartable
    && questStatus != QuestStatusEnum.MarkedAsFailed
    && questStatus != QuestStatusEnum.Expired)
{
    quests.Add(quest);
}
```

## Build Configuration Changes

### Server/MoreCheckmarksBackend.csproj

- Output path set to `dist/SPT/user/mods/MoreCheckmarksBackend/`
- Added settings to prevent copying PDB files and dependencies
- Added optional `CopyToSPT` target for local testing

### Client/MoreCheckmarks.csproj

- Output path set to `dist/BepInEx/plugins/MoreCheckmarks/`
- Added `<Private>false</Private>` to all references to prevent copying game DLLs
- Added `CleanupOutput` target to delete unwanted System.\*.dll files
- Added asset copying (MoreCheckmarksAssets)
- Added optional `CopyToSPT` target for local testing

### .gitignore

- Added `dist/` and `dist_example/` to build results section

## Build Output Structure

```
dist/
├── SPT/user/mods/MoreCheckmarksBackend/
│   └── MoreCheckmarksBackend.dll
│
└── BepInEx/plugins/MoreCheckmarks/
    ├── MoreCheckmarks.dll
    └── MoreCheckmarksAssets
```

**Note:** Configuration is handled via BepInEx's built-in config system. The config file is automatically created at `BepInEx/config/VIP.TommySoucy.MoreCheckmarks.cfg` on first run and can be edited via the F12 in-game menu.

## Build Commands

```bash
# Build entire solution
dotnet build MoreCheckmarks.sln

# Build server only
dotnet build Server/MoreCheckmarksBackend.csproj

# Build client only (may require MSBuild/Visual Studio)
msbuild Client/MoreCheckmarks.csproj
```

## Configuration

Update `$(SPTPath)` in both `.csproj` files to match your SPT installation:

- `Client/MoreCheckmarks.csproj` - Currently set to `C:\SPT`
- `Server/MoreCheckmarksBackend.csproj` - Currently set to `C:\SPT`

## Files Modified

| File                                  | Type of Change                                        |
| ------------------------------------- | ----------------------------------------------------- |
| `Server/MoreCheckmarksBackend.cs`     | Bug fix (quest filtering logic)                       |
| `Server/MoreCheckmarksBackend.csproj` | Build configuration                                   |
| `Client/MoreCheckmarks.cs`            | Quest prerequisites feature, BepInEx config migration |
| `Client/MoreCheckmarks.csproj`        | Build configuration                                   |
| `.gitignore`                          | Added dist folders                                    |
| `.cursorrules`                        | Updated documentation                                 |
| `README.md`                           | Added showPrerequisiteQuests documentation            |

## Files Created

| File                           | Purpose                              |
| ------------------------------ | ------------------------------------ |
| `spt-mod-template.cursorrules` | Template for future SPT mod projects |
| `SESSION_SUMMARY.md`           | This file                            |

## Files Deleted

| File                        | Reason                                   |
| --------------------------- | ---------------------------------------- |
| `Client/Assets/Config.json` | Replaced by BepInEx native config system |

## Testing Notes

- Server logs should show `Got X quests for MoreCheckmarks` with a high count (100+)
- Quest tooltips only appear on **Found In Raid** items
- Test with items like Salewa (needed for "Shortage" quest) to verify functionality

## PR Submitted

The fix for `Server/MoreCheckmarksBackend.cs` was submitted as a PR to the original MoreCheckmarks repository.

---

## Feature Added: BepInEx F12 Config Menu (Dec 3, 2025)

### Overview

Migrated all configuration from static `Config.json` file to BepInEx's native ConfigurationManager system. Settings are now accessible via the F12 in-game menu with real-time editing support.

### Changes Made

**1. Replaced static config loading with `ConfigEntry<T>` bindings:**

- All settings now use `Config.Bind()` with categories, descriptions, and appropriate value ranges
- Color settings use native `Color` type for RGB sliders instead of hex strings
- Priority settings have `AcceptableValueRange<int>(0, 10)` with slider controls

**2. Added event handlers for real-time updates:**

- Colors and priorities update immediately when changed (after switching menus)
- Added informational note at top of config menu about refresh requirement

**3. Config categories:**

- `0. Important Note` - Refresh instructions
- `Hideout` - Hideout-related settings
- `Quests` - Quest-related settings including new prerequisite feature
- `Barter & Craft` - Trade/crafting visibility
- `Priority` - Checkmark priority ordering (sorted by default priority)
- `Colors` - RGB color pickers for each checkmark type

**4. Removed files:**

- `Client/Assets/Config.json` - No longer needed

### Benefits

- No manual file editing required
- Hoverable descriptions for each setting
- Native color pickers with RGB sliders
- Settings persist automatically
- Standard BepInEx config location (`BepInEx/config/VIP.TommySoucy.MoreCheckmarks.cfg`)

---

## Feature Added: Quest Prerequisite Count (Dec 3, 2025)

### Overview

Added a new feature that shows the number of prerequisite quests needed before a quest becomes available.

### Display Format

- **Available quests**: `Quest Name (Available)` - shown in green
- **Future quests**: `Quest Name (X prereqs)` - shown in gray, where X is the count of incomplete prerequisites

### Implementation Details

#### Client/MoreCheckmarks.cs Changes

**1. Modified `QuestPair` class to store quest IDs:**

```csharp
public class QuestPair
{
    // Key: quest name key, Value: (QuestName, QuestId)
    public Dictionary<string, (string questName, string questId)> questData = new();
    public int count;
}
```

**2. Added new data structures:**

```csharp
// Quest prerequisite tracking
public static Dictionary<string, HashSet<string>> questPrerequisites = new();
public static Dictionary<string, HashSet<string>> prereqCache = new();  // Memoization cache
public static HashSet<string> completedQuestIds = new();
```

**3. Added prerequisite map building in `LoadData()`:**

- Parses `conditions.AvailableForStart` for `conditionType: "Quest"`
- Extracts target quest IDs as prerequisites
- Builds `questPrerequisites` dictionary

**4. Added helper methods:**

- `GetAllPrerequisites(questId)` - BFS traversal to get all recursive prerequisites, with caching
- `GetRemainingPrerequisiteCount(questId, profile)` - Counts incomplete prerequisites
- `GetPrerequisiteStatusString(questId, profile)` - Returns formatted status string

**5. Updated tooltip display:**

- Modified both start quest and complete quest sections
- Adds prerequisite status after each quest name

### Technical Notes

- Uses BFS (Breadth-First Search) with memoization for efficient prerequisite chain resolution
- Cache is cleared when `LoadData()` is called (profile selection)
- Handles deep quest chains (e.g., Collector with 50+ prerequisites)
- Checks player's `profile.QuestsData` to determine completion status

---

## Technical Reference

### Quest Status Enum Values

```
Locked = 0
AvailableForStart = 1
Started = 2
AvailableForFinish = 3
Success = 4
Fail = 5
FailRestartable = 6
MarkedAsFailed = 7
Expired = 8
AvailableAfter = 9
```

### Data Flow

1. Client calls `/MoreCheckmarksRoutes/quests`
2. Server's `HandleQuests()` fetches all quests, filters by player side and quest status
3. Server returns JSON array of Quest objects
4. Client parses quests in `LoadData()`, builds lookup dictionaries by item template ID
5. When showing item tooltip, client checks if item is in quest requirements
