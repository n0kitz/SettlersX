using System.Collections.Generic;
using System.Linq;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Pathfinding;

/// <summary>
/// Deterministic A* over a <see cref="RoadGraph"/>. Tie-break on (Q, R) so the same
/// inputs always produce the same path, satisfying the determinism rule (ADR 0002,
/// reinforced by S4 in the roadmap).
/// </summary>
public static class HexAStar
{
  public static IReadOnlyList<HexCoord>? Find(RoadGraph graph, HexCoord start, HexCoord goal)
  {
    if (start.Equals(goal)) { return new[] { start }; }
    if (!graph.HasNode(start) || !graph.HasNode(goal)) { return null; }

    var open = new PriorityQueue<HexCoord, (int F, int Seq)>();
    var cameFrom = new Dictionary<HexCoord, HexCoord>();
    var gScore = new Dictionary<HexCoord, int> { [start] = 0 };
    var closed = new HashSet<HexCoord>();
    var seq = 0;
    open.Enqueue(start, (start.Distance(goal), seq++));

    while (open.TryDequeue(out var current, out _))
    {
      if (!closed.Add(current)) { continue; }
      if (current.Equals(goal)) { return Reconstruct(cameFrom, current); }

      foreach (var n in graph.Neighbors(current).OrderBy(c => c.Q).ThenBy(c => c.R))
      {
        if (closed.Contains(n)) { continue; }
        var tentative = gScore[current] + 1;
        if (!gScore.TryGetValue(n, out var existing) || tentative < existing)
        {
          gScore[n] = tentative;
          cameFrom[n] = current;
          open.Enqueue(n, (tentative + n.Distance(goal), seq++));
        }
      }
    }
    return null;
  }

  private static List<HexCoord> Reconstruct(
      Dictionary<HexCoord, HexCoord> cameFrom, HexCoord end)
  {
    var path = new List<HexCoord> { end };
    var current = end;
    while (cameFrom.TryGetValue(current, out var prev))
    {
      current = prev;
      path.Add(current);
    }
    path.Reverse();
    return path;
  }
}
