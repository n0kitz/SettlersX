# ADR 0003 — LogicBlocks for Unit and Building FSM

**Status:** Accepted  
**Date:** 2026-04-28

## Context

Units and buildings need state machines: a carrier can be idle, pathfinding, carrying, depositing.
A building can be constructing, active, starved, upgrading. We need testable, serializable FSMs.

Options considered:
- Hand-rolled `enum` + `switch` — simple but unstructured, hard to test transitions
- Stateless FSM library — no native C# record states
- **Chickensoft.LogicBlocks** — C# record states, Chickensoft ecosystem integration, testable

## Decision

Use `Chickensoft.LogicBlocks` for unit and building state machines. States are C# `record` types
nested inside the machine class. Input/output messages are also records. No Godot types leak
into state logic.

## Consequences

**Good:**
- States are pure C# records — serializable, testable without Godot.
- Chickensoft ecosystem compatibility (AutoInject, GodotNodeInterfaces).
- Clear transition graph; illegal transitions throw at runtime.
- Outputs decouple state changes from View updates.

**Bad:**
- Learning curve for contributors unfamiliar with LogicBlocks.
- Adds a dependency (`Chickensoft.LogicBlocks 5.*`).

## Scope

Applied to: CarrierFsm, BuildingFsm (Phase 1+). Not applied to simple one-state entities.
