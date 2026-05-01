using System;
using System.Linq;
using GdUnit4;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class HexGridTests
{
  [TestCase]
  public void Constructor_ZeroWidth_Throws()
  {
    AssertThrown(() => new HexGrid(0, 5))
        .IsInstanceOf<ArgumentOutOfRangeException>();
  }

  [TestCase]
  public void Constructor_NegativeHeight_Throws()
  {
    AssertThrown(() => new HexGrid(5, -1))
        .IsInstanceOf<ArgumentOutOfRangeException>();
  }

  [TestCase]
  public void InBounds_InteriorCoord_IsTrue()
  {
    var grid = new HexGrid(10, 10);
    AssertThat(grid.InBounds(new HexCoord(5, 5))).IsTrue();
  }

  [TestCase]
  public void InBounds_NegativeCoord_IsFalse()
  {
    var grid = new HexGrid(10, 10);
    AssertThat(grid.InBounds(new HexCoord(-1, 5))).IsFalse();
  }

  [TestCase]
  public void InBounds_AtUpperEdge_IsFalse()
  {
    var grid = new HexGrid(10, 10);
    AssertThat(grid.InBounds(new HexCoord(10, 5))).IsFalse();
    AssertThat(grid.InBounds(new HexCoord(5, 10))).IsFalse();
  }

  [TestCase]
  public void All_EnumeratesEveryHex()
  {
    var grid = new HexGrid(3, 4);
    var hexes = grid.All().ToList();
    AssertThat(hexes.Count).IsEqual(12);
    AssertThat(new System.Collections.Generic.HashSet<HexCoord>(hexes).Count).IsEqual(12);
  }
}
