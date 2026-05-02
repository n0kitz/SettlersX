using GdUnit4;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class BuildingCatalogTests
{
  [TestCase]
  public void CreateDefault_ContainsAllChainEntries()
  {
    var c = BuildingCatalog.CreateDefault();
    AssertThat(c.Get(BuildingCatalog.StorehouseId)).IsNotNull();
    AssertThat(c.Get(BuildingCatalog.LumberCampId)).IsNotNull();
    AssertThat(c.Get(BuildingCatalog.SawmillId)).IsNotNull();
    AssertThat(c.Get(BuildingCatalog.ConstructionSiteId)).IsNotNull();
    AssertThat(c.Get(BuildingCatalog.HouseId)).IsNotNull();
  }

  [TestCase]
  public void CreateDefault_StorehouseHasStockButNoRecipe()
  {
    var s = BuildingCatalog.CreateDefault().Get(BuildingCatalog.StorehouseId)!;
    AssertThat(s.HasStock).IsTrue();
    AssertThat(s.Recipe.HasValue).IsFalse();
  }

  [TestCase]
  public void CreateDefault_LumberCampHasRecipeNoInput()
  {
    var c = BuildingCatalog.CreateDefault().Get(BuildingCatalog.LumberCampId)!;
    AssertThat(c.Recipe!.Value.HasInput).IsFalse();
    AssertThat(c.Recipe.Value.IsValid).IsTrue();
  }

  [TestCase]
  public void CreateDefault_SawmillHasWoodInPlankOut()
  {
    var c = BuildingCatalog.CreateDefault().Get(BuildingCatalog.SawmillId)!;
    AssertThat(c.Recipe!.Value.HasInput).IsTrue();
  }

  [TestCase]
  public void CreateDefault_ConstructionSitePromotesToHouse()
  {
    var c = BuildingCatalog.CreateDefault().Get(BuildingCatalog.ConstructionSiteId)!;
    AssertThat(c.ConstructionFinishedDefId).IsEqual(BuildingCatalog.HouseId);
  }

  [TestCase]
  public void Get_UnknownDefId_ReturnsNull()
  {
    var c = BuildingCatalog.CreateDefault();
    AssertThat(c.Get("not_a_real_thing")).IsNull();
  }
}
