using System;
using System.Collections.Generic;

namespace SettlersX.Sim.World;

/// <summary>
/// Sector ownership and hex-to-sector membership. Independent from HexGrid (two-map model).
/// Phase 1: static once constructed; capture mechanics arrive in Phase 3.
/// </summary>
public class SectorGraph
{
  private readonly Dictionary<SectorId, int> _owners = new();
  private readonly Dictionary<HexCoord, SectorId> _hexToSector = new();
  private readonly Dictionary<SectorId, List<HexCoord>> _sectorHexes = new();

  public IEnumerable<SectorId> Sectors => _owners.Keys;

  public int Count => _owners.Count;

  public void AddSector(SectorId id, int ownerPlayerId, IEnumerable<HexCoord> hexes)
  {
    if (_owners.ContainsKey(id))
    {
      throw new ArgumentException($"Sector {id.Value} already exists", nameof(id));
    }
    _owners[id] = ownerPlayerId;
    var list = new List<HexCoord>();
    foreach (var hex in hexes)
    {
      if (_hexToSector.ContainsKey(hex))
      {
        throw new ArgumentException(
            $"Hex {hex} already belongs to sector {_hexToSector[hex].Value}", nameof(hexes));
      }
      _hexToSector[hex] = id;
      list.Add(hex);
    }
    _sectorHexes[id] = list;
  }

  public int GetOwner(SectorId id)
  {
    if (!_owners.TryGetValue(id, out var owner))
    {
      throw new KeyNotFoundException($"Sector {id.Value} not found");
    }
    return owner;
  }

  public SectorId? GetSectorOf(HexCoord hex) =>
      _hexToSector.TryGetValue(hex, out var id) ? id : null;

  public IReadOnlyList<HexCoord> HexesOf(SectorId id)
  {
    if (!_sectorHexes.TryGetValue(id, out var list))
    {
      throw new KeyNotFoundException($"Sector {id.Value} not found");
    }
    return list;
  }
}
