using SettlersX.Sim.Economy;

namespace SettlersX.Sim.World;

/// <summary>
/// Static description of a building type. Catalog entries instantiate this; the
/// registry uses it to decide which optional sub-states (Stock, Production,
/// Construction) to attach to a freshly placed <see cref="Building"/>.
/// </summary>
public sealed class BuildingSpec
{
  public string DefId { get; init; } = "";
  public BuildingKind Kind { get; init; }

  public bool HasStock { get; init; }

  public Recipe? Recipe { get; init; }
  public int InputCapacity { get; init; }
  public int OutputCapacity { get; init; }

  public ResourceStack ConstructionCost { get; init; }
  public string ConstructionFinishedDefId { get; init; } = "";
}
