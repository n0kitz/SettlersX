using System;
using Godot;

namespace SettlersX.Data;

/// <summary>
/// Loads and caches typed Resource assets (.tres) at startup.
/// Query by concrete type — e.g. <c>ResourceDatabase.Instance.Get&lt;BuildingDef&gt;("lumberjack")</c>.
/// Stub in Phase 0 — asset loading begins in Phase 1.
/// </summary>
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
