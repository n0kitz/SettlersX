using System;
using Godot;
using SettlersX.Core;

namespace SettlersX.Views.Core;

/// <summary>
/// Translates raw InputEvents into game intents.
/// All pause/speed/camera input lives here — never in WorldSim or other autoloads.
/// </summary>
public partial class InputRouter : Node
{
  private static InputRouter? _instance;
  public static InputRouter Instance => _instance
      ?? throw new InvalidOperationException("InputRouter autoload not initialized");

  public override void _EnterTree()
  {
    if (_instance != null && _instance != this) { QueueFree(); return; }
    _instance = this;
  }

  public override void _ExitTree()
  {
    if (_instance == this) { _instance = null; }
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event.IsActionPressed("ui_accept"))  // Space — remap in Phase 1
    {
      WorldSim.Instance.Paused = !WorldSim.Instance.Paused;
    }
  }
}
