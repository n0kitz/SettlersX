using System.Collections.Generic;
using SettlersX.Sim.World.Intents;

namespace SettlersX.Sim.World;

/// <summary>
/// FIFO buffer of pending intents. Producers (View → GameManager.SubmitIntent)
/// enqueue; the tick loop drains once per tick at the Input phase.
/// </summary>
public class IntentQueue
{
  private readonly Queue<IIntent> _q = new();

  public int Count => _q.Count;

  public void Enqueue(IIntent intent) => _q.Enqueue(intent);

  /// <summary>
  /// Snapshot-and-drain. Safe against re-entrant enqueues during iteration —
  /// any intent enqueued while applying drained items waits until next tick.
  /// </summary>
  public IReadOnlyList<IIntent> Drain()
  {
    if (_q.Count == 0) { return System.Array.Empty<IIntent>(); }
    var batch = new List<IIntent>(_q.Count);
    while (_q.Count > 0) { batch.Add(_q.Dequeue()); }
    return batch;
  }
}
