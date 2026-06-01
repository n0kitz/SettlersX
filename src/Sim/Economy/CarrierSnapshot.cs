using Godot;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Economy;

/// <summary>
/// View-facing per-tick snapshot of one carrier. PrevWorld and NextWorld are the
/// hex centres for the previous and current tick respectively; the view layer
/// linearly interpolates between them using <c>WorldSim.Alpha</c>. <c>Cargo</c>
/// is the resource the carrier is presently holding (Kind=None when empty).
/// </summary>
public sealed record CarrierSnapshot(
    int Id,
    Vector3 PrevWorld,
    Vector3 NextWorld,
    CarrierState State,
    HexCoord Hex,
    ResourceStack Cargo);
