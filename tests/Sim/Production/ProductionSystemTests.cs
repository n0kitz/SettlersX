using GdUnit4;
using SettlersX.Sim.Economy;
using SettlersX.Sim.Production;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Production;

[TestSuite]
public class ProductionSystemTests
{
  private static WorldState NewState() =>
      new WorldState(20, 20, BuildingCatalog.CreateDefault(), carriersPerStorehouse: 0);

  [TestCase]
  public void Tick_LumberCamp_AccumulatesOutputOverTime()
  {
    var state = NewState();
    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.LumberCampId, new HexCoord(0, 0), 0));
    state.Tick();
    var camp = state.Buildings.At(new HexCoord(0, 0))!;

    for (var i = 0; i < 60; i++) { state.Tick(); }
    AssertThat(camp.Production!.Output.Get(ResourceKind.Wood) > 0).IsTrue();
  }

  [TestCase]
  public void Tick_ConstructionSiteFullyDelivered_PromotesToHouse()
  {
    var state = NewState();
    state.Intents.Enqueue(new PlaceBuildingIntent(
        BuildingCatalog.ConstructionSiteId, new HexCoord(0, 0), 0));
    state.Tick();
    var site = state.Buildings.At(new HexCoord(0, 0))!;
    var cost = site.Construction!.Cost;
    site.Construction.Delivered.Add(cost.Kind, cost.Count);

    var raised = false;
    state.ConstructionCompleted += _ => raised = true;
    state.Tick();
    AssertThat(site.DefId).IsEqual(BuildingCatalog.HouseId);
    AssertThat(site.Kind).IsEqual(BuildingKind.House);
    AssertThat(raised).IsTrue();
  }
}
