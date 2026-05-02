using GdUnit4;
using SettlersX.Sim.Economy;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Economy;

[TestSuite]
public class TransportSystemTests
{
  private static WorldState NewState(int carriersPerStorehouse = 3) =>
      new WorldState(20, 20, BuildingCatalog.CreateDefault(), carriersPerStorehouse);

  [TestCase]
  public void Tick_NoStorehouses_NoCarrierSpawned()
  {
    var state = NewState();
    state.Tick();
    AssertThat(state.Carriers.Count).IsEqual(0);
  }

  [TestCase]
  public void Tick_StorehousePlaced_SpawnsConfiguredCarrierCount()
  {
    var state = NewState(carriersPerStorehouse: 3);
    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.StorehouseId, new HexCoord(2, 2), 0));
    state.Tick();
    AssertThat(state.Carriers.Count).IsEqual(3);
    foreach (var c in state.Carriers)
    {
      AssertThat(c.HomeBuildingId).IsEqual(state.Buildings.At(new HexCoord(2, 2))!.Id);
    }
  }

  [TestCase]
  public void Tick_ConfigurableCarrierCountIsRespected()
  {
    var state = NewState(carriersPerStorehouse: 1);
    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.StorehouseId, new HexCoord(2, 2), 0));
    state.Tick();
    AssertThat(state.Carriers.Count).IsEqual(1);
  }

  [TestCase]
  public void Tick_LumberCampOnRoad_EventuallyDeliversWoodToStock()
  {
    var state = BuildLumberChain(houseCostPlanks: 0);
    var store = state.Buildings.At(new HexCoord(0, 0))!;
    var camp = state.Buildings.At(new HexCoord(2, 0))!;

    var maxTicks = 400;
    var delivered = false;
    for (var i = 0; i < maxTicks; i++)
    {
      state.Tick();
      if (store.Stock!.Get(ResourceKind.Wood) > 0)
      {
        delivered = true;
        break;
      }
    }
    AssertThat(delivered).IsTrue();
    AssertThat(camp.Production!.Output.Get(ResourceKind.Wood) >= 0).IsTrue();
  }

  [TestCase]
  public void Tick_FullChain_HouseEventuallyConstructs()
  {
    var state = BuildLumberChain(houseCostPlanks: 1);
    var siteHex = new HexCoord(6, 0);
    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.ConstructionSiteId, siteHex, 0));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(5, 0), siteHex));
    state.Tick();

    var maxTicks = 800;
    var built = false;
    for (var i = 0; i < maxTicks; i++)
    {
      state.Tick();
      var site = state.Buildings.At(siteHex);
      if (site != null && site.Kind == BuildingKind.House)
      {
        built = true;
        break;
      }
    }
    AssertThat(built).IsTrue();
  }

  /// <summary>
  /// Storehouse at (0,0), LumberCamp at (2,0), Sawmill at (4,0). Roads connect all
  /// adjacent pairs. Test caller may then place a ConstructionSite further along.
  /// </summary>
  private static WorldState BuildLumberChain(int houseCostPlanks)
  {
    var catalog = BuildingCatalog.CreateDefault(
        lumberDuration: 3,
        sawmillDuration: 2,
        bufferCapacity: 4,
        houseCostPlanks: houseCostPlanks);
    var state = new WorldState(20, 20, catalog, carriersPerStorehouse: 3);

    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.StorehouseId, new HexCoord(0, 0), 0));
    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.LumberCampId, new HexCoord(2, 0), 0));
    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.SawmillId, new HexCoord(4, 0), 0));

    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(0, 0), new HexCoord(1, 0)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(1, 0), new HexCoord(2, 0)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(2, 0), new HexCoord(3, 0)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(3, 0), new HexCoord(4, 0)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(4, 0), new HexCoord(5, 0)));
    state.Tick();
    return state;
  }
}
