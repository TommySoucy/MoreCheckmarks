# Coding Conventions

**Analysis Date:** 2025-01-11

## Naming Patterns

**Files:**
- PascalCase for all C# files (MoreCheckmarks.cs, MoreCheckmarksBackend.cs)
- Project name as prefix for main files
- .csproj files match project name

**Functions:**
- PascalCase for public methods (LoadData, GetNeeded, HandleQuests)
- camelCase for private methods when short (none observed - private methods use PascalCase)
- Prefix/Postfix for Harmony patch methods

**Variables:**
- camelCase for local variables and parameters
- PascalCase for public static fields (configQuestPriority)
- camelCase for private fields with underscore prefix for injected dependencies (none in client)
- UPPER_SNAKE_CASE for const strings (pluginGuid, pluginName)

**Types:**
- PascalCase for classes, structs, interfaces
- No I prefix for interfaces
- Nested classes for Harmony patches (QuestItemViewPanelShowPatch)

## Code Style

**Formatting:**
- 4-space indentation (observed in both Client and Server)
- Opening brace on same line for methods and control structures
- Spaces around operators

**Linting:**
- No explicit linter configuration found
- Relies on Visual Studio defaults
- Warning level 4 (Client), Warning level 8 (Server)

## Import Organization

**Order (Client):**
1. BepInEx namespaces
2. Game namespaces (Comfort, EFT.*)
3. HarmonyLib
4. Newtonsoft.Json
5. SPT.Common
6. System namespaces
7. Unity namespaces

**Order (Server):**
1. SPTarkov.* namespaces
2. System namespaces

**Grouping:**
- No blank lines between using statements
- Alphabetical within game namespaces

## Error Handling

**Patterns:**
- Try/catch at method boundaries, especially in patches
- Log error with context before continuing
- Harmony patches return true on error to run original method

**Error Types:**
- Catch generic Exception in patches (safe fallback)
- Log error messages via BepInEx Logger (client) or ISptLogger (server)

**Examples from code:**
```csharp
catch (Exception e)
{
    MoreCheckmarksMod.LogError(
        "Failed to show checkmark for item " + item.Template.Name + " - " + e.Message);
    return true; // Run original method
}
```

## Logging

**Framework:**
- Client: BepInEx Logger (Logger.LogInfo, Logger.LogError)
- Server: SPT ISptLogger<T> with typed logging

**Patterns:**
- Log at method entry for data requests ("MoreCheckmarks making quest data request")
- Log counts and results ("Got {count} quests for MoreCheckmarks")
- Static helper methods wrap Logger access (LogInfo, LogError)

## Comments

**When to Comment:**
- Explain game-specific class names that change between versions
- Document UPDATE markers for version-specific code
- Explain complex logic (prerequisite counting, priority system)

**JSDoc/TSDoc:**
- XML documentation on some public methods (summary tags)
- Not consistently applied

**TODO Comments:**
- None found in codebase (clean)

## Function Design

**Size:**
- Large monolithic methods (LoadData ~700 lines, patch methods 100-300 lines)
- Helper methods extracted for reuse (GetNeeded, GetBarters, GetPrerequisiteStatusString)

**Parameters:**
- Use `ref` for output parameters (areaNames list, tooltip string)
- Optional parameters with defaults (needTooltip = true)

**Return Values:**
- Structs for multi-value returns (NeededStruct)
- Nullable types for optional data (Quest[]?, TraderAssort[]?)

## Module Design

**Exports:**
- No explicit module system (single-file projects)
- Public classes and methods accessible across assembly

**Organization:**
- Client: Single file with main class + nested patch classes
- Server: Two files - main server class and router class
- Mod metadata as separate record in server project

## Game-Specific Conventions

**Harmony Patches:**
- Use `[HarmonyPatch]` attribute on class
- `[HarmonyPatch(typeof(TargetClass), "MethodName")]` on methods
- Prefix returns bool to control original execution
- Postfix for modifications after original runs

**BepInEx Plugin:**
- `[BepInPlugin(guid, name, version)]` attribute required
- Inherit from BaseUnityPlugin
- Config via Config.Bind() with ConfigDescription

**SPT Server Mod:**
- Implement IOnLoad interface
- Use `[Injectable]` attribute for DI registration
- AbstractModMetadata record for mod info

---

*Convention analysis: 2025-01-11*
*Update when patterns change*
