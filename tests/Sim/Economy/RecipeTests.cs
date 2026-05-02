using GdUnit4;
using SettlersX.Sim.Economy;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Economy;

[TestSuite]
public class RecipeTests
{
  [TestCase]
  public void Default_IsNotValid()
  {
    var r = default(Recipe);
    AssertThat(r.IsValid).IsFalse();
  }

  [TestCase]
  public void RawProducerRecipe_IsValid_WithoutInput()
  {
    var r = new Recipe(default, new ResourceStack(ResourceKind.Wood, 1), 30);
    AssertThat(r.IsValid).IsTrue();
    AssertThat(r.HasInput).IsFalse();
  }

  [TestCase]
  public void ConverterRecipe_HasInputAndOutput()
  {
    var r = new Recipe(
        new ResourceStack(ResourceKind.Wood, 1),
        new ResourceStack(ResourceKind.Plank, 1),
        20);
    AssertThat(r.IsValid).IsTrue();
    AssertThat(r.HasInput).IsTrue();
  }

  [TestCase]
  public void ZeroDuration_IsNotValid()
  {
    var r = new Recipe(default, new ResourceStack(ResourceKind.Wood, 1), 0);
    AssertThat(r.IsValid).IsFalse();
  }

  [TestCase]
  public void EmptyOutput_IsNotValid()
  {
    var r = new Recipe(default, default, 30);
    AssertThat(r.IsValid).IsFalse();
  }
}
