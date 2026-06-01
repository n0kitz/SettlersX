using GdUnit4;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class IntentQueueTests
{
  [TestCase]
  public void Enqueue_IncreasesCount()
  {
    var q = new IntentQueue();
    q.Enqueue(new BuildRoadIntent(new HexCoord(0, 0), new HexCoord(1, 0)));
    AssertThat(q.Count).IsEqual(1);
  }

  [TestCase]
  public void Drain_EmptiesQueueAndPreservesFifoOrder()
  {
    var q = new IntentQueue();
    var a = new BuildRoadIntent(new HexCoord(0, 0), new HexCoord(1, 0));
    var b = new BuildRoadIntent(new HexCoord(1, 0), new HexCoord(2, 0));
    q.Enqueue(a);
    q.Enqueue(b);
    var drained = q.Drain();
    AssertThat(drained.Count).IsEqual(2);
    AssertThat(drained[0]).IsEqual(a);
    AssertThat(drained[1]).IsEqual(b);
    AssertThat(q.Count).IsEqual(0);
  }

  [TestCase]
  public void Drain_OnEmpty_ReturnsEmpty()
  {
    var q = new IntentQueue();
    var drained = q.Drain();
    AssertThat(drained.Count).IsEqual(0);
  }
}
