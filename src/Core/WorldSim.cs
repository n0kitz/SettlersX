using System;
using Godot;
using SettlersX.Sim.Core;

namespace SettlersX.Core;

public partial class WorldSim : Node
{
  private static WorldSim? _instance;
  public static WorldSim Instance => _instance
      ?? throw new InvalidOperationException("WorldSim autoload not initialized");

  public TickEngine Engine { get; private set; } = null!;
  public double Alpha => Engine.Alpha;
  public bool Paused
  {
    get => Engine.Paused;
    set => Engine.Paused = value;
  }

  public override void _EnterTree()
  {
    if (_instance != null && _instance != this) { QueueFree(); return; }
    _instance = this;
  }

  public override void _Ready()
  {
    // Read tick rate from balance.json later — hardcode for now
    Engine = new TickEngine(tickHz: 10.0);
  }

  public override void _ExitTree()
  {
    if (_instance == this) { _instance = null; }
  }

  public override void _Process(double delta)
  {
    var ticks = Engine.Advance(delta);
    for (var i = 0; i < ticks; i++)
    {
      EventBus.Instance.EmitSignal(
          EventBus.SignalName.GameTicked, Engine.TickNumber);
    }
  }

  // Pause toggling is handled by InputRouter._UnhandledInput — do not add input
  // handling here. See src/Views/Core/InputRouter.cs.
}
