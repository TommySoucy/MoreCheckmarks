# Codebase Concerns

**Analysis Date:** 2025-01-11

## Tech Debt

**Monolithic client file:**
- Issue: All client logic in single 2300-line file `Client/MoreCheckmarks.cs`
- Why: Evolved from simpler mod, easier to keep in one file during rapid development
- Impact: Hard to navigate, difficult to understand data flow, harder to maintain
- Fix approach: Extract into separate files - Patches/, Config/, Data/, UI/

**Hardcoded game class names:**
- Issue: Version-specific class names hardcoded (e.g., `Class308`, `Class1596`)
- Files: `Client/MoreCheckmarks.cs` - `DoPatching()` method around line 1087
- Why: Game obfuscates class names, they change between versions
- Impact: Mod breaks on game updates until class names are updated
- Fix approach: Add comments documenting how to find correct class names in dnSpy

**Repeated quest parsing logic:**
- Issue: Nearly identical code blocks for parsing HandoverItem, FindItem, LeaveItemAtLocation, PlaceBeacon
- Files: `Client/MoreCheckmarks.cs` - `LoadData()` method, lines 210-785
- Why: Each condition type has slight variations, copy-pasted during development
- Impact: Bug fixes must be applied in multiple places, easy to miss one
- Fix approach: Extract common parsing logic into helper method with condition type parameter

## Known Bugs

**New profile checkmarks missing:**
- Symptoms: Checkmarks don't appear on items until first quest is accepted
- Trigger: Create new SPT profile, view items in stash before accepting any quest
- Workaround: Accept any quest, then restart game
- Root cause: Quest data fetch returns empty when profile not fully initialized
- Files: `Server/MoreCheckmarksBackend.cs` - `HandleQuests()`, `Client/MoreCheckmarks.cs` - `LoadData()`
- Note: Documented in README as known issue

**Assorts array not populated:**
- Symptoms: Barter information may be incomplete
- Trigger: Unknown - intermittent
- Files: `Server/MoreCheckmarksBackend.cs` - `HandleAssorts()` around line 152
- Root cause: Using `_ = assorts.Append()` which returns new array but discards it
- Fix: Change to `assorts = assorts.Append(...).ToArray()` or use List<T>

## Security Considerations

**No significant security concerns:**
- Risk: Low - local-only mod, no external network calls, no user data handling
- Mitigations in place: SPT session ID handled by framework, no credentials stored

## Performance Bottlenecks

**Prerequisite calculation on every tooltip:**
- Problem: `GetRemainingPrerequisiteCount()` computed for each quest in tooltip
- Files: `Client/MoreCheckmarks.cs` - `SetTooltip()` method, lines 1697-1834
- Measurement: Not profiled, but involves BFS traversal per quest
- Cause: Prerequisite counts not pre-computed for all quests
- Improvement path: Pre-compute all prerequisite counts in `LoadData()`, store in dictionary

**Large data dictionaries rebuilt on profile switch:**
- Problem: All quest/item data parsed from JSON on every profile selection
- Files: `Client/MoreCheckmarks.cs` - `LoadData()` called from `ProfileSelectionPatch`
- Cause: No caching of parsed data between profile loads
- Improvement path: Cache parsed data, only refresh changed quests

## Fragile Areas

**Harmony patch target methods:**
- Files: `Client/MoreCheckmarks.cs` - All patch classes
- Why fragile: Game updates change method signatures, class names, internal structure
- Common failures: NullReferenceException when accessing moved/renamed fields
- Safe modification: Test thoroughly after any game/SPT update
- Test coverage: None (manual testing only)

**Quest condition type handling:**
- Files: `Client/MoreCheckmarks.cs` - `LoadData()` method
- Why fragile: Assumes specific JSON structure from server
- Common failures: New condition types not handled, structure changes break parsing
- Safe modification: Add try/catch around individual condition parsing

## Scaling Limits

**In-memory quest data:**
- Current capacity: Handles ~200+ quests without issue
- Limit: Memory usage grows with quest count (all loaded at once)
- Symptoms at limit: Increased memory usage, longer load times
- Scaling path: Not a concern for current SPT quest counts

## Dependencies at Risk

**Game version coupling:**
- Risk: Every SPT/game update may break mod due to obfuscated class changes
- Impact: Mod completely non-functional until updated
- Migration plan: Document class finding process, maintain version-specific branches

**SPTarkov.Server.Core v4.0.0:**
- Risk: SPT API changes between major versions
- Impact: Server mod must be rewritten for SPT 5.x
- Migration plan: Monitor SPT development, plan migration when 5.x approaches

## Missing Critical Features

**No automated testing:**
- Problem: No unit or integration tests
- Current workaround: Manual testing after each change
- Blocks: Confident refactoring, regression detection
- Implementation complexity: High (game assembly mocking difficult)

## Test Coverage Gaps

**All code untested:**
- What's not tested: Entire codebase
- Risk: Regressions introduced without detection
- Priority: Medium - mod is stable but refactoring risky
- Difficulty to test: High - requires mocking game assemblies or integration test setup

**Priority areas needing tests:**
- Quest prerequisite calculation (`GetAllPrerequisites`, `GetRemainingPrerequisiteCount`)
- Color priority selection logic
- Hideout requirement fulfillment checking

---

*Concerns audit: 2025-01-11*
*Update as issues are fixed or new ones discovered*
