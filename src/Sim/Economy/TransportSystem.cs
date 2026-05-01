using System.Collections.Generic;
using System.Linq;
using SettlersX.Sim.Pathfinding;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Economy;

/// <summary>
/// Transport phase of the tick pipeline. Phase 1 scope: ensure exactly one carrier
/// exists once two connected storehouses are on the map, and shuttle it back and
/// forth along the road graph. F2a expands this into job-board-driven dispatch.
/// </summary>
public static class TransportSystem
{
  public const string StorehouseDefId = "storehouse";

  public static void Tick(WorldState state)
  {
    EnsureCarrier(state);
    foreach (var carrier in state.Carriers)
    {
      switch (carrier.State)
      {
        case CarrierState.Idle:
        case CarrierState.Arrived:
          AssignNextRoute(state, carrier);
          break;
        case CarrierState.Moving:
          carrier.Step();
          break;
      }
    }
  }

  private static void EnsureCarrier(WorldState state)
  {
    if (state.Carriers.Count > 0) { return; }
    var storehouses = OrderedStorehouses(state);
    if (storehouses.Count < 2) { return; }
    var path = HexAStar.Find(state.Roads, storehouses[0].Origin, storehouses[1].Origin);
    if (path == null) { return; }
    state.SpawnCarrier(storehouses[0].Origin);
  }

  private static void AssignNextRoute(WorldState state, Carrier carrier)
  {
    var storehouses = OrderedStorehouses(state);
    if (storehouses.Count < 2)
    {
      carrier.Reset();
      return;
    }
    var aOrigin = storehouses[0].Origin;
    var bOrigin = storehouses[1].Origin;
    var goal = carrier.Hex.Equals(aOrigin) ? bOrigin : aOrigin;
    var path = HexAStar.Find(state.Roads, carrier.Hex, goal);
    if (path == null || path.Count < 2)
    {
      carrier.Reset();
      return;
    }
    carrier.AssignPath(path);
  }

  private static List<Building> OrderedStorehouses(WorldState state) =>
      state.Buildings.All
          .Where(b => b.DefId == StorehouseDefId)
          .OrderBy(b => b.Id)
          .ToList();
}
