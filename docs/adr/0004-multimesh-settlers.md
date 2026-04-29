# ADR 0004 — MultiMesh for Settler Units

**Status:** Accepted  
**Date:** 2026-04-28

## Context

A typical SettlersX map has 30–100+ carriers active at once, all walking paths between buildings.
Using a `CharacterBody3D` per unit means:
- 100 physics objects with collision shapes
- NavigationAgent3D avoidance breaks above ~10 agents (CPU cost is O(n²))
- Draw calls: one per unit mesh

This approach fails at scale.

## Decision

Render all settler units using a single `MultiMeshInstance3D` per unit type. Position and rotation
are updated from sim state every frame via `RenderingServer.MultimeshInstanceSetTransform()`.
Collision is handled by `PhysicsServer3D` with separate collision shapes (no CharacterBody3D).
Pathfinding uses AStar3D (see ADR 0005) — no NavigationAgent3D avoidance.

## Consequences

**Good:**
- 100 settlers = 1 draw call per unit type.
- No O(n²) avoidance CPU cost.
- View layer (MultiMesh) is fully decoupled from sim pathfinding.

**Bad:**
- No automatic Godot collision response — must manage physics shapes manually.
- Animations require custom shader or skeleton instancing.
- More complex view code than CharacterBody3D.

## Rule

`CharacterBody3D` is banned for anything that may have more than 3 simultaneous instances.
The architecture subagent (`godot-reviewer`) enforces this as a CRITICAL violation.
