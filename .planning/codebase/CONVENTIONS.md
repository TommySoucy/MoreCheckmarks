# Coding Conventions

**Analysis Date:** 2026-01-11

## Naming Patterns

**Files:**
- PascalCase for C# files (MoreCheckmarks.cs, MoreCheckmarksBackend.cs)
- Single file per major component (all client logic in one file)
- Matching names: DLL name matches main class name

**Functions:**
- PascalCase for public methods (LoadData, HandleQuests, DoPatching)
- camelCase not used (consistent PascalCase throughout)
- Handle prefix for request handlers (HandleQuestsRoute, HandleAssortsRoute)

**Variables:**
- camelCase for local variables and parameters (questData, sessionId)
- camelCase for private fields (modPath, benderBold)
- PascalCase for public static fields (modInstance, whiteCheckmark)
- config prefix for ConfigEntry fields (configFulfilledAnyCanBeUpgraded)

**Types:**
- PascalCase for classes (MoreCheckmarksMod, QuestPair)
- PascalCase for structs (NeededStruct)
- No I prefix for interfaces (follows .NET conventions)

## Code Style

**Formatting:**
- 4-space indentation
- Allman style braces (opening brace on new line)
- Spaces around operators
- No trailing whitespace

**Linting:**
- No explicit linting configuration
- Visual Studio default analyzers

## Import Organization

**Order:**
1. System namespaces (System, System.Collections.Generic)
2. Framework namespaces (BepInEx, Comfort, EFT)
3. Third-party namespaces (Newtonsoft.Json, HarmonyLib)
4. SPT namespaces (SPT.Common.Http, SPTarkov.*)
5. Local namespaces (TMPro, UnityEngine)

**Grouping:**
- No blank lines between groups
- Alphabetical within framework categories

**Path Aliases:**
- None used

## Error Handling

**Patterns:**
- Try-catch at API boundaries
- Graceful fallback to empty/default data
- Log errors before continuing

**Error Types:**
- No custom exception classes
- Standard Exception with descriptive messages
- Null checks with early return

**Examples:**
```csharp
// Client pattern
try {
    var response = RequestHandler.GetJson("/route");
    if (string.IsNullOrEmpty(response) || response == "null") {
        LogInfo("Data empty, continuing with defaults");
        return new JArray();
    }
    return JArray.Parse(response);
} catch (Exception ex) {
    LogError($"Failed: {ex.Message}");
    return new JArray();
}

// Server pattern
try {
    return jsonUtil.Serialize(server.HandleQuests(sessionId));
} catch (Exception ex) {
    logger?.Error($"Exception: {ex.Message}");
    return "[]";
}
```

## Logging

**Framework:**
- Client: BepInEx Logger (Logger.LogInfo, Logger.LogError)
- Server: SPT ISptLogger<T> (logger.Info, logger.Error, logger.Success)

**Patterns:**
- Log at method entry for data requests ("MoreCheckmarks making quest data request")
- Log counts after operations ("Got 42 quests for MoreCheckmarks")
- Log warnings for expected edge cases ("Profile is null (not loaded yet?)")
- Custom wrapper methods in client (LogInfo, LogError)

## Comments

**When to Comment:**
- Brief section comments for major code blocks
- XML doc comments on server classes
- Inline comments for non-obvious logic

**JSDoc/TSDoc:**
- XML documentation on public server classes
- `<summary>` blocks for class descriptions

**TODO Comments:**
- None present in current codebase

**Examples:**
```csharp
/// <summary>
/// This is the replacement for the former package.json data. This is required for all mods.
/// </summary>
public record ModMetadata : AbstractModMetadata { ... }

// Check if profile has quest data (new profiles may not have any yet)
var profileQuests = profile.Quests;
```

## Function Design

**Size:**
- Large functions acceptable (LoadData is 400+ lines)
- No strict line limits
- Helper methods extracted where reused

**Parameters:**
- Reasonable parameter counts (1-5 typical)
- No options object pattern used
- Default parameters not commonly used

**Return Values:**
- Explicit returns
- Null returns for error cases on server
- Empty arrays/collections preferred over null on client

## Module Design

**Exports:**
- Public classes for main components
- Internal/private for helpers
- Static fields for shared state

**Barrel Files:**
- Not applicable (single-file per component)

**Client Organization:**
- All code in single MoreCheckmarks.cs file
- Nested classes for data structures (QuestPair)
- Static dictionaries for runtime data

**Server Organization:**
- Separate files for router and server logic
- DI via constructor injection
- Record for mod metadata

---

*Convention analysis: 2026-01-11*
*Update when patterns change*
