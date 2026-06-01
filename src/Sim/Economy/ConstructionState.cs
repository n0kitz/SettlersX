namespace SettlersX.Sim.Economy;

/// <summary>
/// Per-construction-site state. Tracks the resource cost target, what has been
/// delivered so far, and the DefId/Kind the site upgrades into when complete.
/// Mutated by JobBoard deliveries and consumed by ProductionSystem.FinalizeConstruction.
/// </summary>
public class ConstructionState
{
  public ResourceStack Cost { get; }
  public string FinishedDefId { get; }
  public Inventory Delivered { get; } = new();

  public ConstructionState(ResourceStack cost, string finishedDefId)
  {
    Cost = cost;
    FinishedDefId = finishedDefId;
  }

  public int Outstanding =>
      Cost.IsEmpty ? 0 : System.Math.Max(0, Cost.Count - Delivered.Get(Cost.Kind));

  public bool IsComplete => Outstanding == 0 && !Cost.IsEmpty;
}
