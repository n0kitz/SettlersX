# PLAN.md — SettlersX

## Current Phase
**Phase 0 — Bootstrap & Setup** ✅

## Phase 1 — Map, Roads & One Carrier
**Status: Not started**

### Exit Criteria (ALL must pass before Phase 2)
- [ ] Flat-top hex grid renders on Terrain3D plane
- [ ] Mouse raycast correctly identifies hex cell under cursor
- [ ] SectorGraph exists with 3 placeholder sectors (owned by player 0)
- [ ] Two placeholder Storehouses placeable on hex cells
- [ ] Click-drag builds road between hexes, updating AStar3D graph
- [ ] One carrier MultiMeshInstance3D walks a road path at 10Hz tick
- [ ] Visual interpolation between ticks (smooth movement, alpha from TickEngine)
- [ ] Space = pause/unpause, Period = single step
- [ ] Debug HUD shows: tick number, paused state, carrier hex, carrier state
- [ ] Architecture isolation test still passes
- [ ] dotnet build produces zero warnings
- [ ] All Sim unit tests pass without Godot runtime

### Phase 1 Notes
(Add notes here during work)

---

## Phase 2a — First Vertical Slice (One Full Chain)
**Status: Not started**

### Exit Criteria
- [ ] One complete chain functional: Tree → Lumber Camp → Wood → 
      Sawmill → Planks → Constructor → House built
- [ ] Job board matches supply offers to demand requests per tick
- [ ] Carriers pick up and deliver goods along road graph
- [ ] Resource counts visible in HUD per sector
- [ ] 10-minute playable session possible on one sector
- [ ] Chain-failure HUD indicator (blocked input flashing icon)
- [ ] All sim tests pass, coverage on chain logic >80%

---

## Phase 2b — Economy Generalisation + Stub Bot
**Status: Not started**

### Exit Criteria
- [ ] Five raw resources: Wood, Stone, Grain, Iron Ore, Gold Ore
- [ ] Five processed goods: Planks, Flour, Bread, Iron, Coins
- [ ] All production buildings for above chains functional
- [ ] Job board handles all goods generically (not hardcoded to wood)
- [ ] Stub bot opponent exists: builds randomly, doesn't strategise
- [ ] Two-player game loop functional (human vs stub bot)

---

## Phase 2.5 — Save/Load (Mandatory before Phase 3)
**Status: Not started**

### Exit Criteria
- [ ] Full sim state serialises to JSON (Newtonsoft.Json)
- [ ] Versioned save envelope with schema migration hook
- [ ] Atomic write to user://saves/slot0.json
- [ ] Full round-trip save → load reproduces identical sim state
- [ ] All bugs introduced by Phase 2 are reproducible via save files
- [ ] /regression-check passes on a loaded save

---

## Phase 3 — Sector Ownership & Three-Path Capture
**Status: Not started**

### Exit Criteria
- [ ] Sector ownership enforced (storehouse required for production)
- [ ] Military capture: general + soldiers can claim adjacent sector
- [ ] Proselytism: clerics traverse all sectors
- [ ] Bribery: traders claim neutral sectors
- [ ] Sector border overlay shader renders correctly
- [ ] Two-player game validates all three capture paths

---

## Phase 4 — Victory Paths & Tech Trees
**Status: Not started**

---

## Phase 5 — Real Bot AI
**Status: Not started**

---

## Phase 6 — Polish, Fog of War, Campaign
**Status: Not started**

---

## Completed Phases
### Phase 0 ✅
- Chickensoft scaffold, all packages installed
- Autoloads: EventBus, ResourceDatabase, WorldSim, GameManager, InputRouter
- TickEngine (pure C#, tested), architecture isolation test
- CLAUDE.md, PLAN.md, ADRs 0001-0008, slash commands, subagent
- Build green, all tests passing
