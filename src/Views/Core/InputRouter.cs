using Godot;

namespace SettlersX.Views.Core;

public partial class InputRouter : Node
{
  public static InputRouter Instance { get; private set; } = null!;

  public override void _EnterTree()
  {
    if (Instance != null && Instance != this) { QueueFree(); return; }
    Instance = this;
  }

  public override void _ExitTree()
  {
    if (Instance == this) { Instance = null!; }
  }
}
