using Godot;
using SettlersX.Core;

namespace SettlersX.Views.World;

public partial class MainView : Node3D
{
  private EventBus? _eventBus;

  public override void _Ready()
  {
    GD.Print("MainView ready");
    _eventBus = EventBus.Instance;
    _eventBus.GameTicked += OnGameTicked;
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(_eventBus))
    {
      _eventBus!.GameTicked -= OnGameTicked;
    }
  }

  private void OnGameTicked(int tick)
  {
    // Hook for debug HUD later
  }
}
