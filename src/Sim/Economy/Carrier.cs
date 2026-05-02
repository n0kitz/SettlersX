using System.Collections.Generic;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Economy;

/// <summary>
/// One carrier unit. Belongs to a home storehouse (Settlers 7 rule). The carrier
/// itself is dumb: it walks an assigned path one hex per sim tick. JobBoard +
/// TransportSystem own the dispatch logic and reservation accounting.
/// </summary>
public class Carrier
{
  public int Id { get; }
  public int HomeBuildingId { get; }
  public CarrierState State { get; private set; }
  public HexCoord Hex { get; private set; }
  public HexCoord PrevHex { get; private set; }
  public IReadOnlyList<HexCoord>? Path { get; private set; }
  public int PathIndex { get; private set; }

  public Job? CurrentJob { get; private set; }
  public ResourceStack Cargo { get; private set; }

  public Carrier(int id, HexCoord start) : this(id, start, homeBuildingId: 0) { }

  public Carrier(int id, HexCoord start, int homeBuildingId)
  {
    Id = id;
    HomeBuildingId = homeBuildingId;
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

  public void AssignJob(Job job, IReadOnlyList<HexCoord> path)
  {
    CurrentJob = job;
    Cargo = job.Stack;
    AssignPath(path);
  }

  public void ClearJob()
  {
    CurrentJob = null;
    Cargo = default;
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
    ClearJob();
  }
}
