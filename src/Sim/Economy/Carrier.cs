using System.Collections.Generic;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Economy;

/// <summary>
/// One carrier unit. Carriers belong to storehouses (Settlers 7 rule, F2a). Phase 1
/// uses a single carrier shuttling between two storehouses to exercise tick-driven
/// movement and view-side interpolation.
/// </summary>
public class Carrier
{
  public int Id { get; }
  public CarrierState State { get; private set; }
  public HexCoord Hex { get; private set; }
  public HexCoord PrevHex { get; private set; }
  public IReadOnlyList<HexCoord>? Path { get; private set; }
  public int PathIndex { get; private set; }

  public Carrier(int id, HexCoord start)
  {
    Id = id;
    Hex = start;
    PrevHex = start;
    State = CarrierState.Idle;
  }

  public void AssignPath(IReadOnlyList<HexCoord> path)
  {
    if (path.Count == 0) { return; }
    Path = path;
    PathIndex = 0;
    Hex = path[0];
    PrevHex = path[0];
    State = path.Count > 1 ? CarrierState.Moving : CarrierState.Arrived;
  }

  /// <summary>Advance one hex along the assigned path. No-op if not Moving.</summary>
  public void Step()
  {
    if (State != CarrierState.Moving || Path == null) { return; }
    PrevHex = Hex;
    PathIndex++;
    if (PathIndex >= Path.Count)
    {
      PathIndex = Path.Count - 1;
    }
    Hex = Path[PathIndex];
    if (PathIndex >= Path.Count - 1)
    {
      State = CarrierState.Arrived;
    }
  }

  public void Reset()
  {
    Path = null;
    PathIndex = 0;
    State = CarrierState.Idle;
  }
}
