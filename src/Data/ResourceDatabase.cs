using System;
using Godot;

namespace SettlersX.Data;

public partial class ResourceDatabase : Node
{
  private static ResourceDatabase? _instance;
  public static ResourceDatabase Instance => _instance
      ?? throw new InvalidOperationException("ResourceDatabase autoload not initialized");

  public override void _EnterTree()
  {
    if (_instance != null && _instance != this) { QueueFree(); return; }
    _instance = this;
  }

  public override void _ExitTree()
  {
    if (_instance == this) { _instance = null; }
  }

  public override void _Ready()
  {
    GD.Print("ResourceDatabase ready (no .tres files loaded yet)");
  }
}
