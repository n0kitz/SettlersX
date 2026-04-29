using System;
using GdUnit4;
using SettlersX.Sim.Core;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.Core;

[TestSuite]
public class TickEngineTests
{
  [TestCase]
  public void Advance_WithSufficientDelta_FiresOneTick()
  {
    var engine = new TickEngine(tickHz: 10.0); // interval = 0.1s
    var fired = engine.Advance(0.1);
    AssertThat(fired).IsEqual(1);
    AssertThat(engine.TickNumber).IsEqual(1);
  }

  [TestCase]
  public void Advance_WithDoubleDelta_FiresTwoTicks()
  {
    var engine = new TickEngine(tickHz: 10.0);
    var fired = engine.Advance(0.2);
    AssertThat(fired).IsEqual(2);
    AssertThat(engine.TickNumber).IsEqual(2);
  }

  [TestCase]
  public void Advance_WhenPaused_FiresNoTicks()
  {
    var engine = new TickEngine(tickHz: 10.0);
    engine.Paused = true;
    var fired = engine.Advance(1.0);
    AssertThat(fired).IsEqual(0);
    AssertThat(engine.TickNumber).IsEqual(0);
  }

  [TestCase]
  public void Advance_AfterUnpause_ResumesFromLastCount()
  {
    var engine = new TickEngine(tickHz: 10.0);
    engine.Advance(0.1); // tick 1
    engine.Paused = true;
    engine.Advance(1.0); // no ticks while paused
    engine.Paused = false;
    engine.Advance(0.1); // tick 2
    AssertThat(engine.TickNumber).IsEqual(2);
  }

  [TestCase]
  public void Alpha_BetweenTicks_IsInZeroToOneRange()
  {
    var engine = new TickEngine(tickHz: 10.0);
    engine.Advance(0.05); // halfway to next tick
    AssertThat(engine.Alpha).IsGreaterEqual(0.0);
    AssertThat(engine.Alpha).IsLessEqual(1.0);
  }

  [TestCase]
  public void StepOnce_WhenPaused_IncrementsTickNumber()
  {
    var engine = new TickEngine(tickHz: 10.0);
    engine.Paused = true;
    engine.StepOnce();
    AssertThat(engine.TickNumber).IsEqual(1);
  }

  [TestCase]
  public void Constructor_ZeroHz_Throws()
  {
    AssertThrown(() => new TickEngine(tickHz: 0.0))
        .IsInstanceOf<ArgumentOutOfRangeException>();
  }

  [TestCase]
  public void Constructor_NegativeHz_Throws()
  {
    AssertThrown(() => new TickEngine(tickHz: -5.0))
        .IsInstanceOf<ArgumentOutOfRangeException>();
  }

  [TestCase]
  public void Advance_NegativeDelta_ReturnsZero()
  {
    var engine = new TickEngine(tickHz: 10.0);
    var fired = engine.Advance(-1.0);
    AssertThat(fired).IsEqual(0);
    AssertThat(engine.TickNumber).IsEqual(0);
  }

  [TestCase]
  public void Advance_HugeDelta_CapsAtMaxTicksPerFrame()
  {
    var engine = new TickEngine(tickHz: 10.0);
    var fired = engine.Advance(999.0); // would be 9990 ticks without cap
    AssertThat(fired).IsEqual(TickEngine.MaxTicksPerFrame);
  }
}
