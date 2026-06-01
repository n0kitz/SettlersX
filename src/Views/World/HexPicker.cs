using System;
using Godot;
using SettlersX.Sim.World;

namespace SettlersX.Views.World;

/// <summary>
/// Casts the mouse cursor onto the y=0 plane and reports the hex under it.
/// Subscribes nothing; emits a plain C# event so other views can react without going
/// through EventBus (this is per-frame UI, not a sim signal).
/// </summary>
public partial class HexPicker : Node
{
  public Camera3D? Camera { get; set; }
  public double HexSize { get; set; } = 1.0;
  public HexCoord HoveredHex { get; private set; }
  public bool HasHover { get; private set; }

  public event Action<HexCoord>? HoveredHexChanged;

  public override void _UnhandledInput(InputEvent @event)
  {
    if (Camera == null) { return; }
    if (@event is not InputEventMouseMotion motion) { return; }
    if (!TryRaycastGround(motion.Position, out var hit)) { return; }

    var hex = HexCoord.FromWorld(hit, HexSize);
    if (!HasHover || !hex.Equals(HoveredHex))
    {
      HoveredHex = hex;
      HasHover = true;
      HoveredHexChanged?.Invoke(hex);
    }
  }

  private bool TryRaycastGround(Vector2 screenPos, out Vector3 hit)
  {
    hit = default;
    var origin = Camera!.ProjectRayOrigin(screenPos);
    var dir = Camera.ProjectRayNormal(screenPos);
    if (Math.Abs(dir.Y) < 1e-6f) { return false; }
    var t = -origin.Y / dir.Y;
    if (t < 0) { return false; }
    hit = origin + dir * t;
    return true;
  }
}
