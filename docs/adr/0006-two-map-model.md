# ADR 0006: Two Separate Map Systems

## Status
Accepted

## Context
Settlers 7 uses a hex building grid for pathfinding and building placement,
AND organic polygon sectors for territory ownership and VP scoring.
Initial plan proposed hex-only — rejected by architectural review as it 
causes permanent visual and mechanical fidelity problems.

## Decision
Two independent systems from Phase 1:
1. HexGrid (src/Sim/World/HexGrid.cs) — flat-top hex, axial coordinates,
   Dictionary<Vector2I, HexCell>, building footprints, AStar3D points
2. SectorGraph (src/Sim/World/SectorGraph.cs) — polygon regions, 
   adjacency list, sector ownership, border rendering data

Sectors contain a set of hex cells. HexGrid does not know about sectors.
SectorGraph queries HexGrid for contained cells when needed.

## Consequences
+ Correct Settlers 7 visual identity (organic sector borders)
+ Independent evolution of grid vs territory systems
+ HexGrid pathfinding not coupled to sector rules
- Two systems to build and maintain in Phase 1
- More initial complexity before first playable milestone
