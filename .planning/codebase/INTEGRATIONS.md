# External Integrations

**Analysis Date:** 2025-01-11

## APIs & External Services

**SPT Server HTTP API:**
- Custom routes registered by server mod for client data access
  - Endpoint: `/MoreCheckmarksRoutes/quests` - Quest data for current profile
  - Endpoint: `/MoreCheckmarksRoutes/assorts` - Trader assortment data
  - Endpoint: `/MoreCheckmarksRoutes/items` - Item template database
  - Endpoint: `/MoreCheckmarksRoutes/locales` - Localization strings
  - Endpoint: `/MoreCheckmarksRoutes/productions` - Hideout production recipes
- Client: `SPT.Common.Http.RequestHandler.GetJson()` - `Client/MoreCheckmarks.cs`
- Server: `CustomStaticRouter` routes - `Server/MoreCheckmarksRouter.cs`
- Auth: Session ID passed automatically by SPT client

## Data Storage

**Databases:**
- SPT DatabaseServer - Accessed via `databaseServer.GetTables()`
  - Items: `Templates.Items` dictionary
  - Traders: `Traders` dictionary with assorts
  - Hideout: `Hideout.Production` for craft recipes
  - Locales: `Locales` for text translations
- No direct database connection - all through SPT abstractions

**File Storage:**
- Unity AssetBundle: `Client/Assets/MoreCheckmarksAssets`
  - Contains: WhiteCheckmark sprite, BenderBold font
  - Loaded at: Plugin initialization via `AssetBundle.LoadFromFile()`

**Caching:**
- Client-side in-memory caching via static dictionaries
  - `questDataStartByItemTemplateID` - Quest requirements by item
  - `questDataCompleteByItemTemplateID` - Quest completion items
  - `bartersByItemByTrader` - Barter requirements
  - `productionEndProductByID` - Craft outputs
  - `prereqCache` - Computed prerequisite chains
  - `completedQuestIds` - Completed quest tracking

## Authentication & Identity

**Auth Provider:**
- SPT session management (automatic)
- Session ID passed with HTTP requests
- Profile loaded via `ProfileHelper.GetPmcProfile(sessionId)`

**No external auth integrations.**

## Monitoring & Observability

**Error Tracking:**
- BepInEx Logger (client) - Logs to `BepInEx/LogOutput.log`
- SPT ISptLogger (server) - Logs to SPT server console/logs

**Analytics:**
- None

**Logs:**
- Client: BepInEx file logging
- Server: SPT console output
- Log levels: Info, Warning, Error, Success

## CI/CD & Deployment

**Hosting:**
- Local installation only (SPT is single-player)
- No cloud deployment

**CI Pipeline:**
- Not detected
- Manual build via Visual Studio

**Distribution:**
- 7z archive in `dist/` directory
- Manual upload to mod hosting sites

## Environment Configuration

**Development:**
- Required: SPT installation at configured path
- SPT path set in .csproj files: `<SPTPath>C:\SPT</SPTPath>`
- Build automatically copies to SPT installation if path exists

**Production:**
- Same as development (local SPT installation)
- User extracts mod files to SPT directories

## Game Integration Points

**BepInEx Plugin System:**
- Location: `Client/MoreCheckmarks.cs`
- Plugin GUID: `VIP.TommySoucy.MoreCheckmarks`
- Loaded by BepInEx on game start
- Provides F12 configuration menu

**Harmony Patching:**
- Patches game methods at runtime
- Target classes (version-specific):
  - `QuestItemViewPanel.Show()` - Main checkmark display
  - `ItemSpecificationPanel.method_2()` - Inspect window
  - `GetActionsClass.smethod_8()` - "Take" action coloring
  - `QuestClass.SetStatus()` - Quest status change handling
  - Profile selector (dynamic class names)

**SPT Server Mod System:**
- Location: `Server/MoreCheckmarksBackend.cs`
- Mod GUID: `custom-static-MoreCheckmarksRoutes`
- Registered via SPT dependency injection
- Implements `IOnLoad` interface

**Game Singletons Accessed:**
- `Singleton<HideoutClass>.Instance` - Hideout data
- `ItemUiContext.Instance.WishlistManager` - Wishlist checking
- Profile via `profileHelper.GetPmcProfile()` (server)

## Webhooks & Callbacks

**Incoming:**
- None

**Outgoing:**
- None

---

*Integration audit: 2025-01-11*
*Update when adding/removing external services*
