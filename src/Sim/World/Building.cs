using SettlersX.Sim.Economy;
using SettlersX.Sim.Production;

namespace SettlersX.Sim.World;

/// <summary>
/// A placed building. Holds optional sub-states keyed off its <see cref="Kind"/>:
/// Storehouses carry a <c>Stock</c> inventory; producers carry a
/// <c>Production</c> buffer + timer; construction sites carry a
/// <c>Construction</c> tally that promotes them to a finished kind on completion.
/// </summary>
public sealed class Building
{
  public int Id { get; }
  public string DefId { get; private set; }
  public BuildingKind Kind { get; private set; }
  public HexCoord Origin { get; }
  public int OwnerPlayerId { get; }

  public Inventory? Stock { get; }
  public ProductionState? Production { get; }
  public ConstructionState? Construction { get; private set; }

  internal Building(int id, BuildingSpec spec, HexCoord origin, int ownerPlayerId)
  {
    Id = id;
    DefId = spec.DefId;
    Kind = spec.Kind;
    Origin = origin;
    OwnerPlayerId = ownerPlayerId;

    if (spec.HasStock) { Stock = new Inventory(); }
    if (spec.Recipe.HasValue && spec.Recipe.Value.IsValid)
    {
      Production = new ProductionState(
          spec.Recipe.Value, spec.InputCapacity, spec.OutputCapacity);
    }
    if (!spec.ConstructionCost.IsEmpty)
    {
      Construction = new ConstructionState(
          spec.ConstructionCost, spec.ConstructionFinishedDefId);
    }
  }

  /// <summary>
  /// Promote this construction site to its finished form. ProductionSystem invokes
  /// this once <see cref="ConstructionState.IsComplete"/>; the catalog supplies the
  /// finished spec so DefId/Kind flip atomically and the Construction sub-state is
  /// dropped.
  /// </summary>
  internal void FinalizeConstruction(BuildingSpec finishedSpec)
  {
    DefId = finishedSpec.DefId;
    Kind = finishedSpec.Kind;
    Construction = null;
  }
}
