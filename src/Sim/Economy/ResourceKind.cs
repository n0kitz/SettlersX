namespace SettlersX.Sim.Economy;

/// <summary>
/// Resource taxonomy. F2a ships only Wood and Plank (the first vertical chain).
/// Phase 2b expands this to the full five-resource set; never reorder existing
/// values — the underlying integer is part of the save schema.
/// </summary>
public enum ResourceKind
{
  None = 0,
  Wood = 1,
  Plank = 2,
}
