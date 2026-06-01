namespace SettlersX.Sim.Economy;

/// <summary>
/// What the carrier does on arrival at <see cref="Job.Destination"/>.
/// PushToStock: producer → storehouse (deposit into Stock).
/// PullToInput: storehouse → consumer (deposit into Production.Input).
/// PullToConstruction: storehouse → site (deposit into Construction.Delivered).
/// </summary>
public enum JobKind
{
  PushToStock = 1,
  PullToInput = 2,
  PullToConstruction = 3,
}
