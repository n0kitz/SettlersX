# SettlersX Architecture

This document describes the layer responsibilities, autoload order, signal flow, and tick
pipeline. Read before touching any autoload, manager, or cross-layer interface.

---

## Layer Overview

```
┌─────────────────────────────────────────────────────────┐
│  src/Views/   Godot Node subclasses. Thin. No state.    │
│               Subscribes to signals. Sends intents.     │
├─────────────────────────────────────────────────────────┤
│  src/Core/    Autoload bridge layer (Godot Nodes).      │
│               Drives the sim. Emits signals to views.   │
├─────────────────────────────────────────────────────────┤
│  src/Sim/     Pure C#. Deterministic. No Godot types.   │
│               Owns all game logic and state.            │
├─────────────────────────────────────────────────────────┤
│  src/Data/    Resource subclasses. Read-only at runtime.│
│               Loaded by ResourceDatabase on startup.    │
└─────────────────────────────────────────────────────────┘
         data/  .tres files + balance.json
```

---

## Layer Rules

### `src/Sim/`
- No `using Godot;` except `Vector2`, `Vector3`, `Vector2I`, `Vector3I`
- No `Node`, no `Resource`, no `[Export]`, no `[Signal]`
- All arithmetic uses `double`
- All public types must have at least one unit test in `tests/Sim/`
- Systems communicate via plain C# `event` delegates (not Godot signals)
- Enforced by `tests/Architecture/SimLayerIsolationTests.cs`

### `src/Views/`
- All classes are `public partial class X : Node` (or Node3D, Control, etc.)
- No game state — subscribe to signals, render what you receive
- Call Manager methods only to send player intents (not to read state)
- Always call `IsInstanceValid(this)` after any `await`
- Always unsubscribe signals in `_ExitTree`

### `src/Core/`
- Autoload Godot Nodes bridging Sim and Views
- Delegate ALL logic to `src/Sim/` types
- Emit signals via `EventBus` — no direct cross-Manager method calls
- New autoloads require explicit approval

### `src/Data/`
- `[GlobalClass] public partial class XyzDef : Resource`
- `[Export]` properties only — no methods, no logic
- Filename must match class name exactly (case-sensitive)

---

## Autoload Order

Registered in `project.godot` (order matters — later autoloads can reference earlier ones):

| Order | Name             | Namespace          | Role                                      |
|-------|------------------|--------------------|-------------------------------------------|
| 1     | EventBus         | SettlersX.Core     | Signal hub — must be first                |
| 2     | ResourceDatabase | SettlersX.Data     | Loads .tres assets — before managers need them |
| 3     | WorldSim         | SettlersX.Core     | Creates TickEngine, drives tick loop      |
| 4     | GameManager      | SettlersX.Core     | Match state, players, victory conditions  |
| 5     | InputRouter      | SettlersX.Views.Core | Translates InputEvents to intents       |

**Exit order** is reverse: InputRouter → GameManager → WorldSim → ResourceDatabase → EventBus.
This means EventBus is still valid when all other autoloads exit — safe to unsubscribe signals.

---

## Signal Flow

```
Player Input
    │
    ▼
InputRouter._UnhandledInput(event)
    │  calls intent method on Manager
    ▼
WorldSim.Paused = !WorldSim.Paused
    │
    ▼
TickEngine.Paused (pure C# flag)

─────── Each frame ───────

WorldSim._Process(delta)
    │
    ├─► TickEngine.Advance(delta)  ──► returns N ticks fired
    │
    └─► EventBus.EmitSignal(GameTicked, tickNumber)  [for each tick]
              │
              ▼
         MainView.OnGameTicked(tick)   ← subscribes in _Ready
              │
              ▼
         [update HUD, interpolate visual positions using WorldSim.Alpha]
```

---

## Tick Pipeline

Each `TickEngine` tick (default 10 Hz) processes systems in this order:

```
1. Input intents applied      ← InputRouter queues intents between ticks
2. AI decisions               ← AiManager (Phase 2+)
3. Pathfinding updates        ← PathfindingManager recalculates dirty paths
4. Production step            ← ProductionManager consumes inputs, advances timers
5. Transport step             ← CarrierManager moves carriers along paths
6. Consumption step           ← BuildingManager consumes arrived resources
7. Combat step                ← CombatManager (Phase 3+)
8. Diplomacy step             ← DiplomacyManager (Phase 4+)
9. View snapshot              ← WorldSim.EmitSignal(GameTicked)
```

Views must NOT read sim state between ticks. They receive a snapshot via signals and
interpolate visually using `WorldSim.Alpha` (fractional progress to next tick).

---

## Two-Map Model

See [ADR 0006](adr/0006-two-map-model.md). Two independent spatial systems:

| System       | Purpose                                   | Phase |
|--------------|-------------------------------------------|-------|
| `HexGrid`    | Building footprints, road connections, AStar3D pathfinding | 1 |
| `SectorGraph`| Polygon regions, ownership, VP scoring, capture mechanics  | 1 |

Do not merge these systems. HexGrid is for building placement; SectorGraph is for territory.

---

## Key Architectural Decisions

See `docs/adr/README.md` for the full list. Most important:

- [ADR 0001](adr/0001-sim-view-split.md) — Why the Sim/View split exists
- [ADR 0002](adr/0002-double-arithmetic.md) — Why `double` throughout sim
- [ADR 0004](adr/0004-multimesh-settlers.md) — Why MultiMesh, not CharacterBody3D
- [ADR 0005](adr/0005-astar3d-over-navagent.md) — Why AStar3D, not NavigationAgent3D
