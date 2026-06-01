using GdUnit4;
using SettlersX.Sim.Economy;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class BuildingRegistryTests
{
  private static BuildingRegistry NewRegistry() =>
      new BuildingRegistry(BuildingCatalog.CreateDefault());

  [TestCase]
  public void TryAdd_NewHex_Succeeds()
  {
    var reg = NewRegistry();
    var ok = reg.TryAdd(BuildingCatalog.StorehouseId, new HexCoord(2, 3), 0, out var b);
    AssertThat(ok).IsTrue();
    AssertThat(b).IsNotNull();
    AssertThat(b.DefId).IsEqual(BuildingCatalog.StorehouseId);
    AssertThat(reg.Count).IsEqual(1);
  }

  [TestCase]
  public void TryAdd_OccupiedHex_Fails()
  {
    var reg = NewRegistry();
    reg.TryAdd(BuildingCatalog.StorehouseId, new HexCoord(2, 3), 0, out _);
    var ok = reg.TryAdd(BuildingCatalog.StorehouseId, new HexCoord(2, 3), 0, out _);
    AssertThat(ok).IsFalse();
    AssertThat(reg.Count).IsEqual(1);
  }

  [TestCase]
  public void TryAdd_UnknownDefId_Fails()
  {
    var reg = NewRegistry();
    var ok = reg.TryAdd("not_in_catalog", new HexCoord(0, 0), 0, out _);
    AssertThat(ok).IsFalse();
    AssertThat(reg.Count).IsEqual(0);
  }

  [TestCase]
  public void TryAdd_Storehouse_AttachesStockNotProduction()
  {
    var reg = NewRegistry();
    reg.TryAdd(BuildingCatalog.StorehouseId, new HexCoord(2, 3), 0, out var b);
    AssertThat(b.Stock).IsNotNull();
    AssertThat(b.Production).IsNull();
    AssertThat(b.Construction).IsNull();
  }

  [TestCase]
  public void TryAdd_LumberCamp_AttachesProductionNotStock()
  {
    var reg = NewRegistry();
    reg.TryAdd(BuildingCatalog.LumberCampId, new HexCoord(2, 3), 0, out var b);
    AssertThat(b.Production).IsNotNull();
    AssertThat(b.Stock).IsNull();
    AssertThat(b.Construction).IsNull();
  }

  [TestCase]
  public void TryAdd_ConstructionSite_AttachesConstructionNotProduction()
  {
    var reg = NewRegistry();
    reg.TryAdd(BuildingCatalog.ConstructionSiteId, new HexCoord(2, 3), 0, out var b);
    AssertThat(b.Construction).IsNotNull();
    AssertThat(b.Production).IsNull();
    AssertThat(b.Stock).IsNull();
  }

  [TestCase]
  public void At_KnownHex_ReturnsBuilding()
  {
    var reg = NewRegistry();
    reg.TryAdd(BuildingCatalog.StorehouseId, new HexCoord(2, 3), 0, out _);
    AssertThat(reg.At(new HexCoord(2, 3))).IsNotNull();
  }

  [TestCase]
  public void At_UnknownHex_ReturnsNull()
  {
    var reg = NewRegistry();
    AssertThat(reg.At(new HexCoord(99, 99))).IsNull();
  }

  [TestCase]
  public void TryAdd_AssignsAscendingIds()
  {
    var reg = NewRegistry();
    reg.TryAdd(BuildingCatalog.StorehouseId, new HexCoord(0, 0), 0, out var a);
    reg.TryAdd(BuildingCatalog.StorehouseId, new HexCoord(1, 0), 0, out var b);
    AssertThat(b.Id > a.Id).IsTrue();
  }

  [TestCase]
  public void FinalizeConstruction_PromotesToFinishedDefId()
  {
    var reg = NewRegistry();
    reg.TryAdd(BuildingCatalog.ConstructionSiteId, new HexCoord(0, 0), 0, out var b);
    var cost = b.Construction!.Cost;
    b.Construction.Delivered.Add(cost.Kind, cost.Count);
    reg.FinalizeConstruction(new HexCoord(0, 0));
    AssertThat(b.DefId).IsEqual(BuildingCatalog.HouseId);
    AssertThat(b.Kind).IsEqual(BuildingKind.House);
    AssertThat(b.Construction).IsNull();
  }
}
