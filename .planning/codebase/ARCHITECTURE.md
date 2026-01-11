# Architecture

**Analysis Date:** 2026-01-11

## Pattern Overview

**Overall:** Two-Component Mod (Client Plugin + Server Backend)

**Key Characteristics:**
- Client-side BepInEx plugin for UI modifications
- Server-side mod for data access and API endpoints
- HTTP communication between client and server
- Harmony patching for runtime game modification

## Layers

**Client Layer (`Client/`):**
- Purpose: Modify game UI to show colored checkmarks and tooltips
- Contains: BepInEx plugin, Harmony patches, UI rendering logic
- Location: `Client/MoreCheckmarks.cs` (single file, ~118KB)
- Depends on: Server routes for game data, Unity/BepInEx/Harmony APIs
- Used by: Escape from Tarkov game process

**Server Layer (`Server/`):**
- Purpose: Provide data endpoints for client queries
- Contains: Route handlers, data access logic, mod metadata
- Location: `Server/MoreCheckmarksBackend.cs`, `Server/MoreCheckmarksRouter.cs`
- Depends on: SPT Server Core APIs, database tables
- Used by: Client plugin via HTTP requests

## Data Flow

**Item Checkmark Display:**

1. Game loads, BepInEx initializes `MoreCheckmarksMod.Start()`
2. `Init()` binds config, loads assets, calls `LoadData()`
3. `LoadData()` fetches from server endpoints:
   - `/MoreCheckmarksRoutes/quests` → quest requirements
   - `/MoreCheckmarksRoutes/assorts` → trader barters
   - `/MoreCheckmarksRoutes/items` → item templates
   - `/MoreCheckmarksRoutes/locales` → localization
   - `/MoreCheckmarksRoutes/productions` → hideout crafts
4. Data stored in static dictionaries (e.g., `questDataCompleteByItemTemplateID`)
5. Harmony patches intercept UI rendering methods
6. Patch code checks item against dictionaries, applies colored checkmark
7. Tooltip patched to show detailed requirements

**Server Request Handling:**

1. Client calls `RequestHandler.GetJson("/MoreCheckmarksRoutes/quests")`
2. `CustomStaticRouter` routes to `HandleQuestsRoute()`
3. `MoreCheckmarksServer.HandleQuests()` queries database
4. Uses `ProfileHelper`, `QuestHelper`, `DatabaseServer` for data
5. Filters quests by profile side (USEC/BEAR) and completion status
6. Returns JSON array of incomplete quests

**State Management:**
- Client: Static dictionaries rebuilt on data reload
- Server: Stateless request handlers, database accessed per-request
- Config: BepInEx ConfigEntry system with F12 menu

## Key Abstractions

**MoreCheckmarksMod (Client):**
- Purpose: Main plugin class, orchestrates initialization
- Examples: `Client/MoreCheckmarks.cs`
- Pattern: BepInEx BaseUnityPlugin singleton

**MoreCheckmarksServer (Server):**
- Purpose: Data handler with injected dependencies
- Examples: `Server/MoreCheckmarksBackend.cs`
- Pattern: SPT IOnLoad with DI constructor injection

**CustomStaticRouter (Server):**
- Purpose: HTTP route definitions and dispatch
- Examples: `Server/MoreCheckmarksRouter.cs`
- Pattern: SPT StaticRouter extension with RouteAction list

**QuestPair (Client):**
- Purpose: Track quest name/ID pairs and item counts
- Examples: Nested class in `Client/MoreCheckmarks.cs`
- Pattern: Data structure for quest-item mapping

## Entry Points

**Client Entry:**
- Location: `Client/MoreCheckmarks.cs` - `MoreCheckmarksMod.Start()`
- Triggers: BepInEx plugin loading during game startup
- Responsibilities: Initialize config, load assets/data, apply Harmony patches

**Server Entry:**
- Location: `Server/MoreCheckmarksBackend.cs` - `MoreCheckmarksServer.OnLoad()`
- Triggers: SPT server mod loading
- Responsibilities: Register routes, log startup message

## Error Handling

**Strategy:** Try-catch at boundaries, graceful fallback to empty data

**Patterns:**
- Client: Log errors, continue with empty/default data
  ```csharp
  catch (Exception ex) {
      LogError($"Failed to parse quest data: {ex.Message}");
      questData = new JArray(); // Fallback to empty
  }
  ```
- Server: Catch exceptions, return null/empty response
  ```csharp
  catch {
      logger?.Error("Exception when handling QuestsRoute!");
      return new ValueTask<string>("[]");
  }
  ```
- Profile null checks with early returns and logging

## Cross-Cutting Concerns

**Logging:**
- Client: `Logger.LogInfo()`, `Logger.LogError()` (BepInEx)
- Server: `ISptLogger<T>` with `.Info()`, `.Error()`, `.Success()`
- Custom `LogInfo()`, `LogError()` wrappers in client

**Configuration:**
- BepInEx ConfigEntry system for all client settings
- Categories: Hideout, Quests, Barter & Craft, Priority, Colors
- F12 in-game menu for runtime adjustment

**Validation:**
- Null checks on profile, quest conditions, targets
- Graceful handling of missing or malformed data

---

*Architecture analysis: 2026-01-11*
*Update when major patterns change*
