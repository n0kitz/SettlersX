using GdUnit4;
using SettlersX.Sim.Economy;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Economy;

[TestSuite]
public class CarrierTests
{
  [TestCase]
  public void NewCarrier_StartsIdle()
  {
    var c = new Carrier(1, new HexCoord(0, 0));
    AssertThat(c.State == CarrierState.Idle).IsTrue();
  }

  [TestCase]
  public void AssignPath_LengthGreaterThanOne_BecomesMoving()
  {
    var c = new Carrier(1, new HexCoord(0, 0));
    c.AssignPath(new[] { new HexCoord(0, 0), new HexCoord(1, 0) });
    AssertThat(c.State == CarrierState.Moving).IsTrue();
  }

  [TestCase]
  public void AssignPath_LengthOne_IsArrived()
  {
    var c = new Carrier(1, new HexCoord(0, 0));
    c.AssignPath(new[] { new HexCoord(0, 0) });
    AssertThat(c.State == CarrierState.Arrived).IsTrue();
  }

  [TestCase]
  public void Step_AdvancesOneHexPerCall()
  {
    var c = new Carrier(1, new HexCoord(0, 0));
    c.AssignPath(new[] { new HexCoord(0, 0), new HexCoord(1, 0), new HexCoord(2, 0) });
    c.Step();
    AssertThat(c.Hex).IsEqual(new HexCoord(1, 0));
    AssertThat(c.PrevHex).IsEqual(new HexCoord(0, 0));
    AssertThat(c.State == CarrierState.Moving).IsTrue();
  }

  [TestCase]
  public void Step_AtPathEnd_TransitionsToArrived()
  {
    var c = new Carrier(1, new HexCoord(0, 0));
    c.AssignPath(new[] { new HexCoord(0, 0), new HexCoord(1, 0) });
    c.Step();
    AssertThat(c.Hex).IsEqual(new HexCoord(1, 0));
    AssertThat(c.State == CarrierState.Arrived).IsTrue();
  }

  [TestCase]
  public void Step_AfterArrived_DoesNotMove()
  {
    var c = new Carrier(1, new HexCoord(0, 0));
    c.AssignPath(new[] { new HexCoord(0, 0), new HexCoord(1, 0) });
    c.Step();
    var hexAfterArrival = c.Hex;
    c.Step();
    AssertThat(c.Hex).IsEqual(hexAfterArrival);
  }

  [TestCase]
  public void Reset_ReturnsToIdle()
  {
    var c = new Carrier(1, new HexCoord(0, 0));
    c.AssignPath(new[] { new HexCoord(0, 0), new HexCoord(1, 0) });
    c.Reset();
    AssertThat(c.State == CarrierState.Idle).IsTrue();
    AssertThat(c.Path).IsNull();
  }
}
