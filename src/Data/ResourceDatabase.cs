using Godot;

namespace SettlersX.Data;

public partial class ResourceDatabase : Node
{
  public static ResourceDatabase Instance { get; private set; } = null!;

  public override void _EnterTree()
  {
    if (Instance != null && Instance != this) { QueueFree(); return; }
    Instance = this;
  }

  public override void _ExitTree()
  {
    if (Instance == this) { Instance = null!; }
  }

  public override void _Ready()
  {
    GD.Print("ResourceDatabase ready (no .tres files loaded yet)");
  }
}
