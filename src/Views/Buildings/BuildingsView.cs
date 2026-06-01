using System.Collections.Generic;
using Godot;
using SettlersX.Core;
using SettlersX.Sim.World;

namespace SettlersX.Views.Buildings;

/// <summary>
/// Spawns a placeholder mesh per BuildingConstructed signal. F2a colours the box by
/// DefId so the chain (storehouse / lumber camp / sawmill / construction site /
/// house) is distinguishable at a glance. Per-def visual meshes load through
/// ResourceDatabase in F2b once .tres assets exist.
/// </summary>
public partial class BuildingsView : Node3D
{
  private EventBus? _bus;
  private readonly Dictionary<Vector2I, MeshInstance3D> _byCell = new();

  public double HexSize { get; set; } = 1.0;

  private static readonly Dictionary<string, Color> _palette = new()
  {
    [BuildingCatalog.StorehouseId] = new Color(0.78f, 0.55f, 0.32f),
    [BuildingCatalog.LumberCampId] = new Color(0.40f, 0.70f, 0.30f),
    [BuildingCatalog.SawmillId] = new Color(0.65f, 0.45f, 0.20f),
    [BuildingCatalog.ConstructionSiteId] = new Color(0.85f, 0.80f, 0.30f),
    [BuildingCatalog.HouseId] = new Color(0.90f, 0.30f, 0.30f),
  };

  public override void _Ready()
  {
    _bus = EventBus.Instance;
    _bus.BuildingConstructed += OnBuildingConstructed;
    _bus.BuildingFinalized += OnBuildingConstructed;
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(_bus))
    {
      _bus!.BuildingConstructed -= OnBuildingConstructed;
      _bus.BuildingFinalized -= OnBuildingConstructed;
    }
  }

  private void OnBuildingConstructed(string defId, Vector2I cell)
  {
    var hex = new HexCoord(cell.X, cell.Y);
    var origin = hex.ToWorld(HexSize);
    var size = (float)(HexSize * 0.9);

    if (_byCell.TryGetValue(cell, out var existing) && IsInstanceValid(existing))
    {
      existing.QueueFree();
      _byCell.Remove(cell);
    }

    var mesh = new BoxMesh { Size = new Vector3(size, size, size) };
    var color = _palette.GetValueOrDefault(defId, new Color(0.5f, 0.5f, 0.5f));
    var mat = new StandardMaterial3D
    {
      AlbedoColor = color,
      ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
    };
    var node = new MeshInstance3D
    {
      Name = $"Building_{cell.X}_{cell.Y}",
      Mesh = mesh,
      MaterialOverride = mat,
      Position = origin + new Vector3(0f, size * 0.5f, 0f),
    };
    AddChild(node);
    _byCell[cell] = node;
  }
}
