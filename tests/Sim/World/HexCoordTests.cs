using GdUnit4;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class HexCoordTests
{
  [TestCase]
  public void Neighbors_ReturnsSixDistinctCoords()
  {
    var origin = new HexCoord(0, 0);
    var neighbors = origin.Neighbors();
    AssertThat(neighbors.Length).IsEqual(6);
    AssertThat(new System.Collections.Generic.HashSet<HexCoord>(neighbors).Count).IsEqual(6);
  }

  [TestCase]
  public void Neighbors_AllAtDistanceOne()
  {
    var origin = new HexCoord(3, -2);
    foreach (var n in origin.Neighbors())
    {
      AssertThat(origin.Distance(n)).IsEqual(1);
    }
  }

  [TestCase]
  public void Distance_IsSymmetric()
  {
    var a = new HexCoord(2, -3);
    var b = new HexCoord(-1, 4);
    AssertThat(a.Distance(b)).IsEqual(b.Distance(a));
  }

  [TestCase]
  public void Distance_ToSelf_IsZero()
  {
    var a = new HexCoord(7, 11);
    AssertThat(a.Distance(a)).IsEqual(0);
  }

  // Vector3 is a pure C# struct — no Godot runtime required.
  // GdUnit0501 is overly conservative for math-only Godot types.
#pragma warning disable GdUnit0501
  [TestCase]
  public void ToWorld_OriginAtZero()
  {
    var origin = new HexCoord(0, 0);
    var world = origin.ToWorld(1.0);
    AssertThat(world.X).IsEqual(0f);
    AssertThat(world.Z).IsEqual(0f);
  }

  [TestCase]
  public void FromWorld_RoundTrip_ReturnsSameCoord()
  {
    var size = 1.0;
    var samples = new[]
    {
      new HexCoord(0, 0),
      new HexCoord(1, 0),
      new HexCoord(0, 1),
      new HexCoord(3, -2),
      new HexCoord(-4, 5),
      new HexCoord(7, 7),
    };
    foreach (var coord in samples)
    {
      var world = coord.ToWorld(size);
      var back = HexCoord.FromWorld(world, size);
      AssertThat(back).IsEqual(coord);
    }
  }

  [TestCase]
  public void FromWorld_ScaledSize_RoundTrips()
  {
    var size = 2.5;
    var coord = new HexCoord(4, -3);
    var world = coord.ToWorld(size);
    var back = HexCoord.FromWorld(world, size);
    AssertThat(back).IsEqual(coord);
  }
#pragma warning restore GdUnit0501
}
