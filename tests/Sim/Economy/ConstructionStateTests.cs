using GdUnit4;
using SettlersX.Sim.Economy;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Economy;

[TestSuite]
public class ConstructionStateTests
{
  [TestCase]
  public void Outstanding_StartsAtFullCost()
  {
    var s = new ConstructionState(new ResourceStack(ResourceKind.Plank, 5), "house");
    AssertThat(s.Outstanding).IsEqual(5);
  }

  [TestCase]
  public void Outstanding_DecreasesAsDelivered()
  {
    var s = new ConstructionState(new ResourceStack(ResourceKind.Plank, 5), "house");
    s.Delivered.Add(ResourceKind.Plank, 2);
    AssertThat(s.Outstanding).IsEqual(3);
  }

  [TestCase]
  public void IsComplete_TrueOnceCostMet()
  {
    var s = new ConstructionState(new ResourceStack(ResourceKind.Plank, 3), "house");
    s.Delivered.Add(ResourceKind.Plank, 3);
    AssertThat(s.IsComplete).IsTrue();
    AssertThat(s.Outstanding).IsEqual(0);
  }

  [TestCase]
  public void IsComplete_FalseBelowCost()
  {
    var s = new ConstructionState(new ResourceStack(ResourceKind.Plank, 3), "house");
    s.Delivered.Add(ResourceKind.Plank, 2);
    AssertThat(s.IsComplete).IsFalse();
  }
}
