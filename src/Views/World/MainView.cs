using Godot;
using SettlersX.Core;

namespace SettlersX.Views.World;

public partial class MainView : Node3D
{
  public override void _Ready()
  {
    GD.Print("MainView ready");
    EventBus.Instance.GameTicked += OnGameTicked;
  }

  public override void _ExitTree()
  {
    EventBus.Instance.GameTicked -= OnGameTicked;
  }

  private void OnGameTicked(int tick)
  {
    // Hook for debug HUD later
  }
}
