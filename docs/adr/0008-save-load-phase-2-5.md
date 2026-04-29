# ADR 0008 — Save/Load Deferred to Phase 2.5

**Status:** Accepted  
**Date:** 2026-04-28

## Context

Save/load requires a stable, versioned schema for the entire sim state graph. In Phase 1, the
core sim types (HexGrid, SectorGraph, carriers, buildings, production chains) are still being
designed. Defining a save schema before these types stabilize would require frequent breaking
schema changes and migration functions.

## Decision

Save/load implementation is deferred to Phase 2.5 (after Phase 2 core gameplay is feature-complete
and the sim state graph is stable).

The save format will be:
- Newtonsoft.Json, versioned envelope: `{ "version": N, "sim": { ... } }`
- Atomic write: write to `.tmp` then `File.Move` (avoids corruption on crash)
- Fixed-point serialization for all double values (multiply by 1e6, store as long)
- Migration functions required for every schema bump (`IMigration` interface, applied on load)

## Consequences

**Good:**
- No wasted migration effort on an unstable schema.
- Phase 1-2 can freely refactor sim types without save compatibility burden.

**Bad:**
- No persistence in Phase 1-2 (acceptable — match sessions are short during development).

## Pre-Requisite

Before implementing save/load, all Phase 2 sim types must have `[JsonProperty]` annotations
reviewed and a schema version document created in `docs/save-schema-vN.md`.
