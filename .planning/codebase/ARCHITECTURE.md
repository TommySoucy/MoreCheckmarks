# Architecture

**Analysis Date:** 2025-01-11

## Pattern Overview

**Overall:** Client-Server Mod with Harmony Patching

**Key Characteristics:**
- Two-project architecture (Client plugin + Server mod)
- Client uses Harmony patches to intercept/modify game behavior
- Server provides data endpoints via HTTP routes
- Client communicates with server for quest/item/assort data
- BepInEx plugin lifecycle for client initialization

## Layers

**Client Plugin (`Client/`):**
- Purpose: Modify game UI to show enhanced checkmarks and tooltips
- Contains: BepInEx plugin entry point, Harmony patches, UI logic
- Location: `Client/MoreCheckmarks.cs`
- Depends on: Game assemblies, BepInEx, Harmony, Server HTTP endpoints
- Used by: Game runtime (loaded by BepInEx)

**Server Mod (`Server/`):**
- Purpose: Provide game data (quests, items, assorts, productions) to client
- Contains: HTTP route handlers, data access logic
- Location: `Server/MoreCheckmarksBackend.cs`, `Server/MoreCheckmarksRouter.cs`
- Depends on: SPTarkov.Server.Core, database tables, profile data
- Used by: Client plugin via HTTP requests

## Data Flow

**Plugin Initialization:**

1. BepInEx loads `MoreCheckmarks.dll` on game start
2. `MoreCheckmarksMod.Start()` called - sets up mod instance
3. `Init()` runs: loads config, assets (checkmark sprite), data, applies patches
4. `LoadData()` fetches quest/item/assort data from server via HTTP
5. Harmony patches applied to game methods

**Item Checkmark Display:**

1. Game calls `QuestItemViewPanel.Show()` for item display
2. Harmony prefix intercepts call (`QuestItemViewPanelShowPatch`)
3. Patch checks item against: quests, hideout, wishlist, barters, crafts
4. Color determined by priority system and fulfillment status
5. Custom tooltip built with detailed requirement info
6. Original method skipped, custom UI rendered

**Server Data Request:**

1. Client calls `RequestHandler.GetJson("/MoreCheckmarksRoutes/quests")`
2. `CustomStaticRouter` routes request to `MoreCheckmarksServer.HandleQuests()`
3. Server queries database for quest data, filters by profile
4. JSON response returned to client
5. Client parses and caches data in dictionaries

**State Management:**
- Client-side: Static dictionaries cache quest/item mappings
- Server-side: Stateless - reads from SPT database on each request
- Profile-aware: Server filters data based on player profile

## Key Abstractions

**MoreCheckmarksMod (Client):**
- Purpose: Main plugin class, entry point, data management
- Location: `Client/MoreCheckmarks.cs`
- Pattern: Singleton (static `modInstance`)

**Harmony Patches (Client):**
- Purpose: Intercept game methods to modify behavior
- Examples: `QuestItemViewPanelShowPatch`, `ItemSpecificationPanelShowPatch`, `AvailableActionsPatch`
- Location: `Client/MoreCheckmarks.cs` (nested classes)
- Pattern: Harmony prefix/postfix methods

**MoreCheckmarksServer (Server):**
- Purpose: Handle HTTP requests for game data
- Location: `Server/MoreCheckmarksBackend.cs`
- Pattern: Injectable singleton via SPT DI

**CustomStaticRouter (Server):**
- Purpose: Register and route HTTP endpoints
- Location: `Server/MoreCheckmarksRouter.cs`
- Pattern: SPT StaticRouter extension

## Entry Points

**Client Entry:**
- Location: `Client/MoreCheckmarks.cs` - `MoreCheckmarksMod.Start()`
- Triggers: BepInEx plugin loading on game start
- Responsibilities: Initialize config, load assets, fetch data, apply patches

**Server Entry:**
- Location: `Server/MoreCheckmarksBackend.cs` - `MoreCheckmarksServer.OnLoad()`
- Triggers: SPT server startup (IOnLoad interface)
- Responsibilities: Register routes, log success message

**HTTP Endpoints:**
- `/MoreCheckmarksRoutes/quests` - Get quest data for profile
- `/MoreCheckmarksRoutes/assorts` - Get trader assort data
- `/MoreCheckmarksRoutes/items` - Get item templates
- `/MoreCheckmarksRoutes/locales` - Get localization data
- `/MoreCheckmarksRoutes/productions` - Get hideout production recipes

## Error Handling

**Strategy:** Try/catch at patch level, log errors, fall back to original behavior

**Patterns:**
- Harmony patches wrap logic in try/catch, return `true` to run original on error
- Server methods catch exceptions, log errors, return null/empty responses
- Client handles null/empty server responses gracefully

## Cross-Cutting Concerns

**Logging:**
- Client: BepInEx Logger (`Logger.LogInfo`, `Logger.LogError`)
- Server: SPT ISptLogger with typed logging

**Configuration:**
- Client: BepInEx ConfigEntry system with F12 menu support
- Server: Uses SPT ConfigServer for quest config

**Data Caching:**
- Quest data cached in static dictionaries
- Completed quest IDs cached for performance
- Prerequisite chains cached with BFS computation

---

*Architecture analysis: 2025-01-11*
*Update when major patterns change*
