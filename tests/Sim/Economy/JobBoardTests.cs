using GdUnit4;
using SettlersX.Sim.Economy;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Economy;

[TestSuite]
public class JobBoardTests
{
  private static WorldState NewState() =>
      new WorldState(20, 20, BuildingCatalog.CreateDefault(), carriersPerStorehouse: 0);

  private static Building Place(WorldState state, string defId, HexCoord at)
  {
    state.Intents.Enqueue(new PlaceBuildingIntent(defId, at, 0));
    state.Tick();
    return state.Buildings.At(at)!;
  }

  [TestCase]
  public void TryFindJob_EmptyWorld_ReturnsFalse()
  {
    var state = NewState();
    AssertThat(JobBoard.TryFindJob(state, new HexCoord(0, 0), out _)).IsFalse();
  }

  [TestCase]
  public void TryFindJob_LumberCampWithOutput_ReturnsPushJobAndReservesStock()
  {
    var state = NewState();
    var camp = Place(state, BuildingCatalog.LumberCampId, new HexCoord(0, 0));
    var store = Place(state, BuildingCatalog.StorehouseId, new HexCoord(2, 0));
    camp.Production!.Output.Add(ResourceKind.Wood, 1);

    var ok = JobBoard.TryFindJob(state, new HexCoord(0, 0), out var job);
    AssertThat(ok).IsTrue();
    AssertThat(job.Kind).IsEqual(JobKind.PushToStock);
    AssertThat(job.Source.Id).IsEqual(camp.Id);
    AssertThat(job.Destination.Id).IsEqual(store.Id);
    AssertThat(camp.Production.Output.Get(ResourceKind.Wood)).IsEqual(0);
  }

  [TestCase]
  public void TryFindJob_ConstructionSiteAndStockedStorehouse_ReturnsPullToConstruction()
  {
    var state = NewState();
    var store = Place(state, BuildingCatalog.StorehouseId, new HexCoord(0, 0));
    store.Stock!.Add(ResourceKind.Plank, 5);
    var site = Place(state, BuildingCatalog.ConstructionSiteId, new HexCoord(2, 0));

    var ok = JobBoard.TryFindJob(state, new HexCoord(0, 0), out var job);
    AssertThat(ok).IsTrue();
    AssertThat(job.Kind).IsEqual(JobKind.PullToConstruction);
    AssertThat(job.Source.Id).IsEqual(store.Id);
    AssertThat(job.Destination.Id).IsEqual(site.Id);
    AssertThat(store.Stock.Get(ResourceKind.Plank)).IsEqual(4);
  }

  [TestCase]
  public void TryFindJob_SawmillNeedsWood_ReturnsPullToInput()
  {
    var state = NewState();
    var store = Place(state, BuildingCatalog.StorehouseId, new HexCoord(0, 0));
    store.Stock!.Add(ResourceKind.Wood, 5);
    var saw = Place(state, BuildingCatalog.SawmillId, new HexCoord(2, 0));

    var ok = JobBoard.TryFindJob(state, new HexCoord(0, 0), out var job);
    AssertThat(ok).IsTrue();
    AssertThat(job.Kind).IsEqual(JobKind.PullToInput);
    AssertThat(job.Destination.Id).IsEqual(saw.Id);
    AssertThat(store.Stock.Get(ResourceKind.Wood)).IsEqual(4);
  }

  [TestCase]
  public void Refund_PushJob_RestoresProducerOutput()
  {
    var state = NewState();
    var camp = Place(state, BuildingCatalog.LumberCampId, new HexCoord(0, 0));
    Place(state, BuildingCatalog.StorehouseId, new HexCoord(2, 0));
    camp.Production!.Output.Add(ResourceKind.Wood, 1);

    JobBoard.TryFindJob(state, new HexCoord(0, 0), out var job);
    AssertThat(camp.Production.Output.Get(ResourceKind.Wood)).IsEqual(0);
    JobBoard.Refund(job);
    AssertThat(camp.Production.Output.Get(ResourceKind.Wood)).IsEqual(1);
  }

  [TestCase]
  public void Refund_PullJob_RestoresStorehouseStock()
  {
    var state = NewState();
    var store = Place(state, BuildingCatalog.StorehouseId, new HexCoord(0, 0));
    store.Stock!.Add(ResourceKind.Plank, 1);
    Place(state, BuildingCatalog.ConstructionSiteId, new HexCoord(2, 0));

    JobBoard.TryFindJob(state, new HexCoord(0, 0), out var job);
    AssertThat(store.Stock.Get(ResourceKind.Plank)).IsEqual(0);
    JobBoard.Refund(job);
    AssertThat(store.Stock.Get(ResourceKind.Plank)).IsEqual(1);
  }
}
