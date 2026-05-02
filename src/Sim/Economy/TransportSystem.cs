using System.Collections.Generic;
using SettlersX.Sim.Pathfinding;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Economy;

/// <summary>
/// Transport phase of the tick pipeline.
///
/// 1. Spawns carriers up to each storehouse's quota.
/// 2. Steps every Moving carrier one hex along its assigned path.
/// 3. On Arrived: delivers cargo at the job's destination, clears the job.
/// 4. On Idle: asks <see cref="JobBoard"/> for a job. If found, computes the
///    pickup→delivery path; on success the carrier accepts; on failure the
///    reservation is refunded so JobBoard does not lose the resource.
/// </summary>
public static class TransportSystem
{
  public const string StorehouseDefId = "storehouse";

  public static void Tick(WorldState state, int carriersPerStorehouse)
  {
    SpawnCarriers(state, carriersPerStorehouse);

    foreach (var carrier in state.Carriers)
    {
      switch (carrier.State)
      {
        case CarrierState.Moving:
          carrier.Step();
          break;
        case CarrierState.Arrived:
          DeliverCargo(carrier);
          AssignNextJob(state, carrier);
          break;
        case CarrierState.Idle:
          AssignNextJob(state, carrier);
          break;
      }
    }
  }

  private static void SpawnCarriers(WorldState state, int carriersPerStorehouse)
  {
    foreach (var b in state.Buildings.All)
    {
      if (b.Kind != BuildingKind.Storehouse) { continue; }
      var owned = CountCarriersForHome(state.Carriers, b.Id);
      var deficit = carriersPerStorehouse - owned;
      for (var i = 0; i < deficit; i++)
      {
        state.SpawnCarrier(b.Origin, b.Id);
      }
    }
  }

  private static int CountCarriersForHome(IReadOnlyList<Carrier> carriers, int homeId)
  {
    var count = 0;
    foreach (var c in carriers)
    {
      if (c.HomeBuildingId == homeId) { count++; }
    }
    return count;
  }

  private static void DeliverCargo(Carrier carrier)
  {
    if (carrier.CurrentJob is not Job job) { return; }
    if (carrier.Cargo.IsEmpty) { carrier.ClearJob(); return; }

    switch (job.Kind)
    {
      case JobKind.PushToStock:
        job.Destination.Stock?.Add(carrier.Cargo.Kind, carrier.Cargo.Count);
        break;
      case JobKind.PullToInput:
        job.Destination.Production?.Input.Add(carrier.Cargo.Kind, carrier.Cargo.Count);
        break;
      case JobKind.PullToConstruction:
        job.Destination.Construction?.Delivered.Add(carrier.Cargo.Kind, carrier.Cargo.Count);
        break;
    }
    carrier.ClearJob();
  }

  private static void AssignNextJob(WorldState state, Carrier carrier)
  {
    if (!JobBoard.TryFindJob(state, carrier.Hex, out var job)) { return; }

    var pickupPath = HexAStar.Find(state.Roads, carrier.Hex, job.Source.Origin);
    var deliveryPath = HexAStar.Find(state.Roads, job.Source.Origin, job.Destination.Origin);
    if (pickupPath == null || deliveryPath == null)
    {
      JobBoard.Refund(job);
      return;
    }

    var path = Concat(pickupPath, deliveryPath);
    if (path.Count < 2)
    {
      // src == dst (same hex). Synthesize a 1-step path so the carrier still
      // transitions through Arrived next tick and the cargo is delivered.
      path = new List<HexCoord> { carrier.Hex, job.Destination.Origin };
    }
    carrier.AssignJob(job, path);
  }

  private static List<HexCoord> Concat(
      IReadOnlyList<HexCoord> a, IReadOnlyList<HexCoord> b)
  {
    var result = new List<HexCoord>(a.Count + b.Count - 1);
    result.AddRange(a);
    for (var i = 1; i < b.Count; i++) { result.Add(b[i]); }
    return result;
  }
}
