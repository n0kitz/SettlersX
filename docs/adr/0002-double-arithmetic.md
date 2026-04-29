# ADR 0002 — Double Arithmetic in Sim

**Status:** Accepted  
**Date:** 2026-04-28

## Context

A typical game session can run for 60+ minutes of in-game time. Float arithmetic accumulates
rounding error roughly proportional to time elapsed. In a tick-based economy sim, small float
errors in resource counts or production timers compound over thousands of ticks, eventually
causing visible discrepancies between clients in future multiplayer, or between save/load cycles.

## Decision

All arithmetic in `src/Sim/` uses `double` (64-bit IEEE 754). This applies to positions, speeds,
timers, resource fractions, and production progress. Integer resource counts (`int`) are used
only for whole-unit inventories where fractional state is not needed.

At save boundaries only, values are serialized as fixed-point integers (e.g. multiply by 1000,
round, store as `long`). This eliminates float representation issues in save files.

## Consequences

**Good:**
- Negligible drift across 60+ minute sessions.
- Deterministic across platforms (IEEE 754 double is consistent on x86/ARM).
- Save files contain exact reproducible state.

**Bad:**
- Slightly more memory per value vs. float (8 bytes vs. 4 bytes).
- Godot rendering API uses float — conversion needed at View boundary (acceptable; Views only
  read snapshot values once per frame).
