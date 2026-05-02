namespace SettlersX.Sim.World;

/// <summary>
/// High-level role tag used by sim systems to dispatch on a building's behaviour.
/// Sister to <see cref="Building.DefId"/> — DefId is the granular identifier for
/// data lookups; Kind is the bucket the sim cares about. Persisted as int.
/// </summary>
public enum BuildingKind
{
  Unknown = 0,
  Storehouse = 1,
  LumberCamp = 2,
  Sawmill = 3,
  ConstructionSite = 4,
  House = 5,
}
