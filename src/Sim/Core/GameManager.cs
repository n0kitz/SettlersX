using Godot;

namespace SettlersX.Core;

public partial class GameManager : Node
{
  public static GameManager Instance { get; private set; } = null!;

  public override void _EnterTree()
  {
    if (Instance != null && Instance != this) { QueueFree(); return; }
    Instance = this;
  }

  public override void _Ready()
  {
    GD.Print("GameManager ready");
  }

  public override void _ExitTree()
  {
    if (Instance == this) { Instance = null!; }
  }
}
