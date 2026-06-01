using System.Linq;
using GdUnit4;
using SettlersX.Sim.Pathfinding;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Pathfinding;

[TestSuite]
public class HexAStarTests
{
  [TestCase]
  public void Find_StartEqualsGoal_ReturnsSinglePoint()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    var path = HexAStar.Find(graph, new HexCoord(0, 0), new HexCoord(0, 0));
    AssertThat(path).IsNotNull();
    AssertThat(path!.Count).IsEqual(1);
  }

  [TestCase]
  public void Find_StraightLine_ReturnsExpectedPath()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    graph.TryAddEdge(new HexCoord(1, 0), new HexCoord(2, 0));
    graph.TryAddEdge(new HexCoord(2, 0), new HexCoord(3, 0));
    var path = HexAStar.Find(graph, new HexCoord(0, 0), new HexCoord(3, 0));
    AssertThat(path).IsNotNull();
    AssertThat(path!.Count).IsEqual(4);
    AssertThat(path[0]).IsEqual(new HexCoord(0, 0));
    AssertThat(path[3]).IsEqual(new HexCoord(3, 0));
  }

  [TestCase]
  public void Find_Disconnected_ReturnsNull()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    graph.TryAddEdge(new HexCoord(5, 5), new HexCoord(6, 5));
    var path = HexAStar.Find(graph, new HexCoord(0, 0), new HexCoord(5, 5));
    AssertThat(path).IsNull();
  }

  [TestCase]
  public void Find_GoalNotInGraph_ReturnsNull()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    var path = HexAStar.Find(graph, new HexCoord(0, 0), new HexCoord(99, 99));
    AssertThat(path).IsNull();
  }

  [TestCase]
  public void Find_RepeatedRuns_AreDeterministic()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    graph.TryAddEdge(new HexCoord(1, 0), new HexCoord(1, 1));
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(0, 1));
    graph.TryAddEdge(new HexCoord(0, 1), new HexCoord(1, 1));
    var p1 = HexAStar.Find(graph, new HexCoord(0, 0), new HexCoord(1, 1));
    var p2 = HexAStar.Find(graph, new HexCoord(0, 0), new HexCoord(1, 1));
    AssertThat(p1).IsNotNull();
    AssertThat(p2).IsNotNull();
    AssertThat(p1!.SequenceEqual(p2!)).IsTrue();
  }
}
