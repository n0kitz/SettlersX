using System;
using Godot;
using SettlersX.Sim.World.Intents;

namespace SettlersX.Core;

/// <summary>
/// Owns match-level state: players, active victory paths, and game phase transitions.
/// Phase 1 surface: <see cref="SubmitIntent"/> forwards player intents to WorldSim's
/// IntentQueue, which the sim drains at the Input phase of the next tick.
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

  /// <summary>
  /// Queue a player or AI intent for the next tick's Input phase.
  /// Raises EventBus.IntentSubmitted; WorldSim enqueues it into the sim on receive.
  /// </summary>
  public void SubmitIntent(IIntent intent)
  {
    EventBus.Instance.RaiseIntentSubmitted(intent);
  }
}
