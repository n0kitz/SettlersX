namespace SettlersX.Sim.Core;

/// <summary>
/// Pure C# tick accumulator. Testable without Godot runtime.
/// </summary>
public class TickEngine
{
  public int TickNumber { get; private set; }
  public bool Paused { get; set; }
  public double TickInterval { get; }

  private double _accumulator;

  public TickEngine(double tickHz = 10.0)
  {
    TickInterval = 1.0 / tickHz;
  }

  /// <summary>
  /// Advance time. Returns number of ticks that fired.
  /// </summary>
  public int Advance(double delta)
  {
    if (Paused) { return 0; }
    _accumulator += delta;
    var ticks = 0;
    while (_accumulator >= TickInterval)
    {
      _accumulator -= TickInterval;
      TickNumber++;
      ticks++;
    }
    return ticks;
  }

  /// <summary>
  /// Visual interpolation alpha [0,1] between last and next tick.
  /// </summary>
  public double Alpha => _accumulator / TickInterval;

  public void StepOnce()
  {
    TickNumber++;
  }
}
