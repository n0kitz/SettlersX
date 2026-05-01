# Sim Layer

Pure C# only. No using Godot; except Vector2/3/2I/3I math structs.
No Node. No Resource. No [Export]. No [Signal].
All arithmetic uses double. Serialize as fixed-point at save boundary only.
Every public method has a test in tests/Sim/ mirroring source layout.
Systems communicate via plain C# events, not Godot signals.
Tick entry point: src/Core/WorldSim._Process → TickEngine.Advance → each system.

Current systems:
- Core/TickEngine — fixed-rate tick accumulator with visual alpha
- World/HexCoord, HexGrid, SectorId, SectorGraph — two-map model (ADR 0006)
- World/Building, BuildingRegistry — placed buildings indexed by hex
- World/IntentQueue + Intents/* — drained at the Input phase of each tick
- World/WorldState — root container; runs the Input → Transport phases of the tick pipeline
- Pathfinding/RoadGraph, HexAStar — adjacency graph + deterministic A*
- Economy/Carrier, CarrierState, TransportSystem, CarrierSnapshot — one-carrier shuttle (F1)
(add entries here as systems are built — do NOT list planned/future systems)
