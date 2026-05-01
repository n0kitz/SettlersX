using System.Linq;
using Godot;
using SettlersX.Core;

namespace SettlersX.Views.UI;

/// <summary>
/// Phase 1 debug overlay: tick number, paused state, first-carrier hex and state.
/// Replaced by an in-game HUD in F2a; this view exists to satisfy F1 exit criterion 9.
/// </summary>
public partial class DebugHud : CanvasLayer
{
  private Label? _label;
  private EventBus? _bus;

  public override void _Ready()
  {
    _label = new Label
    {
      Text = "tick: 0",
      Position = new Vector2(12f, 12f),
    };
    _label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
    _label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f));
    _label.AddThemeConstantOverride("outline_size", 4);
    AddChild(_label);

    _bus = EventBus.Instance;
    _bus.GameTicked += OnGameTicked;
    Refresh(0);
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(_bus))
    {
      _bus!.GameTicked -= OnGameTicked;
    }
  }

  private void OnGameTicked(int tick) => Refresh(tick);

  private void Refresh(int tick)
  {
    if (_label == null) { return; }
    var sim = WorldSim.Instance;
    var paused = sim.Paused ? "yes" : "no";
    var carrier = sim.World?.Carriers.FirstOrDefault();
    var hex = carrier != null ? $"({carrier.Hex.Q},{carrier.Hex.R})" : "—";
    var state = carrier != null ? carrier.State.ToString() : "—";
    _label.Text =
        $"tick: {tick}\n" +
        $"paused: {paused}\n" +
        $"carrier hex: {hex}\n" +
        $"carrier state: {state}";
  }
}
