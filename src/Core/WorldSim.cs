using System;
using Godot;
using SettlersX.Sim.Core;
using SettlersX.Sim.World;

namespace SettlersX.Core;

/// <summary>
/// Drives the simulation tick loop each frame via TickEngine.Advance.
/// Emits <see cref="EventBus.GameTicked"/> for every tick fired.
/// Exposes Alpha for view-layer interpolation between ticks.
/// </summary>
public partial class WorldSim : Node
{
  private const int DefaultGridWidth = 20;
  private const int DefaultGridHeight = 20;

  private static WorldSim? _instance;
  public static WorldSim Instance => _instance
      ?? throw new InvalidOperationException("WorldSim autoload not initialized");

  public TickEngine? Engine { get; private set; }
  public WorldState? World { get; private set; }
  public double HexSize { get; private set; } = 1.0;

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
    HexSize = (double)(balance["hex"]!["size"]!);
    Engine = new TickEngine(tickHz: hz);
    World = new WorldState(DefaultGridWidth, DefaultGridHeight);
    World.BuildingPlaced += OnBuildingPlaced;
    World.RoadBuilt += OnRoadBuilt;
  }

  public override void _ExitTree()
  {
    if (World != null)
    {
      World.BuildingPlaced -= OnBuildingPlaced;
      World.RoadBuilt -= OnRoadBuilt;
    }
    if (_instance == this) { _instance = null; }
  }

  private void OnBuildingPlaced(Building building)
  {
    EventBus.Instance.EmitSignal(
        EventBus.SignalName.BuildingConstructed,
        building.DefId,
        new Vector2I(building.Origin.Q, building.Origin.R));
  }

  private void OnRoadBuilt(HexCoord a, HexCoord b)
  {
    EventBus.Instance.EmitSignal(
        EventBus.SignalName.RoadBuilt,
        new Vector2I(a.Q, a.R),
        new Vector2I(b.Q, b.R));
  }

  public override void _Process(double delta)
  {
    if (Engine == null) { return; }
    var ticks = Engine.Advance(delta);
    if (ticks <= 0) { return; }
    var bus = EventBus.Instance;
    for (var i = 0; i < ticks; i++)
    {
      World?.Tick();
      bus.EmitSignal(EventBus.SignalName.GameTicked, Engine.TickNumber);
    }
  }

  /// <summary>
  /// Manually advance one tick — bound to the <c>step_once</c> input action. Bumps
  /// TickEngine, runs the sim tick, and emits GameTicked exactly once.
  /// </summary>
  public void StepOnce()
  {
    if (Engine == null) { return; }
    Engine.StepOnce();
    World?.Tick();
    EventBus.Instance.EmitSignal(EventBus.SignalName.GameTicked, Engine.TickNumber);
  }

  // Pause toggling is handled by InputRouter._UnhandledInput — do not add input
  // handling here. See src/Views/Core/InputRouter.cs.
}
