# Testing Patterns

**Analysis Date:** 2026-01-11

## Test Framework

**Runner:**
- None configured

**Assertion Library:**
- Not applicable

**Run Commands:**
```bash
# No test commands available
# Manual testing via SPT game client
```

## Test File Organization

**Location:**
- No test files present in codebase

**Naming:**
- Not applicable

**Structure:**
```
# No test directory structure exists
```

## Test Structure

**Suite Organization:**
- Not applicable - no automated tests

**Patterns:**
- Manual testing only
- Test in local SPT installation

## Mocking

**Framework:**
- Not applicable

**Patterns:**
- Not applicable

**What to Mock:**
- Not applicable

**What NOT to Mock:**
- Not applicable

## Fixtures and Factories

**Test Data:**
- Not applicable

**Location:**
- Not applicable

## Coverage

**Requirements:**
- No coverage requirements
- No automated testing

**Configuration:**
- Not applicable

**View Coverage:**
```bash
# No coverage tooling configured
```

## Test Types

**Unit Tests:**
- Not present

**Integration Tests:**
- Not present

**E2E Tests:**
- Manual testing in SPT game
- Build → Install to SPT → Launch game → Verify UI changes

## Manual Testing Process

**Build & Deploy:**
1. Build solution in Visual Studio
2. Files output to `dist/` folder
3. Copy to SPT installation (automated via .csproj targets if SPTPath exists)

**Verification:**
1. Launch SPT server
2. Launch game client
3. Open inventory/stash
4. Verify checkmarks appear on needed items
5. Verify tooltips show correct information
6. Test F12 config menu changes

**Test Scenarios:**
- New profile (no quest data)
- Profile with completed quests
- Items needed for hideout only
- Items needed for quests only
- Items needed for multiple purposes
- Barter items
- Craft ingredients
- Wishlist items
- Color customization
- Priority settings

## Common Patterns

**Async Testing:**
- Not applicable

**Error Testing:**
- Not applicable

**Snapshot Testing:**
- Not applicable

## Recommendations

**Future Testing Improvements:**
- Consider xUnit or NUnit for .NET testing
- Mock SPT server responses for client testing
- Unit test data parsing logic
- Unit test priority/color selection logic
- Integration test HTTP routes

**Testing Gaps:**
- All logic paths untested
- No regression testing
- No CI/CD validation
- Relies entirely on manual QA

---

*Testing analysis: 2026-01-11*
*Update when test patterns change*
