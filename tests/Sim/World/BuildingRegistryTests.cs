using GdUnit4;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class BuildingRegistryTests
{
  [TestCase]
  public void TryAdd_NewHex_Succeeds()
  {
    var reg = new BuildingRegistry();
    var ok = reg.TryAdd("storehouse", new HexCoord(2, 3), 0, out var b);
    AssertThat(ok).IsTrue();
    AssertThat(b).IsNotNull();
    AssertThat(b.DefId).IsEqual("storehouse");
    AssertThat(reg.Count).IsEqual(1);
  }

  [TestCase]
  public void TryAdd_OccupiedHex_Fails()
  {
    var reg = new BuildingRegistry();
    reg.TryAdd("storehouse", new HexCoord(2, 3), 0, out _);
    var ok = reg.TryAdd("storehouse", new HexCoord(2, 3), 0, out _);
    AssertThat(ok).IsFalse();
    AssertThat(reg.Count).IsEqual(1);
  }

  [TestCase]
  public void At_KnownHex_ReturnsBuilding()
  {
    var reg = new BuildingRegistry();
    reg.TryAdd("storehouse", new HexCoord(2, 3), 0, out _);
    AssertThat(reg.At(new HexCoord(2, 3))).IsNotNull();
  }

  [TestCase]
  public void At_UnknownHex_ReturnsNull()
  {
    var reg = new BuildingRegistry();
    AssertThat(reg.At(new HexCoord(99, 99))).IsNull();
  }

  [TestCase]
  public void TryAdd_AssignsAscendingIds()
  {
    var reg = new BuildingRegistry();
    reg.TryAdd("storehouse", new HexCoord(0, 0), 0, out var a);
    reg.TryAdd("storehouse", new HexCoord(1, 0), 0, out var b);
    AssertThat(b.Id > a.Id).IsTrue();
  }
}
