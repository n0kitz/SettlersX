namespace SettlersX.Sim.Economy;

/// <summary>
/// Single-input, single-output production recipe. Raw producers (e.g. LumberCamp)
/// set <c>Input</c> to <c>default</c> (empty). DurationTicks is the wall-clock cost
/// at the configured tick rate (10 Hz default = 30 ticks ≈ 3 seconds).
/// </summary>
public readonly record struct Recipe(
    ResourceStack Input,
    ResourceStack Output,
    int DurationTicks)
{
  public bool HasInput => !Input.IsEmpty;
  public bool IsValid => DurationTicks > 0 && !Output.IsEmpty;
}
