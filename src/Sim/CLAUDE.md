# Sim Layer

Pure C# only. No using Godot; except Vector2/3/2I/3I math structs.
No Node. No Resource. No [Export]. No [Signal].
All arithmetic uses double. Serialize as fixed-point at save boundary only.
Every public method has a test in tests/Sim/ mirroring source layout.
Systems communicate via plain C# events, not Godot signals.
Tick entry point: src/Core/WorldSim._Process → TickEngine.Advance → each system.

Current systems:
- Core/TickEngine — fixed-rate tick accumulator with visual alpha
(add entries here as systems are built — do NOT list planned/future systems)
