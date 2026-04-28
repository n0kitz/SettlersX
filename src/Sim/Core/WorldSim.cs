using Godot;
using SettlersX.Sim.Core;

namespace SettlersX.Core;

public partial class WorldSim : Node
{
  public static WorldSim Instance { get; private set; } = null!;

  public TickEngine Engine { get; private set; } = null!;
  public double Alpha => Engine.Alpha;
  public bool Paused
  {
    get => Engine.Paused;
    set => Engine.Paused = value;
  }

  public override void _EnterTree()
  {
    if (Instance != null && Instance != this) { QueueFree(); return; }
    Instance = this;
  }

  public override void _Ready()
  {
    // Read tick rate from balance.json later — hardcode for now
    Engine = new TickEngine(tickHz: 10.0);
  }

  public override void _ExitTree()
  {
    if (Instance == this) { Instance = null!; }
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

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event.IsActionPressed("ui_accept"))  // Space — remap later
    {
      Paused = !Paused;
    }
  }
}
