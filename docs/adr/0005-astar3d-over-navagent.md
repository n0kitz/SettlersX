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

Use `Godot.AStar3D` for road-following pathfinding in the sim layer. Hex grid cells that have
roads are registered as AStar points. The sim calls `GetPointPath()` on state changes only
(not every tick). The resulting path array is stored on the carrier entity.

`NavigationAgent3D` and its avoidance are not used.

## Consequences

**Good:**
- Deterministic paths — same input always yields same route.
- O(E log V) per query, no per-frame O(n²) cost.
- Carriers on roads naturally avoid overlap by road capacity constraints.
- Path computation happens in sim layer — testable headlessly.

**Bad:**
- No dynamic obstacle avoidance (acceptable — roads are fixed, carriers yield by design).
- Must manually maintain AStar graph when roads are built/destroyed.

## Note

`AStar3D` is a Godot math utility class (`Godot.AStar3D`). Its use in `src/Sim/` is a narrow
exception to the Godot-free rule — flagged in the architecture test allowlist if needed.
