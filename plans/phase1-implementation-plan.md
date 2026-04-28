# Phase 1 Implementation Plan — Map, Roads & One Carrier

**Date:** 2026-04-28  
**Runtime:** Godot 4.6.2 / .NET 10 / net10.0  
**Status:** Awaiting approval — no code written yet

---

## 1. SYSTEMS LIST

Every new file, its purpose, and which exit criterion it satisfies.

### Sim Layer — Pure C# (no Godot except math structs)

| File | Purpose | Exit Criterion |
|------|---------|----------------|
| `src/Sim/World/HexMath.cs` | Static math: axial↔world, flat-top geometry, 6-neighbor directions, world→axial rounding | Underpins criteria 1, 2, 5, 6, 7 |
| `src/Sim/World/HexCell.cs` | Record: axial coord, hasRoad, hasBuilding, buildingId | Underpins criteria 1, 4, 5 |
| `src/Sim/World/HexGrid.cs` | Dictionary<Vector2I,HexCell>, Initialize(radius), road/building mutation, neighbor queries, C# event OnCellChanged | Criteria 1, 2, 4, 5, 10, 12 |
| `src/Sim/World/Sector.cs` | Record: id, ownerId, IReadOnlySet<Vector2I> hexCells, IReadOnlyList<int> adjacentSectorIds | Criterion 3 |
| `src/Sim/World/SectorGraph.cs` | List<Sector>, Initialize() with 3 hardcoded sectors, GetSectorAt(Vector2I), GetOwner(sectorId) | Criterion 3 |
| `src/Sim/World/RoadPath.cs` | Readonly record: IReadOnlyList<Vector2I> cells (ordered hex path) | Criteria 5, 6 |
| `src/Sim/World/HexPathfinder.cs` | Pure C# A* on HexGrid road connections, FindPath(from, to) → RoadPath? | Criteria 5, 6 |
| `src/Sim/World/CarrierState.cs` | Enum: Idle, Walking | Criteria 6, 9 |
| `src/Sim/World/Carrier.cs` | Pure C# class: id, currentHex, path, progress along edge (double), state; Tick(tickInterval), InterpolatedPosition(alpha, grid) → Vector3 | Criteria 6, 7, 9, 12 |
| `src/Sim/World/WorldState.cs` | Root sim state: HexGrid, SectorGraph, carriers list, buildings dict; Tick(tickInterval) drives carrier updates | Criteria 3–9 |

**Update (not new):**
| File | Change | Criterion |
|------|--------|-----------|
| `src/Sim/Core/WorldSim.cs` | Add `WorldState` field; call `WorldState.Tick()` per tick; expose `WorldState` property for views | Criteria 6, 7 |

### Data Layer — Resource Defs

| File | Purpose | Exit Criterion |
|------|---------|----------------|
| `src/Data/Buildings/StorehouseDef.cs` | [GlobalClass] Resource: DisplayName, FootprintRadius (int = 1) | Criterion 4 |
| `data/buildings/Storehouse.tres` | .tres with default values for StorehouseDef | Criterion 4 |

### Views Layer — Godot Nodes

| File | Purpose | Exit Criterion |
|------|---------|----------------|
| `src/Views/World/HexGridView.cs` | Node3D: renders hex grid as ImmediateMesh outline cells, highlights hovered cell, road cells styled differently | Criteria 1, 2, 5 |
| `src/Views/World/SectorBorderView.cs` | Node3D: renders sector borders as line mesh (3 placeholder boundary polygons) | Criterion 3 |
| `src/Views/World/HexRaycaster.cs` | Node3D: per-frame camera ray vs. Y=0 plane → world pos → HexMath.WorldToAxial → hex coord; C# event OnHexHovered(Vector2I), OnHexClicked(Vector2I) | Criterion 2 |
| `src/Views/World/RoadPathfinderBridge.cs` | Node: owns Godot AStar3D; subscribes to HexGrid.OnCellChanged; adds/removes AStar3D points as roads are built; provides GetPath(from,to)→RoadPath for external use | Criterion 5 ("updating AStar3D graph") |
| `src/Views/World/RoadBuilderView.cs` | Node3D: click-drag input → paint road on each hovered hex → calls WorldState.HexGrid.SetRoad(); highlights drag preview | Criterion 5 |
| `src/Views/World/BuildingPlacementView.cs` | Node3D: click on valid hex → places Storehouse via WorldState; enforces max 2 in Phase 1 | Criterion 4 |
| `src/Views/World/CarrierView.cs` | MultiMeshInstance3D (N=1): each _Process frame, reads Carrier.InterpolatedPosition(alpha) and sets transform; NO CharacterBody3D | Criteria 6, 7 |
| `src/Views/UI/DebugHudView.cs` | CanvasLayer: 4 labels updated each _Process: tick number, paused state, carrier hex (Vector2I), carrier state (enum name) | Criterion 9 |

### Input Handling (in existing files)

| Location | Change | Criterion |
|----------|--------|-----------|
| `src/Sim/Core/WorldSim.cs` `_UnhandledInput` | Space already wired to "ui_accept"; add Period via `Input.IsPhysicalKeyJustPressed(Key.Period)` in `_Process`, calls `Engine.StepOnce()` | Criterion 8 |

### Scenes

| File | Purpose | Criterion |
|------|---------|-----------|
| `scenes/ui/DebugHud.tscn` | CanvasLayer scene with 4 Label nodes | Criterion 9 |
| Update `scenes/world/Main.tscn` | Add HexGridView, SectorBorderView, HexRaycaster, RoadPathfinderBridge, RoadBuilderView, BuildingPlacementView, CarrierView; attach DebugHud as child | All criteria |

### Test Files

| File | Tests | Criterion |
|------|-------|-----------|
| `tests/Sim/World/HexMathTests.cs` | axial↔world round-trips, neighbor directions, flat-top geometry | Criterion 12 |
| `tests/Sim/World/HexGridTests.cs` | Initialize radius, SetRoad/SetBuilding, GetNeighbors, event fires | Criterion 12 |
| `tests/Sim/World/SectorGraphTests.cs` | 3 sectors init, GetSectorAt returns correct sector, adjacency correct | Criteria 3, 12 |
| `tests/Sim/World/HexPathfinderTests.cs` | Path found on connected road, null when disconnected, correct cell count | Criteria 5, 12 |
| `tests/Sim/World/CarrierTests.cs` | Tick advances position, reaches next hex, wraps path, InterpolatedPosition in [0,1] range, stays idle when no path | Criteria 6, 7, 12 |

**Total new files: 26** (21 new + 3 updated + 2 scenes)

---

## 2. IMPLEMENTATION ORDER

Work bottom-up: Sim first, Views last. Build and test each Sim file before the next.

### Stage A — Hex Math Foundation
**⚠️ HIGHEST RISK STAGE — mistakes here invalidate everything above it**

```
Step 1: HexMath.cs + HexMathTests.cs
Step 2: HexCell.cs (no test needed, trivial record)
Step 3: HexGrid.cs + HexGridTests.cs
```

**Why first:** Every other file depends on axial coordinates and world positions. A wrong formula in HexMath (e.g., confusing flat-top with pointy-top, or wrong size interpretation) will cause the rendered grid to be misaligned with raycasts, which misaligns road building, which misaligns carrier movement. Fix before moving on — this is the load-bearing wall.

Coordinate convention for flat-top, size=1.0 (from balance.json):
```
World X = size * (3/2 * q)
World Z = size * (√3/2 * q + √3 * r)
```
Y is always 0 in sim (view adds terrain height later). Tests will verify the round-trip: WorldToAxial(AxialToWorld(q,r)) == (q,r).

### Stage B — Sector Graph
```
Step 4: Sector.cs (trivial record)
Step 5: SectorGraph.cs + SectorGraphTests.cs
```

**Why here:** Independent of roads and carriers. Simple enough to do early. The 3 placeholder sectors will be hardcoded cell ranges (e.g., q < -2, q ∈ [-2,2], q > 2 on a radius-6 grid).

### Stage C — Pathfinding
```
Step 6: RoadPath.cs (trivial record)
Step 7: HexPathfinder.cs + HexPathfinderTests.cs
```

**Why here:** Needs HexGrid but not Carrier. Tests can set up a mini road network and verify paths. Pure C# A* — no Godot. This is tested in isolation before the carrier uses it.

### Stage D — Carrier Sim
```
Step 8: CarrierState.cs (trivial enum)
Step 9: Carrier.cs + CarrierTests.cs
Step 10: WorldState.cs
Step 11: Update WorldSim.cs (add WorldState, wire ticking, expose Alpha)
```

**Why here:** Carrier depends on HexGrid (for world positions) and RoadPath (for movement). WorldState integrates all sim pieces. After step 11, the complete sim loop works: TickEngine → WorldState.Tick() → Carrier moves — all testable without Godot.

**⚠️ RISK:** Carrier position arithmetic. The carrier moves at `base_speed_cells_per_second = 2` cells/second. At 10Hz, that is `2 / 10 = 0.2` cells per tick. `progress` advances 0.2 per tick. When `progress >= 1.0`, consume 1.0 and move to next hex. `InterpolatedPosition(alpha)` lerps between current hex center and next hex center using `(progress + alpha * 0.2)`. This must be verified in tests before visual work starts.

### Stage E — StorehouseDef
```
Step 12: StorehouseDef.cs
Step 13: data/buildings/Storehouse.tres
```

**Why here:** Before Views, so ResourceDatabase can load it. No test needed (just [Export] properties).

### Stage F — Basic Rendering (Grid + Sectors)
```
Step 14: HexGridView.cs
Step 15: SectorBorderView.cs
Step 16: Update Main.tscn — add HexGridView, SectorBorderView
```

**Why here:** Get visual output on screen before adding input. Validate hex positions visually. HexGridView uses ImmediateMesh to draw hex outlines; SectorBorderView draws 3 colored line loops.

### Stage G — Mouse Input
```
Step 17: HexRaycaster.cs
Step 18: Update Main.tscn — add HexRaycaster
```

**Why here:** Ray vs. flat plane (Y=0) is simple. Validate that hovering highlights the correct hex. This must work before road building or placement.

### Stage H — Road Building
```
Step 19: RoadPathfinderBridge.cs (wraps AStar3D, synced to HexGrid.OnCellChanged)
Step 20: RoadBuilderView.cs
Step 21: BuildingPlacementView.cs
Step 22: Update Main.tscn — add remaining views
```

**Why here:** Road building requires working raycaster. RoadPathfinderBridge must be created before RoadBuilderView because the bridge registers AStar3D points when roads are added.

**⚠️ RISK:** Road building is the only UI interaction where HexGrid state changes at runtime. Must ensure HexGrid.OnCellChanged fires reliably and RoadPathfinderBridge updates AStar3D in the same frame.

### Stage I — Carrier View + Interpolation
```
Step 23: CarrierView.cs (MultiMeshInstance3D)
Step 24: Update Main.tscn — add CarrierView
Step 25: Manually trigger carrier to walk a path (auto-triggered when two Storehouses placed and road connects them)
```

**Why here:** Carrier sim is fully tested by now. View just reads `WorldSim.Instance.WorldState.Carriers[0].InterpolatedPosition(WorldSim.Instance.Alpha)`. The MultiMesh transform is set each `_Process` frame.

### Stage J — Debug HUD + Input Finalization
```
Step 26: DebugHudView.cs
Step 27: scenes/ui/DebugHud.tscn
Step 28: Update WorldSim._Process / _UnhandledInput — add Period key for StepOnce
Step 29: Update Main.tscn — add DebugHud
```

**Why last:** Cosmetic. Does not block any other criterion.

### Stage K — Verification Pass
```
Step 30: Run /regression-check (all tests pass, zero warnings)
Step 31: Manual walkthrough of all 12 exit criteria
Step 32: Update PLAN.md, update src/Sim/CLAUDE.md systems list
Step 33: Commit
```

---

## 3. TWO-MAP ARCHITECTURE CONFIRMATION

### Separation Contract

`HexGrid` and `SectorGraph` are **completely ignorant of each other**.

```
HexGrid                            SectorGraph
──────────────────────────         ──────────────────────────
Dictionary<Vector2I,HexCell>       List<Sector>
GetNeighbors(axial)                GetSectorAt(axial) → Sector?
SetRoad(axial, bool)               GetOwner(sectorId) → int
GetCell(axial) → HexCell?          GetAdjacent(sectorId) → IReadOnlyList<int>
event OnCellChanged                (no event in Phase 1)

No reference to SectorGraph        No reference to HexGrid
No reference to HexGrid            No reference to HexCell
```

### How a Sector Knows Its Cells

`Sector` holds a `HashSet<Vector2I>` of axial coordinates that belong to it. These are set at initialization. In Phase 1, the three sectors are hardcoded:

```
Sector 0 (player 0): q ∈ [-6, -3], all valid r values in the grid radius
Sector 1 (player 0): q ∈ [-2,  2], all valid r values
Sector 2 (player 0): q ∈ [ 3,  6], all valid r values
```

`SectorGraph.GetSectorAt(Vector2I axial)` iterates `_sectors` and checks `sector.HexCells.Contains(axial)`. This is O(N_sectors) lookups, fast for 3 sectors. In Phase 2+, replace with a dictionary reverse-mapping.

**Key invariant:** `SectorGraph` never calls any `HexGrid` method. If Phase 2 logic needs "which sector does this cell's road belong to", that query runs through `SectorGraph.GetSectorAt`, not through any coupling in `HexGrid`.

### How the Pathfinder Uses HexGrid Without Knowing Sectors

`HexPathfinder.FindPath(Vector2I from, Vector2I to, HexGrid grid)` receives `HexGrid` only.

The A* algorithm:
1. Gets neighbors via `grid.GetNeighbors(current)`
2. Filters to neighbors where `grid.GetCell(neighbor)?.HasRoad == true`
3. Heuristic: `HexMath.AxialDistance(current, goal)` (hex grid distance = max of |dq|, |dr|, |ds|)
4. Returns `RoadPath` with the ordered list of hex coordinates

**No sector reference anywhere in HexPathfinder.** It does not know or care which sector a cell belongs to. Sector rules (e.g., "can only path through owned sectors") are Phase 3 concerns and will be added by passing a predicate filter into FindPath at that point, not by coupling the two systems now.

---

## 4. CARRIER RENDERING APPROACH

### MultiMeshInstance3D Setup

`CarrierView` is a `MultiMeshInstance3D` node. Phase 1 uses N=1 instances.

```
_mesh = new MultiMesh();
_mesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
_mesh.InstanceCount = 1;
_mesh.Mesh = /* a simple CapsuleMesh or BoxMesh placeholder */;
MultiMesh = _mesh;
```

### Per-Frame Update (NO CharacterBody3D)

Each `_Process(double delta)` frame:

```csharp
var alpha = WorldSim.Instance.Alpha;            // double [0,1] from TickEngine
var carrier = WorldSim.Instance.WorldState.Carriers[0];
var worldPos = carrier.InterpolatedPosition(alpha, WorldSim.Instance.WorldState.HexGrid);
var t = Transform3D.Identity;
t.Origin = worldPos;
_mesh.SetInstanceTransform(0, t);
```

### Visual Interpolation

`Carrier.InterpolatedPosition(double alpha, HexGrid grid)`:

```
currentCenter = HexMath.AxialToWorld(currentHex)
nextCenter    = HexMath.AxialToWorld(path.Cells[pathIndex + 1])  // or currentHex if at end
t = Clamp(progress + alpha * tickProgressIncrement, 0, 1)
return Lerp(currentCenter, nextCenter, t)
```

Where `tickProgressIncrement = tickInterval * speed` (= 0.1 * 2.0 = 0.2 cells/tick from balance.json). `alpha` comes from `TickEngine.Alpha = _accumulator / TickInterval`, in range [0,1].

**Result:** Position is smoothly interpolated between the last physics tick and where it will be at the next physics tick. No stutter at 10Hz.

**Confirmed: NO CharacterBody3D.** The carrier has no physics body. It is a visual placeholder (transform on a MultiMesh instance) driven entirely by sim arithmetic.

---

## 5. TEST PLAN

### When Tests Are Written

Tests are written **immediately after** each Sim class, before moving to the next stage. No Sim file ships without at least one test.

### Test Inventory

**HexMathTests.cs** (Stage A)
- `AxialToWorld_Origin_ReturnsZeroVector`
- `AxialToWorld_Q1R0_CorrectFlatTopPosition` — verifies (3/2, 0, √3/2)
- `WorldToAxial_RoundTrip_IsIdentity` — 20 random axial coords
- `GetNeighbors_CenterCell_ReturnsSixNeighbors`
- `AxialDistance_Adjacent_IsOne`
- `AxialDistance_SameCell_IsZero`

**HexGridTests.cs** (Stage A)
- `Initialize_Radius3_HasCorrectCellCount` — radius 3 hex grid = 37 cells
- `SetRoad_ValidCell_CellHasRoad`
- `SetRoad_InvalidCell_ThrowsOrIgnores` — boundary behavior
- `GetNeighbors_EdgeCell_ReturnsOnlyExistingNeighbors`
- `OnCellChanged_FiresWhenRoadSet`

**SectorGraphTests.cs** (Stage B)
- `Initialize_ThreeSectors_AllOwnedByPlayer0`
- `GetSectorAt_CellInSector0_ReturnsSector0`
- `GetSectorAt_CellInSector1_ReturnsSector1`
- `GetSectorAt_CellInSector2_ReturnsSector2`
- `GetSectorAt_UnknownCell_ReturnsNull`
- `Sectors_DoNotOverlap` — no cell in two sectors simultaneously

**HexPathfinderTests.cs** (Stage C)
- `FindPath_NoRoads_ReturnsNull`
- `FindPath_DirectRoad_ReturnsTwoCells`
- `FindPath_LShapedRoad_ReturnsCorrectPath`
- `FindPath_DisconnectedGraph_ReturnsNull`
- `FindPath_SameCell_ReturnsOneCell`

**CarrierTests.cs** (Stage D)
- `Tick_NoPath_StaysIdle`
- `Tick_WithPath_AdvancesProgress`
- `Tick_ProgressReachesOne_MovesToNextHex`
- `Tick_ReachesEndOfPath_BecomesIdle`
- `InterpolatedPosition_AlphaZero_AtCurrentHexCenter`
- `InterpolatedPosition_AlphaOne_AtNextHexCenter`
- `InterpolatedPosition_AlphaMidpoint_BetweenHexes`

### Which Exit Criteria Need Tests to Prove Them

| Criterion | Proof |
|-----------|-------|
| Architecture isolation still passes | Existing `SimLayerIsolationTests` — ensure no new Sim files import Godot types beyond math |
| dotnet build zero warnings | CI: `dotnet build` exit code |
| All Sim unit tests pass without Godot | `dotnet test --filter "Category!=Integration"` — all 6 test suites |
| Carrier walks path at 10Hz | `CarrierTests` — `Tick_ProgressReachesOne_MovesToNextHex` proves 0.2 progress/tick |
| Visual interpolation | `CarrierTests` — `InterpolatedPosition_*` tests prove alpha math |
| HexGrid renders correctly | Visual inspection (no automated test for rendering) |
| Mouse raycast correct | Manual verification (automated would require Godot runtime) |
| Space/Period keys | Manual verification |
| Debug HUD | Manual verification |

---

## 6. RISKS & QUESTIONS

### Q1 — Terrain3D: Plugin or Flat Plane?

**Exit criterion:** "Flat-top hex grid renders on Terrain3D plane"

**Issue:** Terrain3D is a third-party Godot addon (github.com/TokisanGames/Terrain3D). It is not installed. Using it requires adding it to `addons/`, enabling it in project.godot (requires approval), and learning its API.

**Proposal:** For Phase 1, use a simple flat `MeshInstance3D` with a `PlaneMesh` (size 20×20) as the ground. The hex grid renders on top of this. Call it "placeholder terrain". Add real Terrain3D in a future polish phase.

**Question for you:** Is Terrain3D required for Phase 1 exit, or is a flat plane acceptable?

---

### Q2 — AStar3D Location (Sim vs. View Bridge)

**Issue:** Godot's `AStar3D` is a Godot class. Placing it in `src/Sim/` would violate Sim layer isolation (the architecture test would catch it). But the exit criterion says "updating AStar3D graph".

**Proposal:** 
- `HexPathfinder` (Sim) — pure C# A* implementation, no AStar3D
- `RoadPathfinderBridge` (Views) — owns Godot `AStar3D`, subscribes to `HexGrid.OnCellChanged`, adds/removes AStar3D points when roads are built. The exit criterion "updating AStar3D graph" is satisfied here.
- Carrier path comes from `HexPathfinder` (pure C#), not from the View bridge.

**Question:** Is this separation acceptable, or should AStar3D be in a different location?

---

### Q3 — Road Building UX: Paint vs. Point-to-Point

**Exit criterion:** "Click-drag builds road between hexes"

**Ambiguity:** Three interpretations:
- **(a) Paint mode:** Click and hold, road is built on every hex you drag over. Simplest to implement.
- **(b) Endpoint selection:** Click hex A, then click hex B; pathfinder finds and builds the shortest road between them.
- **(c) Manual chain:** Each click adds a hex to a chain, double-click confirms.

**Proposal:** Implement (a) paint mode for Phase 1 — it's the simplest and gives direct control. Option (b) requires the pathfinder to be complete and working (depends on Stage C). If you want (b), I can do that instead; it just needs to come after HexPathfinder is working.

**Question:** Which road building UX do you want?

---

### Q4 — Carrier Trigger: Automatic or Manual?

**Exit criterion:** "One carrier MultiMeshInstance3D walks a road path"

**Ambiguity:** How does the carrier get its path?
- **(a) Automatic:** When two Storehouses are placed and a road connects them, the carrier immediately starts patrolling between them.
- **(b) Hardcoded at startup:** Carrier gets a fixed path assigned in WorldState.Initialize() for demo purposes.
- **(c) Manual trigger:** A debug button or key press assigns the path.

**Proposal:** (b) hardcoded path for Phase 1 — a road must exist first (built by the player), then the carrier walks it. Once Storehouses are placed and road connects them, WorldState automatically assigns a path. This tests the complete loop without a UI for carrier management.

**Question:** Is automatic path assignment on storehouse+road connection acceptable for Phase 1?

---

### Q5 — Sector Cell Boundaries: Hardcoded or Procedural?

**Exit criterion:** "SectorGraph exists with 3 placeholder sectors (owned by player 0)"

**Proposal:** Three hardcoded column bands on a radius-6 grid:
- Sector 0: q ≤ -3
- Sector 1: -2 ≤ q ≤ 2  
- Sector 2: q ≥ 3

These are simple and visually clear. Each sector has a different tint in SectorBorderView.

**Question:** Is this hardcoding acceptable for Phase 1, or should sectors be loaded from a data file?

---

### Q6 — Period Key Input Action

**Exit criterion:** "Period = single step"

**Issue:** Adding input actions to project.godot is in the deny list (`Edit(./project.godot)`). The `WorldSim._UnhandledInput` already handles "ui_accept" (Space). Period is not a default Godot action.

**Proposal:** In `WorldSim._Process`, check `Input.IsPhysicalKeyJustPressed(Key.Period)` directly — no project.godot edit needed.

**No question — this is the correct approach.** Noting it here for transparency.

---

### Q7 — Hex Grid Radius

**Not in balance.json.** How large should the Phase 1 grid be?

**Proposal:** Radius 6 (271 cells). Large enough to see sector boundaries, small enough to render instantly. Configurable via balance.json later.

**Question:** Is radius 6 correct, or do you have a preferred size?

---

## 7. SESSION ESTIMATE

Rough breakdown by stage:

| Session | Stages | Content |
|---------|--------|---------|
| 1 | A + B | HexMath, HexGrid, SectorGraph + all tests (pure Sim, no Godot needed) |
| 2 | C + D | HexPathfinder, Carrier, WorldState, WorldSim integration + tests |
| 3 | E + F | StorehouseDef, HexGridView, SectorBorderView — first visual output |
| 4 | G + H | HexRaycaster, RoadPathfinderBridge, RoadBuilderView, BuildingPlacement |
| 5 | I + J | CarrierView (MultiMesh), visual interpolation, DebugHUD, Period key |
| 6 | K | Verification pass, exit criteria checklist, PLAN.md update, commit |

**Estimated: 5–6 sessions.**

Sessions 1 and 2 can potentially be combined if the Sim code flows without issues (they're all pure C#, no Godot friction). Session 4 is the most uncertain — road building UI involves input handling, which can be fiddly in Godot.

The critical path bottleneck is Session 1 (HexMath correctness). If that's solid, sessions 2–3 follow cleanly.

---

*Plan created: 2026-04-28 | Awaiting approval before coding starts.*
