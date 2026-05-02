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
- World/Building, BuildingKind, BuildingSpec, BuildingCatalog, BuildingRegistry —
  placed buildings indexed by hex; sub-states (Stock / Production / Construction)
  attached by catalog spec at placement time
- World/IntentQueue + Intents/* — drained at the Input phase of each tick
- World/WorldState — root container; runs Input → Production → Transport phases
- Pathfinding/RoadGraph, HexAStar — adjacency graph + deterministic A*
- Economy/ResourceKind, ResourceStack, Inventory, Recipe, ConstructionState —
  resource taxonomy + per-building containers
- Economy/Job, JobBoard — stateless dispatcher; reserves source stock at
  assignment to prevent double-booking; Refund() restores when path fails
- Economy/Carrier, CarrierState, CarrierSnapshot — carrier holds Cargo + CurrentJob,
  belongs to a home storehouse (Settlers 7 rule)
- Economy/TransportSystem — spawns carriers per storehouse quota, walks paths,
  delivers cargo on Arrived, asks JobBoard for next dispatch when Idle
- Production/ProductionState, ProductionSystem — per-tick recipe timer; finalizes
  completed Construction sites by promoting them via the catalog
(add entries here as systems are built — do NOT list planned/future systems)
