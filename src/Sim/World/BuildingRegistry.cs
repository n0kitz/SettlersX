using System;
using System.Collections.Generic;

namespace SettlersX.Sim.World;

/// <summary>
/// Owns the canonical list of placed buildings indexed by hex. Looks up specs
/// through an injected <see cref="BuildingCatalog"/>; <c>TryAdd</c> rejects unknown
/// DefIds and occupied hexes. Footprint is 1 hex per building in F2a.
/// </summary>
public class BuildingRegistry
{
  private readonly BuildingCatalog _catalog;
  private readonly Dictionary<HexCoord, Building> _byHex = new();
  private readonly List<Building> _all = new();
  private int _nextId = 1;

  public BuildingRegistry(BuildingCatalog catalog)
  {
    _catalog = catalog;
  }

  public IReadOnlyList<Building> All => _all;
  public int Count => _all.Count;

  public bool TryAdd(string defId, HexCoord origin, int ownerPlayerId, out Building building)
  {
    var spec = _catalog.Get(defId);
    if (spec == null) { building = null!; return false; }
    if (_byHex.ContainsKey(origin)) { building = null!; return false; }
    building = new Building(_nextId++, spec, origin, ownerPlayerId);
    _byHex[origin] = building;
    _all.Add(building);
    return true;
  }

  public Building? At(HexCoord hex) =>
      _byHex.TryGetValue(hex, out var b) ? b : null;

  /// <summary>
  /// Promote the construction site at <paramref name="hex"/> to its finished form.
  /// Caller must ensure <c>Construction.IsComplete</c>. Throws if the spec for the
  /// finished DefId is missing — that is a catalog bug, not runtime user error.
  /// </summary>
  public void FinalizeConstruction(HexCoord hex)
  {
    var building = At(hex);
    if (building?.Construction == null) { return; }
    if (!building.Construction.IsComplete) { return; }
    var finishedSpec = _catalog.Get(building.Construction.FinishedDefId)
        ?? throw new InvalidOperationException(
            $"Catalog missing finished spec '{building.Construction.FinishedDefId}'.");
    building.FinalizeConstruction(finishedSpec);
  }
}
