using SettlersX.Sim.World;

namespace SettlersX.Sim.Economy;

/// <summary>
/// One carrier dispatch: move a single <see cref="ResourceStack"/> from
/// <c>Source</c> to <c>Destination</c>. Created and reserved by JobBoard at the
/// moment of assignment — the source's output buffer is decremented immediately,
/// so duplicate dispatches cannot race. <see cref="JobBoard.Refund"/> restores
/// the reservation when the carrier cannot reach the destination.
/// </summary>
public readonly record struct Job(
    Building Source,
    Building Destination,
    ResourceStack Stack,
    JobKind Kind);

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
