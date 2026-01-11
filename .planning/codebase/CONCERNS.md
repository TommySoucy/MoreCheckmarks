# Codebase Concerns

**Analysis Date:** 2026-01-11

## Tech Debt

**Monolithic Client File:**
- Issue: All client logic in single 118KB file (`Client/MoreCheckmarks.cs`)
- Why: Organic growth, simple initial design
- Impact: Difficult to navigate, high coupling, slow IDE performance
- Fix approach: Extract logical components (data loading, UI patches, config) into separate files

**Repeated Quest Condition Parsing:**
- Issue: Nearly identical code blocks for HandoverItem, FindItem, LeaveItemAtLocation, PlaceBeacon
- Files: `Client/MoreCheckmarks.cs` (lines 220-500 approximately)
- Why: Each condition type added incrementally
- Impact: Bug fixes need to be applied in multiple places
- Fix approach: Extract common pattern into reusable method with condition type parameter

**Hardcoded SPT Path:**
- Issue: Default SPT path hardcoded in .csproj files
- Files: `Client/MoreCheckmarks.csproj`, `Server/MoreCheckmarksBackend.csproj`
- Why: Developer convenience
- Impact: Fails for users with non-standard SPT installations
- Fix approach: Already uses Condition="Exists('$(SPTPath)')" - acceptable trade-off

## Known Bugs

**No Known Bugs:**
- README.md shows previously known issues as fixed in v2.0.1 and v2.1.0
- No TODO/FIXME comments in codebase

## Security Considerations

**No Significant Security Risks:**
- Local mod, no network exposure beyond localhost SPT server
- No user credentials or sensitive data handled
- No external API calls outside SPT ecosystem

## Performance Bottlenecks

**Large Data Loading on Startup:**
- Problem: Client fetches all quest, item, assort, locale, production data on init
- Files: `Client/MoreCheckmarks.cs` - `LoadData()` method
- Measurement: Not profiled, but observable load time on game start
- Cause: All data cached in memory for fast lookups during gameplay
- Improvement path: Lazy loading or background loading could improve startup time

**Large Dictionary Lookups:**
- Problem: Multiple dictionary lookups per item view
- Files: `Client/MoreCheckmarks.cs` - Harmony patch methods
- Measurement: Not profiled, but many items in stash
- Cause: Design choice for feature richness
- Improvement path: Acceptable for game mod context

## Fragile Areas

**Harmony Patches:**
- Files: `Client/MoreCheckmarks.cs` (various patch classes)
- Why fragile: Game updates may change patched methods, breaking mod
- Common failures: NullReferenceException if game internals change
- Safe modification: Test thoroughly after game/SPT updates
- Test coverage: No automated tests

**Quest Data Parsing:**
- Files: `Client/MoreCheckmarks.cs` - `LoadData()` method
- Why fragile: Depends on exact JSON structure from server
- Common failures: KeyNotFoundException, NullReferenceException if structure changes
- Safe modification: Add more null checks, validate structure before parsing
- Test coverage: None

## Scaling Limits

**Memory Usage:**
- Current capacity: All game data loaded into dictionaries
- Limit: Very large mod lists or item databases could exhaust memory
- Symptoms at limit: OutOfMemoryException, game crash
- Scaling path: Lazy loading, LRU cache for rarely-used data

## Dependencies at Risk

**SPT Version Coupling:**
- Risk: Mod explicitly requires SPT 4.0.x, will break on major SPT updates
- Files: `Server/MoreCheckmarksBackend.cs` - `SptVersion { get; init; } = new("~4.0.0")`
- Impact: Mod non-functional until updated for new SPT version
- Migration plan: Monitor SPT releases, update references and API usage

**Game Assembly References:**
- Risk: References specific game DLLs that change with game updates
- Files: `Client/MoreCheckmarks.csproj` - all HintPath references to $(SPTPath)
- Impact: Compilation fails if assemblies change
- Migration plan: Update references when game/SPT updates

## Missing Critical Features

**No Critical Missing Features:**
- Mod is feature-complete for its purpose
- Recent v2.1.0 fixed barter display issues

## Test Coverage Gaps

**No Automated Tests:**
- What's not tested: All code paths
- Risk: Regressions can ship undetected
- Priority: Medium (mod is stable, manual testing works)
- Difficulty to test: Would need to mock game/SPT APIs

**Specific Untested Areas:**
- Quest data parsing edge cases
- Profile null/empty states
- Color/priority configuration application
- Tooltip rendering logic
- Harmony patch interactions

---

*Concerns audit: 2026-01-11*
*Update as issues are fixed or new ones discovered*
