using SettlersX.Sim.Economy;

namespace SettlersX.Sim.Production;

/// <summary>
/// Per-building production buffer + timer. Holds two inventories (input, output) and
/// a countdown timer. <c>Tick</c> advances exactly one step per sim tick: it pulls
/// inputs and starts the timer when idle, or advances the timer toward output release.
/// </summary>
public class ProductionState
{
  public Recipe Recipe { get; }
  public int InputCapacity { get; }
  public int OutputCapacity { get; }
  public Inventory Input { get; } = new();
  public Inventory Output { get; } = new();
  public int Timer { get; private set; }

  public bool Working => Timer > 0;

  public ProductionState(Recipe recipe, int inputCapacity, int outputCapacity)
  {
    Recipe = recipe;
    InputCapacity = inputCapacity;
    OutputCapacity = outputCapacity;
  }

  /// <summary>
  /// True iff the building is starved — recipe has an input, no input is buffered,
  /// and nothing is in progress. The HUD's chain-failure indicator reads this.
  /// </summary>
  public bool IsStarved =>
      Recipe.IsValid && Recipe.HasInput && !Working
      && Input.Get(Recipe.Input.Kind) < Recipe.Input.Count;

  /// <summary>
  /// True iff the building is blocked — its output buffer is at capacity and a new
  /// run cannot complete. Producers in this state stop pulling inputs.
  /// </summary>
  public bool IsBlocked =>
      Recipe.IsValid
      && Output.Get(Recipe.Output.Kind) + Recipe.Output.Count > OutputCapacity;

  public bool InputCanAccept(ResourceKind kind, int count) =>
      Recipe.HasInput
      && kind == Recipe.Input.Kind
      && Input.Get(kind) + count <= InputCapacity;

  /// <summary>Single per-tick step. Call from ProductionSystem.</summary>
  public void Tick()
  {
    if (!Recipe.IsValid) { return; }
    if (Working)
    {
      Timer--;
      if (Timer == 0)
      {
        Output.Add(Recipe.Output.Kind, Recipe.Output.Count);
      }
      return;
    }
    if (IsBlocked) { return; }
    if (Recipe.HasInput)
    {
      if (Input.Get(Recipe.Input.Kind) < Recipe.Input.Count) { return; }
      Input.TryRemove(Recipe.Input.Kind, Recipe.Input.Count);
    }
    Timer = Recipe.DurationTicks;
  }
}
