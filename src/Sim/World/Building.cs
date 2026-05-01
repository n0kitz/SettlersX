namespace SettlersX.Sim.World;

/// <summary>
/// A placed building. Phase 1: storehouses only. Future phases add production buildings,
/// houses, military buildings; the DefId string keys into <c>data/buildings/*.tres</c>.
/// </summary>
public sealed record Building(int Id, string DefId, HexCoord Origin, int OwnerPlayerId);
