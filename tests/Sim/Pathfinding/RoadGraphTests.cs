using System.Linq;
using GdUnit4;
using SettlersX.Sim.Pathfinding;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Pathfinding;

[TestSuite]
public class RoadGraphTests
{
  [TestCase]
  public void TryAddEdge_AdjacentHexes_AddsAndCounts()
  {
    var graph = new RoadGraph();
    var added = graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    AssertThat(added).IsTrue();
    AssertThat(graph.EdgeCount).IsEqual(1);
  }

  [TestCase]
  public void TryAddEdge_NonAdjacent_Rejected()
  {
    var graph = new RoadGraph();
    var added = graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(2, 0));
    AssertThat(added).IsFalse();
    AssertThat(graph.EdgeCount).IsEqual(0);
  }

  [TestCase]
  public void TryAddEdge_SameHex_Rejected()
  {
    var graph = new RoadGraph();
    AssertThat(graph.TryAddEdge(new HexCoord(3, 3), new HexCoord(3, 3))).IsFalse();
  }

  [TestCase]
  public void TryAddEdge_Duplicate_Rejected()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    AssertThat(graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0))).IsFalse();
    AssertThat(graph.EdgeCount).IsEqual(1);
  }

  [TestCase]
  public void HasEdge_IsSymmetric()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    AssertThat(graph.HasEdge(new HexCoord(0, 0), new HexCoord(1, 0))).IsTrue();
    AssertThat(graph.HasEdge(new HexCoord(1, 0), new HexCoord(0, 0))).IsTrue();
  }

  [TestCase]
  public void Neighbors_ReturnsConnectedHexes()
  {
    var graph = new RoadGraph();
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(1, 0));
    graph.TryAddEdge(new HexCoord(0, 0), new HexCoord(0, 1));
    var ns = graph.Neighbors(new HexCoord(0, 0)).ToList();
    AssertThat(ns.Count).IsEqual(2);
  }

  [TestCase]
  public void Neighbors_OfUnknownNode_IsEmpty()
  {
    var graph = new RoadGraph();
    AssertThat(graph.Neighbors(new HexCoord(99, 99)).Any()).IsFalse();
  }
}
