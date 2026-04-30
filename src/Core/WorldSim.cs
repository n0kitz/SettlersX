using System;
using Godot;
using SettlersX.Sim.Core;

namespace SettlersX.Core;

/// <summary>
/// Drives the simulation tick loop each frame via TickEngine.Advance.
/// Emits <see cref="EventBus.GameTicked"/> for every tick fired.
/// Exposes Alpha for view-layer interpolation between ticks.
/// </summary>
public partial class WorldSim : Node
{
  private static WorldSim? _instance;
  public static WorldSim Instance => _instance
      ?? throw new InvalidOperationException("WorldSim autoload not initialized");

  public TickEngine? Engine { get; private set; }

  /// <summary>Interpolation alpha [0,1] between last and next tick. Returns 0 before _Ready.</summary>
  public double Alpha => Engine?.Alpha ?? 0.0;

  /// <summary>Pause state. Setter is a no-op before _Ready.</summary>
  public bool Paused
  {
    get => Engine?.Paused ?? false;
    set { if (Engine != null) { Engine.Paused = value; } }
  }

  public override void _EnterTree()
  {
    if (_instance != null && _instance != this) { QueueFree(); return; }
    _instance = this;
  }

  public override void _Ready()
  {
    var json = FileAccess.GetFileAsString("res://data/balance.json");
    var balance = Newtonsoft.Json.Linq.JObject.Parse(json);
    var hz = (double)(balance["tick"]!["hz"]!);
    Engine = new TickEngine(tickHz: hz);
  }

  public override void _ExitTree()
  {
    if (_instance == this) { _instance = null; }
  }

  public override void _Process(double delta)
  {
    if (Engine == null) { return; }
    var ticks = Engine.Advance(delta);
    if (ticks <= 0) { return; }
    var bus = EventBus.Instance;
    for (var i = 0; i < ticks; i++)
    {
      bus.EmitSignal(EventBus.SignalName.GameTicked, Engine.TickNumber);
    }
  }

  // Pause toggling is handled by InputRouter._UnhandledInput — do not add input
  // handling here. See src/Views/Core/InputRouter.cs.
}
