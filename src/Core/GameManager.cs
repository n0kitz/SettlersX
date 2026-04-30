using System;
using Godot;

namespace SettlersX.Core;

/// <summary>
/// Owns match-level state: players, active victory paths, and game phase transitions.
/// Stub in Phase 0 — implementation begins in Phase 2.
/// </summary>
public partial class GameManager : Node
{
  private static GameManager? _instance;
  public static GameManager Instance => _instance
      ?? throw new InvalidOperationException("GameManager autoload not initialized");

  public override void _EnterTree()
  {
    if (_instance != null && _instance != this) { QueueFree(); return; }
    _instance = this;
  }

  public override void _Ready()
  {
    GD.Print("GameManager ready");
  }

  public override void _ExitTree()
  {
    if (_instance == this) { _instance = null; }
  }
}
