using GdUnit4;
using SettlersX.Sim.Economy;
using SettlersX.Sim.Production;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Production;

[TestSuite]
public class ProductionStateTests
{
  private static ProductionState NewLumber() => new(
      new Recipe(default, new ResourceStack(ResourceKind.Wood, 1), 3),
      inputCapacity: 0, outputCapacity: 4);

  private static ProductionState NewSaw() => new(
      new Recipe(
          new ResourceStack(ResourceKind.Wood, 1),
          new ResourceStack(ResourceKind.Plank, 1), 2),
      inputCapacity: 4, outputCapacity: 4);

  [TestCase]
  public void Lumber_TickStartsTimerAndProducesAtZero()
  {
    var p = NewLumber();
    p.Tick(); // starts timer at 3
    AssertThat(p.Working).IsTrue();
    p.Tick(); // 2
    p.Tick(); // 1
    p.Tick(); // 0 → produces
    AssertThat(p.Output.Get(ResourceKind.Wood)).IsEqual(1);
    AssertThat(p.Working).IsFalse();
  }

  [TestCase]
  public void Saw_StarvedUntilInputDelivered()
  {
    var p = NewSaw();
    AssertThat(p.IsStarved).IsTrue();
    p.Tick();
    AssertThat(p.Working).IsFalse();
    AssertThat(p.IsStarved).IsTrue();
    p.Input.Add(ResourceKind.Wood, 1);
    AssertThat(p.IsStarved).IsFalse();
  }

  [TestCase]
  public void Saw_ConsumesInputOnStart()
  {
    var p = NewSaw();
    p.Input.Add(ResourceKind.Wood, 1);
    p.Tick();
    AssertThat(p.Input.Get(ResourceKind.Wood)).IsEqual(0);
    AssertThat(p.Working).IsTrue();
  }

  [TestCase]
  public void Lumber_BlockedWhenOutputAtCapacity()
  {
    var p = NewLumber();
    p.Output.Add(ResourceKind.Wood, 4);
    AssertThat(p.IsBlocked).IsTrue();
    p.Tick();
    AssertThat(p.Working).IsFalse();
  }

  [TestCase]
  public void InputCanAccept_RespectsCapacity()
  {
    var p = NewSaw();
    p.Input.Add(ResourceKind.Wood, 3);
    AssertThat(p.InputCanAccept(ResourceKind.Wood, 1)).IsTrue();
    p.Input.Add(ResourceKind.Wood, 1);
    AssertThat(p.InputCanAccept(ResourceKind.Wood, 1)).IsFalse();
  }
}
