using Godot;
using SettlersX.Core;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;

namespace SettlersX.Views.World;

/// <summary>
/// Click-drag road builder. Mouse down arms the controller; while held, every time
/// the picker reports a new hovered hex adjacent to the previous one, a
/// <see cref="BuildRoadIntent"/> is submitted. Mouse up disarms.
/// </summary>
public partial class RoadBuildController : Node
{
  public HexPicker? Picker { get; set; }

  private bool _dragging;
  private HexCoord _last;
  private bool _hasLast;

  public override void _Ready()
  {
    if (Picker != null) { Picker.HoveredHexChanged += OnHover; }
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(Picker)) { Picker!.HoveredHexChanged -= OnHover; }
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (Picker == null) { return; }
    if (@event is not InputEventMouseButton mb) { return; }
    if (mb.ButtonIndex != MouseButton.Left) { return; }
    if (mb.Pressed)
    {
      _dragging = true;
      if (Picker.HasHover)
      {
        _last = Picker.HoveredHex;
        _hasLast = true;
      }
    }
    else
    {
      _dragging = false;
      _hasLast = false;
    }
  }

  private void OnHover(HexCoord newHex)
  {
    if (!_dragging) { return; }
    if (_hasLast && _last.Distance(newHex) == 1)
    {
      GameManager.Instance.SubmitIntent(new BuildRoadIntent(_last, newHex));
    }
    _last = newHex;
    _hasLast = true;
  }
}
