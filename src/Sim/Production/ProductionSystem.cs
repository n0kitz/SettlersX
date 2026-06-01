using System.Collections.Generic;
using SettlersX.Sim.World;

namespace SettlersX.Sim.Production;

/// <summary>
/// Production phase of the tick pipeline. Walks every building, ticks its
/// <see cref="ProductionState"/> if any, and finalizes any construction site whose
/// delivered inventory has reached its cost target. Mutations stay local to each
/// building — JobBoard handles cross-building moves.
/// </summary>
public static class ProductionSystem
{
  public static void Tick(WorldState state)
  {
    foreach (var building in state.Buildings.All)
    {
      building.Production?.Tick();
    }

    // Snapshot completed sites first so we don't mutate the registry mid-iteration.
    List<HexCoord>? completed = null;
    foreach (var building in state.Buildings.All)
    {
      var construction = building.Construction;
      if (construction == null) { continue; }
      if (!construction.IsComplete) { continue; }
      completed ??= new List<HexCoord>();
      completed.Add(building.Origin);
    }

    if (completed == null) { return; }
    foreach (var hex in completed)
    {
      state.Buildings.FinalizeConstruction(hex);
      state.RaiseConstructionCompleted(hex);
    }
  }
}
