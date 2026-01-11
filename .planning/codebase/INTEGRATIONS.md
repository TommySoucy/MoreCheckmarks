# External Integrations

**Analysis Date:** 2026-01-11

## APIs & External Services

**SPT Server API:**
- Custom HTTP routes for client-server communication
  - Client: `SPT.Common.Http.RequestHandler` for HTTP requests
  - Server: `CustomStaticRouter` extends `StaticRouter` for route handling
  - Routes defined in `Server/MoreCheckmarksRouter.cs`

**Custom Endpoints:**
- `/MoreCheckmarksRoutes/quests` - Get incomplete quests for player
- `/MoreCheckmarksRoutes/assorts` - Get trader barter assortments
- `/MoreCheckmarksRoutes/items` - Get item template data
- `/MoreCheckmarksRoutes/locales` - Get localization strings
- `/MoreCheckmarksRoutes/productions` - Get hideout crafting recipes

## Data Storage

**Databases:**
- SPT Server Database - Accessed via `DatabaseServer.GetTables()`
  - Quest data: `databaseServer.GetTables().Templates.Items`
  - Trader data: `databaseServer.GetTables().Traders`
  - Hideout data: `databaseServer.GetTables().Hideout.Production`
  - Locales: `databaseServer.GetTables().Locales`

**File Storage:**
- Unity AssetBundle for custom sprites
  - Location: `Client/Assets/MoreCheckmarksAssets`
  - Contains: White checkmark sprite for colored overlays

**Caching:**
- In-memory dictionaries in client for quest/item lookups
  - `questDataStartByItemTemplateID`, `questDataCompleteByItemTemplateID`
  - `bartersByItemByTrader`, `productionEndProductByID`
  - Rebuilt on data reload

## Authentication & Identity

**Auth Provider:**
- SPT session system
  - Session ID passed automatically via `RequestHandler`
  - Profile access via `ProfileHelper.GetPmcProfile(sessionId)`

**OAuth Integrations:**
- None

## Monitoring & Observability

**Error Tracking:**
- BepInEx Logger for client-side logging
  - `Logger.LogInfo()`, `Logger.LogError()`
- SPTarkov ISptLogger for server-side logging
  - `logger.Info()`, `logger.Error()`, `logger.Success()`

**Analytics:**
- None

**Logs:**
- Client: BepInEx log output (console/file)
- Server: SPT server log output

## CI/CD & Deployment

**Hosting:**
- Local installation only (game mod)
- No cloud deployment

**CI Pipeline:**
- None configured
- Manual build via Visual Studio

## Environment Configuration

**Development:**
- SPT path in .csproj: `<SPTPath>C:\SPT</SPTPath>`
- Reference assemblies from local SPT installation
- Build output to `dist/` folder

**Staging:**
- Not applicable (local mod)

**Production:**
- Users extract mod files to their SPT installation
- No secrets or environment variables required

## Webhooks & Callbacks

**Incoming:**
- None

**Outgoing:**
- None

## Game Integration Points

**BepInEx Plugin System:**
- Plugin metadata: `[BepInPlugin(pluginGuid, pluginName, pluginVersion)]`
- Configuration: BepInEx ConfigEntry system
- Entry point: `Start()` method in `MoreCheckmarksMod`

**Harmony Patches:**
- UI modification patches in `Client/MoreCheckmarks.cs`
- Patches game methods to add colored checkmarks and tooltips

**Unity Integration:**
- AssetBundle loading for custom sprites
- TextMeshPro for tooltip text
- UnityEngine.UI for checkmark images

---

*Integration audit: 2026-01-11*
*Update when adding/removing external services*
