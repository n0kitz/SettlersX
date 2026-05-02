namespace SettlersX.Sim.Economy;

/// <summary>
/// Immutable (kind, count) pair. Used for recipe inputs/outputs, job payloads,
/// and construction costs. <c>IsEmpty</c> means "no work to do here".
/// </summary>
public readonly record struct ResourceStack(ResourceKind Kind, int Count)
{
  public bool IsEmpty => Kind == ResourceKind.None || Count <= 0;
}
