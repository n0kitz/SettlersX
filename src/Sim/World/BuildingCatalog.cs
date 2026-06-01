using System.Collections.Generic;
using SettlersX.Sim.Economy;

namespace SettlersX.Sim.World;

/// <summary>
/// Lookup table from DefId → <see cref="BuildingSpec"/>. WorldSim builds the runtime
/// catalog from balance.json on startup; tests construct an explicit one. Sim never
/// reads .tres files directly — the catalog is the only injection point.
/// </summary>
public class BuildingCatalog
{
  public const string StorehouseId = "storehouse";
  public const string LumberCampId = "lumber_camp";
  public const string SawmillId = "sawmill";
  public const string ConstructionSiteId = "construction_site";
  public const string HouseId = "house";

  private readonly Dictionary<string, BuildingSpec> _byId;

  public BuildingCatalog(IEnumerable<BuildingSpec> specs)
  {
    _byId = new Dictionary<string, BuildingSpec>();
    foreach (var spec in specs)
    {
      _byId[spec.DefId] = spec;
    }
  }

  public BuildingSpec? Get(string defId) =>
      _byId.TryGetValue(defId, out var s) ? s : null;

  public IReadOnlyCollection<BuildingSpec> All => _byId.Values;

  /// <summary>
  /// Default F2a catalog with one full chain. Numeric balance lives in
  /// <c>data/balance.json</c>; this factory accepts those values rather than baking
  /// them in. Tests use the parameterless overload for predictable defaults.
  /// </summary>
  public static BuildingCatalog CreateDefault(
      int lumberDuration = 30,
      int sawmillDuration = 20,
      int bufferCapacity = 4,
      int storehouseStockCap = 999,
      int houseCostPlanks = 5)
  {
    var specs = new List<BuildingSpec>
    {
      new()
      {
        DefId = StorehouseId,
        Kind = BuildingKind.Storehouse,
        HasStock = true,
      },
      new()
      {
        DefId = LumberCampId,
        Kind = BuildingKind.LumberCamp,
        Recipe = new Recipe(
            Input: default,
            Output: new ResourceStack(ResourceKind.Wood, 1),
            DurationTicks: lumberDuration),
        InputCapacity = 0,
        OutputCapacity = bufferCapacity,
      },
      new()
      {
        DefId = SawmillId,
        Kind = BuildingKind.Sawmill,
        Recipe = new Recipe(
            Input: new ResourceStack(ResourceKind.Wood, 1),
            Output: new ResourceStack(ResourceKind.Plank, 1),
            DurationTicks: sawmillDuration),
        InputCapacity = bufferCapacity,
        OutputCapacity = bufferCapacity,
      },
      new()
      {
        DefId = ConstructionSiteId,
        Kind = BuildingKind.ConstructionSite,
        ConstructionCost = new ResourceStack(ResourceKind.Plank, houseCostPlanks),
        ConstructionFinishedDefId = HouseId,
      },
      new()
      {
        DefId = HouseId,
        Kind = BuildingKind.House,
      },
    };
    _ = storehouseStockCap; // reserved for future capacity wiring
    return new BuildingCatalog(specs);
  }
}
