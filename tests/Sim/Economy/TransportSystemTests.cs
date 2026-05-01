using GdUnit4;
using SettlersX.Sim.Economy;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Economy;

[TestSuite]
public class TransportSystemTests
{
  private static WorldState BuildWithTwoConnectedStorehouses()
  {
    var state = new WorldState(20, 20);
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(2, 2), 0));
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(5, 2), 0));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(2, 2), new HexCoord(3, 2)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(3, 2), new HexCoord(4, 2)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(4, 2), new HexCoord(5, 2)));
    state.Tick();
    return state;
  }

  [TestCase]
  public void Tick_NoStorehouses_NoCarrierSpawned()
  {
    var state = new WorldState(20, 20);
    state.Tick();
    AssertThat(state.Carriers.Count).IsEqual(0);
  }

  [TestCase]
  public void Tick_TwoStorehousesConnected_SpawnsOneCarrier()
  {
    var state = BuildWithTwoConnectedStorehouses();
    state.Tick();
    AssertThat(state.Carriers.Count).IsEqual(1);
  }

  [TestCase]
  public void Tick_TwoStorehousesNotConnected_NoCarrierSpawned()
  {
    var state = new WorldState(20, 20);
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(2, 2), 0));
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(8, 8), 0));
    state.Tick();
    state.Tick();
    AssertThat(state.Carriers.Count).IsEqual(0);
  }

  [TestCase]
  public void Tick_OverMultipleTicks_CarrierMovesAlongPath()
  {
    var state = BuildWithTwoConnectedStorehouses();
    state.Tick();
    var startHex = state.Carriers[0].Hex;
    state.Tick();
    var afterTwoTicks = state.Carriers[0].Hex;
    AssertThat(afterTwoTicks.Equals(startHex)).IsFalse();
  }

  [TestCase]
  public void Tick_CarrierEventuallyReachesOtherStorehouse()
  {
    var state = BuildWithTwoConnectedStorehouses();
    state.Tick();
    var maxTicks = 50;
    var reached = false;
    for (var i = 0; i < maxTicks; i++)
    {
      state.Tick();
      if (state.Carriers[0].Hex.Equals(new HexCoord(5, 2)))
      {
        reached = true;
        break;
      }
    }
    AssertThat(reached).IsTrue();
  }
}
