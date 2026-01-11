# Testing Patterns

**Analysis Date:** 2025-01-11

## Test Framework

**Runner:**
- Not detected - no automated test framework in use

**Assertion Library:**
- Not detected

**Run Commands:**
```bash
# No test commands configured
# Manual testing via SPT game client
```

## Test File Organization

**Location:**
- No test files present in codebase

**Naming:**
- Not applicable

**Structure:**
```
(No test directory structure)
```

## Test Structure

**Suite Organization:**
- Not applicable - no automated tests

**Patterns:**
- Manual testing approach
- Test by running SPT game and verifying UI behavior

## Mocking

**Framework:**
- Not applicable

**Patterns:**
- Game must be running for any testing
- No mock infrastructure for game assemblies

## Fixtures and Factories

**Test Data:**
- Not applicable

**Location:**
- Not applicable

## Coverage

**Requirements:**
- No enforced coverage target
- No coverage tooling configured

**Configuration:**
- Not applicable

## Test Types

**Unit Tests:**
- Not present
- Would require mocking game assemblies (complex)

**Integration Tests:**
- Not present
- Would require running SPT server + game client

**E2E Tests:**
- Manual testing only
- Run game, check items, verify checkmarks display correctly

## Common Patterns

**Current Testing Approach:**

1. Build solution in Visual Studio
2. DLLs automatically copied to `dist/` and optionally to local SPT installation
3. Launch SPT game
4. Navigate to stash/inventory
5. Verify checkmark colors and tooltips display correctly
6. Check F12 config menu works

**Testing Scenarios:**
- Items needed for quests (yellow checkmark)
- Items needed for hideout (red/green based on quantity)
- Items on wishlist (blue checkmark)
- Items for barters (magenta checkmark)
- Items for crafts (cyan checkmark)
- Priority system when item needed for multiple purposes
- Tooltip information accuracy
- New profile handling (empty quest data)

## Recommendations for Future Testing

**Unit Testing Challenges:**
- Heavy dependency on game assemblies (EFT.*, Assembly-CSharp)
- Harmony patches require runtime injection
- Unity types not easily mockable

**Potential Approaches:**
- Extract pure logic (color calculations, priority sorting) into testable classes
- Create integration test project that runs against live SPT server
- Use test profiles with known quest/hideout state

**Priority Areas for Tests:**
- `GetNeeded()` - hideout requirement logic
- `GetRemainingPrerequisiteCount()` - prerequisite calculation
- Priority sorting logic
- Color selection based on fulfillment state

---

*Testing analysis: 2025-01-11*
*Update when test infrastructure is added*
