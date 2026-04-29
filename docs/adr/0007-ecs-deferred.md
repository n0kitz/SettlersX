# ADR 0007 — ECS Deferred to Phase 2+

**Status:** Accepted  
**Date:** 2026-04-28

## Context

Entity Component System (ECS) frameworks like `Friflo.Engine.ECS` offer excellent cache locality
and bulk-update performance for large entity counts. They are most valuable when:
- Entity counts exceed ~1000 (carriers, trees, resource piles)
- Bulk system updates dominate frame time (pathfinding, production ticks)

In Phase 1, the entity count is small (≤ 100 carriers, ~30 buildings). Introducing an ECS
framework before profiling adds complexity and a dependency with no measured benefit.

## Decision

Do not add `Friflo.Engine.ECS` or any ECS framework in Phase 1.

Entities are represented as plain C# classes or records in `src/Sim/`. If profiling in Phase 2+
shows that tick processing is a bottleneck and entity count is high enough to justify ECS, we
will add Friflo then.

The sim architecture (pure C# records, no Godot types) is ECS-compatible — migration would not
require touching `src/Views/`.

## Consequences

**Good:**
- Less complexity in Phase 1.
- No premature optimization.
- Architecture remains migration-compatible.

**Bad:**
- May require a refactor in Phase 2+ if entity counts grow unexpectedly.

## Trigger for Revisiting

Add ECS if `dotnet-trace` shows > 5% frame time in sim entity iteration with > 500 active
entities in a Phase 2 profiling session.
