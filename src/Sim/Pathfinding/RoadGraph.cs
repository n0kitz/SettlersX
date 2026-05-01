using System.Collections.Generic;
using System.Linq;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Pathfinding;

/// <summary>
/// Pure adjacency graph of road edges between hex coordinates. Edges are undirected
/// and only allowed between hex neighbors (distance 1). Sim-side authoritative source;
/// the View layer mirrors this into Godot.AStar3D for visualization (ADR 0005).
/// </summary>
public class RoadGraph
{
  private readonly Dictionary<HexCoord, HashSet<HexCoord>> _adj = new();
  private int _edgeCount;

  public int EdgeCount => _edgeCount;

  public bool HasNode(HexCoord c) => _adj.ContainsKey(c);

  public bool HasEdge(HexCoord a, HexCoord b) =>
      _adj.TryGetValue(a, out var s) && s.Contains(b);

  public IEnumerable<HexCoord> Neighbors(HexCoord c) =>
      _adj.TryGetValue(c, out var s) ? s : Enumerable.Empty<HexCoord>();

  /// <summary>
  /// Adds an undirected edge between two adjacent hexes. Returns false if the edge
  /// already exists, the endpoints are equal, or the endpoints are not hex neighbors.
  /// </summary>
  public bool TryAddEdge(HexCoord a, HexCoord b)
  {
    if (a.Equals(b)) { return false; }
    if (a.Distance(b) != 1) { return false; }
    if (!_adj.TryGetValue(a, out var sa)) { sa = new HashSet<HexCoord>(); _adj[a] = sa; }
    if (!_adj.TryGetValue(b, out var sb)) { sb = new HashSet<HexCoord>(); _adj[b] = sb; }
    if (!sa.Add(b)) { return false; }
    sb.Add(a);
    _edgeCount++;
    return true;
  }
}
