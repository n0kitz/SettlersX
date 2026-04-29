# ADR 0001 — Sim/View Split

**Status:** Accepted  
**Date:** 2026-04-28

## Context

The game needs to be testable without a running Godot runtime. Sim logic (pathfinding, production,
AI) must be deterministic and replayable. Views must be replaceable without touching logic.
Save/load requires a serializable state graph with no Node references.

## Decision

All game logic lives in `src/Sim/` as pure C# classes with no `using Godot;` (except math structs
Vector2/3/2I/3I). All rendering and input handling lives in `src/Views/` as Godot Node subclasses.
The two layers communicate only via Godot signals emitted through `EventBus` autoload or via
direct property reads on Manager autoloads.

## Consequences

**Good:**
- Sim tests run headless via `dotnet test` — no Godot process, fast CI.
- Sim state is serializable to JSON without GodotObject references.
- View layer is fully replaceable (e.g. swapping 3D for 2D map).
- Deterministic replay is possible (replay event log against pure-C# sim).

**Bad:**
- Extra indirection: views cannot call sim methods directly.
- Requires discipline — architecture test (`SimLayerIsolationTests`) enforces the boundary.

## Enforcement

`tests/Architecture/SimLayerIsolationTests.cs` reflects over the `SettlersX.Sim` namespace and
fails the build if any Godot type (beyond math structs) is found in fields, properties, methods,
or base classes.
