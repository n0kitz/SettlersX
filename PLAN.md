# PLAN.md — SettlersX

## Current Phase
**Phase 1 — Map, Roads & One Carrier** ⏳ in review (awaiting CI green)

## Phase 1 — Map, Roads & One Carrier
**Status: Implementation complete, pending CI verification**

### Exit Criteria (ALL must pass before Phase 2)
- [x] Flat-top hex grid renders on a flat ground plane (PlaneMesh stand-in — see Notes)
- [x] Mouse raycast correctly identifies hex cell under cursor
- [x] SectorGraph exists with 3 placeholder sectors (owned by player 0)
- [x] Two placeholder Storehouses placeable on hex cells
- [x] Click-drag builds road between hexes, updating AStar3D graph
- [x] One carrier MultiMeshInstance3D walks a road path at 10Hz tick
- [x] Visual interpolation between ticks (smooth movement, alpha from TickEngine)
- [x] Space = pause/unpause, Period = single step
- [x] Debug HUD shows: tick number, paused state, carrier hex, carrier state
- [ ] Architecture isolation test still passes (CI to confirm)
- [ ] dotnet build produces zero warnings (CI to confirm — no local .NET SDK)
- [ ] All Sim unit tests pass without Godot runtime (CI to confirm)

### Phase 1 Notes
- **Terrain3D deferred to F5/polish.** Criterion 1 originally required Terrain3D
  but the addon is not installed. F1 ships a `PlaneMesh` ground in
  `HexGridView.cs`. Re-evaluate when heightmaps become necessary for gameplay.
- AStar3D mirror lives in `src/Views/World/RoadView.cs` and is fed by
  `EventBus.RoadBuilt`. The Sim's `RoadGraph` (`src/Sim/Pathfinding/RoadGraph.cs`)
  remains the authoritative source; carriers path via the pure-C# `HexAStar`.
- Tick pipeline currently exercises Input + Transport phases. AI / Pathfinding
  re-compute / Production / Consumption / Combat / Diplomacy phases are
  documented stubs in `WorldState.Tick()` and fill in across F2a, F2b, F3.
- Verification limited to static review: no .NET SDK was available in the
  authoring environment, so `dotnet build` / `dotnet test` were not executed
  locally. CI (`.github/workflows/tests.yaml`) is the gate.
- New `EventBus` signal added: `RoadBuilt(Vector2I a, Vector2I b)`.
- Tests added: `HexCoordTests`, `HexGridTests`, `SectorGraphTests`,
  `WorldStateTests`, `RoadGraphTests`, `HexAStarTests`, `IntentQueueTests`,
  `BuildingRegistryTests`, `CarrierTests`, `TransportSystemTests`.

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
