using System.Collections.Generic;

namespace SettlersX.Sim.World;

/// <summary>
/// Owns the canonical list of placed buildings indexed by hex.
/// Phase 1: only storehouses. Footprint of 1 hex per building (multi-hex footprints arrive in F2a).
/// </summary>
public class BuildingRegistry
{
  private readonly Dictionary<HexCoord, Building> _byHex = new();
  private readonly List<Building> _all = new();
  private int _nextId = 1;

  public IReadOnlyList<Building> All => _all;
  public int Count => _all.Count;

  public bool TryAdd(string defId, HexCoord origin, int ownerPlayerId, out Building building)
  {
    if (_byHex.ContainsKey(origin))
    {
      building = null!;
      return false;
    }
    building = new Building(_nextId++, defId, origin, ownerPlayerId);
    _byHex[origin] = building;
    _all.Add(building);
    return true;
  }

  public Building? At(HexCoord hex) =>
      _byHex.TryGetValue(hex, out var b) ? b : null;
}
