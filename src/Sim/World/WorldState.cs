using System;
using System.Collections.Generic;
using SettlersX.Sim.Economy;
using SettlersX.Sim.Pathfinding;
using SettlersX.Sim.World.Intents;

namespace SettlersX.Sim.World;

/// <summary>
/// Root container for all sim state. Constructed by WorldSim after balance load.
/// Tick() is the per-tick entry point invoked from WorldSim._Process.
/// </summary>
public class WorldState
{
  public HexGrid Grid { get; }
  public SectorGraph Sectors { get; }
  public RoadGraph Roads { get; }
  public BuildingRegistry Buildings { get; }
  public IntentQueue Intents { get; }
  public List<Carrier> Carriers { get; } = new();

  private int _nextCarrierId = 1;

  /// <summary>
  /// Plain C# event fired after a building is placed during the Input phase.
  /// WorldSim subscribes and republishes via EventBus.BuildingConstructed.
  /// </summary>
  public event Action<Building>? BuildingPlaced;

  /// <summary>
  /// Plain C# event fired after a road edge is added during the Input phase.
  /// WorldSim subscribes and republishes via EventBus.RoadBuilt.
  /// </summary>
  public event Action<HexCoord, HexCoord>? RoadBuilt;

  /// <summary>Plain C# event fired when a carrier is spawned.</summary>
  public event Action<Carrier>? CarrierSpawned;

  public WorldState(int gridWidth, int gridHeight)
  {
    Grid = new HexGrid(gridWidth, gridHeight);
    Sectors = BuildPlaceholderSectors(Grid);
    Roads = new RoadGraph();
    Buildings = new BuildingRegistry();
    Intents = new IntentQueue();
  }

  /// <summary>
  /// Tick pipeline: Input → AI → Pathfinding → Production → Transport →
  /// Consumption → Combat → Diplomacy. Phases without systems are no-ops.
  /// </summary>
  public void Tick()
  {
    ApplyIntents();
    // AI phase — F2b+
    // Pathfinding phase — dirty-path recomputation lands with chain logic in F2a
    TransportSystem.Tick(this);
    // Consumption phase — F2a
    // Combat phase — F3
    // Diplomacy phase — F3+
  }

  public Carrier SpawnCarrier(HexCoord origin)
  {
    var carrier = new Carrier(_nextCarrierId++, origin);
    Carriers.Add(carrier);
    CarrierSpawned?.Invoke(carrier);
    return carrier;
  }

  /// <summary>
  /// View-facing per-tick snapshot. Returns prev/next world positions for every
  /// carrier, computed at the supplied hex size. Views interpolate between the
  /// two using <c>WorldSim.Alpha</c>; they must NOT read carrier state directly.
  /// </summary>
  public IReadOnlyList<CarrierSnapshot> SnapshotCarriers(double hexSize)
  {
    if (Carriers.Count == 0) { return Array.Empty<CarrierSnapshot>(); }
    var list = new List<CarrierSnapshot>(Carriers.Count);
    foreach (var c in Carriers)
    {
      list.Add(new CarrierSnapshot(
          c.Id,
          c.PrevHex.ToWorld(hexSize),
          c.Hex.ToWorld(hexSize),
          c.State,
          c.Hex));
    }
    return list;
  }

  private void ApplyIntents()
  {
    foreach (var intent in Intents.Drain())
    {
      switch (intent)
      {
        case PlaceBuildingIntent place:
          ApplyPlaceBuilding(place);
          break;
        case BuildRoadIntent road:
          ApplyBuildRoad(road);
          break;
      }
    }
  }

  private void ApplyPlaceBuilding(PlaceBuildingIntent intent)
  {
    if (!Grid.InBounds(intent.Cell)) { return; }
    if (Buildings.TryAdd(intent.DefId, intent.Cell, intent.OwnerPlayerId, out var building))
    {
      BuildingPlaced?.Invoke(building);
    }
  }

  private void ApplyBuildRoad(BuildRoadIntent intent)
  {
    if (!Grid.InBounds(intent.A) || !Grid.InBounds(intent.B)) { return; }
    if (Roads.TryAddEdge(intent.A, intent.B))
    {
      RoadBuilt?.Invoke(intent.A, intent.B);
    }
  }

  /// <summary>
  /// Phase 1 placeholder: partition the grid into 3 column-stripes, all owned by player 0.
  /// Real sector authoring lands with map tooling in a later phase.
  /// </summary>
  private static SectorGraph BuildPlaceholderSectors(HexGrid grid)
  {
    var sectors = new SectorGraph();
    var third = Math.Max(1, grid.Width / 3);
    var buckets = new Dictionary<int, List<HexCoord>>
    {
      [0] = new List<HexCoord>(),
      [1] = new List<HexCoord>(),
      [2] = new List<HexCoord>(),
    };
    foreach (var hex in grid.All())
    {
      var idx = hex.Q < third ? 0 : hex.Q < 2 * third ? 1 : 2;
      buckets[idx].Add(hex);
    }
    for (var i = 0; i < 3; i++)
    {
      sectors.AddSector(new SectorId(i), ownerPlayerId: 0, buckets[i]);
    }
    return sectors;
  }
}
