using Godot;
using SettlersX.Core;
using SettlersX.Sim.World;

namespace SettlersX.Views.Buildings;

/// <summary>
/// Spawns a placeholder mesh per BuildingConstructed signal. Phase 1 uses a single
/// box mesh per building irrespective of DefId; F2a swaps these for per-def visuals
/// loaded through ResourceDatabase.
/// </summary>
public partial class BuildingsView : Node3D
{
  private EventBus? _bus;

  public double HexSize { get; set; } = 1.0;

  public override void _Ready()
  {
    _bus = EventBus.Instance;
    _bus.BuildingConstructed += OnBuildingConstructed;
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(_bus))
    {
      _bus!.BuildingConstructed -= OnBuildingConstructed;
    }
  }

  private void OnBuildingConstructed(string defId, Vector2I cell)
  {
    var hex = new HexCoord(cell.X, cell.Y);
    var origin = hex.ToWorld(HexSize);
    var size = (float)(HexSize * 0.9);
    var mesh = new BoxMesh { Size = new Vector3(size, size, size) };
    var mat = new StandardMaterial3D
    {
      AlbedoColor = new Color(0.78f, 0.55f, 0.32f),
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
  }
}
