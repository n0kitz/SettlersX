using System.Linq;
using GdUnit4;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class WorldStateTests
{
  [TestCase]
  public void Constructor_BuildsGridAndThreeSectors()
  {
    var state = new WorldState(20, 20);
    AssertThat(state.Grid.Width).IsEqual(20);
    AssertThat(state.Grid.Height).IsEqual(20);
    AssertThat(state.Sectors.Count).IsEqual(3);
  }

  [TestCase]
  public void Constructor_AllSectorsOwnedByPlayerZero()
  {
    var state = new WorldState(20, 20);
    foreach (var sector in state.Sectors.Sectors)
    {
      AssertThat(state.Sectors.GetOwner(sector)).IsEqual(0);
    }
  }

  [TestCase]
  public void Constructor_EveryHexBelongsToASector()
  {
    var state = new WorldState(20, 20);
    foreach (var hex in state.Grid.All())
    {
      AssertThat(state.Sectors.GetSectorOf(hex).HasValue).IsTrue();
    }
  }

  [TestCase]
  public void Tick_OnFreshState_DoesNotThrow()
  {
    var state = new WorldState(20, 20);
    state.Tick();
    state.Tick();
    AssertThat(true).IsTrue();
  }

  [TestCase]
  public void PlaceBuildingIntent_InBounds_AddsBuildingAndFiresEvent()
  {
    var state = new WorldState(20, 20);
    Building? captured = null;
    state.BuildingPlaced += b => captured = b;
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(2, 3), 0));
    state.Tick();
    AssertThat(state.Buildings.Count).IsEqual(1);
    AssertThat(captured).IsNotNull();
    AssertThat(captured!.DefId).IsEqual("storehouse");
  }

  [TestCase]
  public void PlaceBuildingIntent_OutOfBounds_Rejected()
  {
    var state = new WorldState(20, 20);
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(99, 99), 0));
    state.Tick();
    AssertThat(state.Buildings.Count).IsEqual(0);
  }

  [TestCase]
  public void PlaceBuildingIntent_OnOccupiedCell_Rejected()
  {
    var state = new WorldState(20, 20);
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(2, 3), 0));
    state.Tick();
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(2, 3), 0));
    state.Tick();
    AssertThat(state.Buildings.Count).IsEqual(1);
  }

  [TestCase]
  public void BuildRoadIntent_Adjacent_AddsEdgeAndFiresEvent()
  {
    var state = new WorldState(20, 20);
    var fired = 0;
    state.RoadBuilt += (_, _) => fired++;
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(0, 0), new HexCoord(1, 0)));
    state.Tick();
    AssertThat(state.Roads.EdgeCount).IsEqual(1);
    AssertThat(fired).IsEqual(1);
  }

  [TestCase]
  public void BuildRoadIntent_NonAdjacent_Rejected()
  {
    var state = new WorldState(20, 20);
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(0, 0), new HexCoord(5, 5)));
    state.Tick();
    AssertThat(state.Roads.EdgeCount).IsEqual(0);
  }

  [TestCase]
  public void BuildRoadIntent_OutOfBounds_Rejected()
  {
    var state = new WorldState(20, 20);
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(0, 0), new HexCoord(-1, 0)));
    state.Tick();
    AssertThat(state.Roads.EdgeCount).IsEqual(0);
  }

  [TestCase]
  public void SnapshotCarriers_NoCarriers_ReturnsEmpty()
  {
    var state = new WorldState(20, 20);
    var snap = state.SnapshotCarriers(1.0);
    AssertThat(snap.Count).IsEqual(0);
  }

  [TestCase]
  public void SnapshotCarriers_AfterMovement_PrevAndNextWorldAreDistinct()
  {
    var state = new WorldState(20, 20);
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(2, 2), 0));
    state.Intents.Enqueue(new PlaceBuildingIntent("storehouse", new HexCoord(5, 2), 0));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(2, 2), new HexCoord(3, 2)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(3, 2), new HexCoord(4, 2)));
    state.Intents.Enqueue(new BuildRoadIntent(new HexCoord(4, 2), new HexCoord(5, 2)));
    state.Tick();
    state.Tick();
    state.Tick();

    var snap = state.SnapshotCarriers(1.0);
    AssertThat(snap.Count).IsEqual(1);
    var s = snap[0];
    var dx = s.PrevWorld.X - s.NextWorld.X;
    var dz = s.PrevWorld.Z - s.NextWorld.Z;
    var distSq = dx * dx + dz * dz;
    AssertThat(distSq > 0f).IsTrue();
    AssertThat(distSq < 4f).IsTrue();
  }
}
