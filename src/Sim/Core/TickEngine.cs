using System;

namespace SettlersX.Sim.Core;

/// <summary>
/// Pure C# tick accumulator. Testable without Godot runtime.
/// </summary>
public class TickEngine
{
  /// <summary>Maximum ticks allowed per Advance() call. Prevents runaway on huge delta.</summary>
  public const int MaxTicksPerFrame = 10;

  public int TickNumber { get; private set; }
  public bool Paused { get; set; }
  public double TickInterval { get; }

  private double _accumulator;

  public TickEngine(double tickHz = 10.0)
  {
    if (tickHz <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tickHz),
          $"tickHz must be > 0, got {tickHz}");
    }
    TickInterval = 1.0 / tickHz;
  }

  /// <summary>
  /// Advance time. Returns number of ticks that fired (capped at MaxTicksPerFrame).
  /// </summary>
  public int Advance(double delta)
  {
    if (delta < 0) { return 0; }
    if (Paused) { return 0; }
    _accumulator += delta;
    var ticks = 0;
    while (_accumulator >= TickInterval && ticks < MaxTicksPerFrame)
    {
      _accumulator -= TickInterval;
      TickNumber++;
      ticks++;
    }
    // Drain leftover accumulator if we hit the cap, to avoid runaway on next frame
    if (ticks == MaxTicksPerFrame)
    {
      _accumulator = 0.0;
    }
    return ticks;
  }

  /// <summary>
  /// Visual interpolation alpha [0,1] between last and next tick.
  /// </summary>
  public double Alpha => _accumulator / TickInterval;

  /// <summary>
  /// Manually fire one tick (debug/testing). Resets accumulator to avoid surprise ticks on next Advance.
  /// </summary>
  public void StepOnce()
  {
    TickNumber++;
    _accumulator = 0.0;
  }
}
