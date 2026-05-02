# PLAN.md — SettlersX

## Current Phase
**Phase 2a — First Vertical Slice (One Full Chain)** ⏳ in review (awaiting CI green)

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
**Status: Implementation complete, pending CI verification**

### Exit Criteria
- [x] One complete chain functional: LumberCamp → Wood → Sawmill → Planks →
      ConstructionSite → House (Tree harvesting deferred — see Notes)
- [x] Job board matches supply offers to demand requests per tick
      (`JobBoard.TryFindJob` + reservation; `Refund` on path failure)
- [x] Carriers pick up and deliver goods along road graph
      (`TransportSystem` walks pickup→delivery path concat)
- [x] Resource counts visible in HUD per sector
      (`DebugHud` sums Stock per sector)
- [x] Chain-failure HUD indicator (`ProductionState.IsStarved` shown in HUD)
- [x] Production tick order respects pipeline:
      Input → Production → Transport (consumption folded into Production)
- [x] balance.json drives chain numbers (`chain` block)
- [x] All Sim unit tests pass, end-to-end chain test
      (`TransportSystemTests.Tick_FullChain_HouseEventuallyConstructs`)
- [ ] dotnet build produces zero warnings (CI to confirm)
- [ ] CI green on merged Sim + Architecture tests
- [ ] 10-minute playable session validated by hand (deferred to F2b polish)

### F2a Notes
- **Tree harvesting deferred to F2b.** F2a's LumberCamp is a raw producer
  (no input tile). Tree resources on hex tiles arrive with the map authoring
  pass in F2b alongside the second/third chains.
- **Sector inventory is sum across storehouses.** Multiple storehouses in
  the same sector pool stock for HUD totals. JobBoard treats them as a
  shared pool when matching demand to supply.
- **Reservation model.** JobBoard decrements Source on assignment, refunds
  on path failure, drops cargo on delivery to a finalized ConstructionSite.
  Overshoot of construction Delivered above Cost is intentional and bounded
  by carrier count; investigated for F2b if it becomes visible to players.
- **No Tree-harvesting visualisation.** LumberCamp shows green; no felled
  trees, no chopping animation. Visuals land in F6/polish.
- **One-hex footprint everywhere.** Multi-hex footprints arrive when sites
  need adjacent worker tiles (F2b).
- **EventBus.BuildingFinalized** added so the view can re-skin a site to a
  house without reading sim state.
- New keys: `1=Storehouse 2=LumberCamp 3=Sawmill 4=ConstructionSite`.

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

## Security Phase S1 — Build & Supply Chain Hardening
**Status: Implementation complete, pending CI verification**

### Exit Criteria
- [x] `RestorePackagesWithLockFile=true` in csproj
- [x] `Deterministic=true` and conditional `ContinuousIntegrationBuild` set
- [x] Newtonsoft.Json pinned to exact version (`13.0.3`)
- [x] CI runs `dotnet list package --vulnerable --include-transitive`
- [x] CodeQL workflow on push, PR, and weekly schedule
- [x] Dependabot config for NuGet + github-actions
- [x] `docs/security/threat-model.md` v0.1 with S1 STRIDE table
- [x] `docs/security/branch-rules.md` documents required GitHub settings
- [x] ADR 0009 written and listed in ADR README
- [x] Architecture test guards csproj supply-chain switches
- [ ] CI green on first run after this commit
- [ ] `packages.lock.json` generated and committed in follow-up

### S1 Notes
- Action SHA-pinning and SBOM generation are deferred follow-ups, tracked
  in `docs/security/threat-model.md` open follow-ups.
- Once `packages.lock.json` is committed, switch CI to
  `RestoreLockedMode=true` in a follow-up commit.
- Workflow `working-directory: SettlersX` mismatch (pre-existing) flagged
  in threat-model follow-ups for investigation.

---

## Completed Phases
### Phase 0 ✅
- Chickensoft scaffold, all packages installed
- Autoloads: EventBus, ResourceDatabase, WorldSim, GameManager, InputRouter
- TickEngine (pure C#, tested), architecture isolation test
- CLAUDE.md, PLAN.md, ADRs 0001-0008, slash commands, subagent
- Build green, all tests passing
