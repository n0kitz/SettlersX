using System.Collections.Generic;
using Godot;
using SettlersX.Core;
using SettlersX.Sim.World;

namespace SettlersX.Views.World;

/// <summary>
/// Renders road edges as a single ArrayMesh of line segments and mirrors the same
/// edges into a Godot.AStar3D instance for visualisation (criterion 5 of F1).
/// The Sim layer's RoadGraph is the authoritative source; this view is read-only.
/// </summary>
public partial class RoadView : Node3D
{
  private EventBus? _bus;
  private MeshInstance3D? _meshInstance;
  private AStar3D? _astar;
  private readonly Dictionary<Vector2I, long> _hexToId = new();
  private long _nextId;

  public double HexSize { get; set; } = 1.0;

  public override void _Ready()
  {
    _bus = EventBus.Instance;
    _bus.RoadBuilt += OnRoadBuilt;
    _astar = new AStar3D();
    _meshInstance = new MeshInstance3D { Name = "RoadMesh" };
    AddChild(_meshInstance);
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(_bus))
    {
      _bus!.RoadBuilt -= OnRoadBuilt;
    }
  }

  private void OnRoadBuilt(Vector2I a, Vector2I b)
  {
    var aid = GetOrAddPoint(a);
    var bid = GetOrAddPoint(b);
    if (!_astar!.ArePointsConnected(aid, bid))
    {
      _astar.ConnectPoints(aid, bid);
    }
    RebuildMesh();
  }

  private long GetOrAddPoint(Vector2I cell)
  {
    if (_hexToId.TryGetValue(cell, out var id)) { return id; }
    id = _nextId++;
    _hexToId[cell] = id;
    var hex = new HexCoord(cell.X, cell.Y);
    var world = hex.ToWorld(HexSize);
    _astar!.AddPoint(id, world);
    return id;
  }

  private void RebuildMesh()
  {
    var st = new SurfaceTool();
    st.Begin(Mesh.PrimitiveType.Lines);
    foreach (var (cell, id) in _hexToId)
    {
      foreach (var conn in _astar!.GetPointConnections(id))
      {
        if (conn <= id) { continue; }
        var a = _astar.GetPointPosition(id);
        var b = _astar.GetPointPosition(conn);
        a.Y = 0.05f;
        b.Y = 0.05f;
        st.AddVertex(a);
        st.AddVertex(b);
      }
    }
    var mat = new StandardMaterial3D
    {
      AlbedoColor = new Color(0.55f, 0.4f, 0.2f),
      ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
    };
    _meshInstance!.Mesh = st.Commit();
    _meshInstance.MaterialOverride = mat;
  }
}
