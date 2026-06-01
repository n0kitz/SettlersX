using GdUnit4;
using SettlersX.Sim.Economy;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Economy;

[TestSuite]
public class InventoryTests
{
  [TestCase]
  public void Get_UnknownKind_ReturnsZero()
  {
    var inv = new Inventory();
    AssertThat(inv.Get(ResourceKind.Wood)).IsEqual(0);
  }

  [TestCase]
  public void Add_AccumulatesCount()
  {
    var inv = new Inventory();
    inv.Add(ResourceKind.Wood, 2);
    inv.Add(ResourceKind.Wood, 3);
    AssertThat(inv.Get(ResourceKind.Wood)).IsEqual(5);
  }

  [TestCase]
  public void Add_NoneKind_NoOp()
  {
    var inv = new Inventory();
    inv.Add(ResourceKind.None, 5);
    AssertThat(inv.Total).IsEqual(0);
  }

  [TestCase]
  public void Add_NegativeCount_NoOp()
  {
    var inv = new Inventory();
    inv.Add(ResourceKind.Wood, -3);
    AssertThat(inv.Total).IsEqual(0);
  }

  [TestCase]
  public void TryRemove_SufficientStock_ReturnsTrueAndDecrements()
  {
    var inv = new Inventory();
    inv.Add(ResourceKind.Wood, 5);
    var ok = inv.TryRemove(ResourceKind.Wood, 2);
    AssertThat(ok).IsTrue();
    AssertThat(inv.Get(ResourceKind.Wood)).IsEqual(3);
  }

  [TestCase]
  public void TryRemove_InsufficientStock_ReturnsFalseAndUnchanged()
  {
    var inv = new Inventory();
    inv.Add(ResourceKind.Wood, 1);
    var ok = inv.TryRemove(ResourceKind.Wood, 5);
    AssertThat(ok).IsFalse();
    AssertThat(inv.Get(ResourceKind.Wood)).IsEqual(1);
  }

  [TestCase]
  public void TryRemove_DownToZero_RemovesEntryFromSnapshot()
  {
    var inv = new Inventory();
    inv.Add(ResourceKind.Wood, 1);
    inv.TryRemove(ResourceKind.Wood, 1);
    AssertThat(inv.Snapshot().ContainsKey(ResourceKind.Wood)).IsFalse();
  }

  [TestCase]
  public void Total_SumsEveryKind()
  {
    var inv = new Inventory();
    inv.Add(ResourceKind.Wood, 2);
    inv.Add(ResourceKind.Plank, 3);
    AssertThat(inv.Total).IsEqual(5);
  }
}
