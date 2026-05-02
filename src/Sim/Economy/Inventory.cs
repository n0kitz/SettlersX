using System.Collections.Generic;

namespace SettlersX.Sim.Economy;

/// <summary>
/// Mutable per-building resource counter. No internal capacity — callers (storehouse
/// stock, production input/output buffers) gate writes against their own caps.
/// Lookups and writes are O(1). Snapshot returns the live dictionary; do not mutate.
/// </summary>
public class Inventory
{
  private readonly Dictionary<ResourceKind, int> _counts = new();

  public int Get(ResourceKind kind) =>
      _counts.TryGetValue(kind, out var c) ? c : 0;

  public void Add(ResourceKind kind, int count)
  {
    if (kind == ResourceKind.None || count <= 0) { return; }
    _counts[kind] = Get(kind) + count;
  }

  public bool TryRemove(ResourceKind kind, int count)
  {
    if (kind == ResourceKind.None || count <= 0) { return false; }
    var current = Get(kind);
    if (current < count) { return false; }
    var next = current - count;
    if (next == 0) { _counts.Remove(kind); }
    else { _counts[kind] = next; }
    return true;
  }

  public int Total
  {
    get
    {
      var sum = 0;
      foreach (var v in _counts.Values) { sum += v; }
      return sum;
    }
  }

  public IReadOnlyDictionary<ResourceKind, int> Snapshot() => _counts;
}
