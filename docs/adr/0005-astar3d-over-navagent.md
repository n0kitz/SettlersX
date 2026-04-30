# ADR 0005 — AStar3D over NavigationAgent3D

**Status:** Accepted  
**Date:** 2026-04-28

## Context

NavigationAgent3D with avoidance has a hard performance limit: CPU cost is roughly O(n²) per
frame for n agents using RVO avoidance. Godot's NavigationServer3D documentation notes that
avoidance becomes impractical above ~10 simultaneous agents.

SettlersX carriers follow fixed road paths between buildings. They do not need real-time
dynamic avoidance — roads are the constraint. Pathfinding needs to be:
- Deterministic (same start/end always produces same path)
- Testable without Godot runtime
- Expressed as pure C# for the sim layer

## Decision

Use a **pure C# A\* implementation (`HexPathfinder`)** in `src/Sim/` for deterministic,
headlessly-testable road-following pathfinding. Hex grid cells with roads are registered as
graph nodes; the pathfinder queries on state changes only (not every tick) and stores the
resulting path array on the carrier entity.

A thin `RoadPathfinderBridge` in `src/Views/` may synchronize a `Godot.AStar3D` instance for
view-layer needs (e.g. debug visualization), but the sim never touches `Godot.AStar3D`.

`NavigationAgent3D` and its avoidance are not used anywhere.

## Consequences

**Good:**
- Deterministic paths — same input always yields same route.
- O(E log V) per query, no per-frame O(n²) cost.
- Carriers on roads naturally avoid overlap by road capacity constraints.
- Path computation stays in `src/Sim/` — fully testable without the Godot runtime.
- Preserves the strict Sim/View split; no Godot type leaks into the sim layer.

**Bad:**
- No dynamic obstacle avoidance (acceptable — roads are fixed, carriers yield by design).
- Must manually maintain the path graph when roads are built or destroyed.
