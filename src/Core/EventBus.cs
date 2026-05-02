using System;
using Godot;

namespace SettlersX.Core;

/// <summary>
/// Signal hub for all cross-system communication. Every inter-layer signal is declared here.
/// Must be the first autoload — all other autoloads may subscribe in their _Ready.
/// </summary>
public partial class EventBus : Node
{
  private static EventBus? _instance;
  public static EventBus Instance => _instance
      ?? throw new InvalidOperationException("EventBus autoload not initialized");

  public override void _EnterTree()
  {
    if (_instance != null && _instance != this) { QueueFree(); return; }
    _instance = this;
  }

  public override void _ExitTree()
  {
    if (_instance == this) { _instance = null; }
  }

  [Signal]
  public delegate void BuildingConstructedEventHandler(
      string buildingId, Vector2I hexCell);
  [Signal]
  public delegate void SectorCapturedEventHandler(
      int sectorId, int newOwnerPlayerId);
  [Signal]
  public delegate void ResourceShortageEventHandler(
      string goodId, int sectorId);
  [Signal]
  public delegate void VictoryAchievedEventHandler(
      int playerId, string victoryPath);
  [Signal] public delegate void GameTickedEventHandler(int tickNumber);

  [Signal]
  public delegate void RoadBuiltEventHandler(Vector2I a, Vector2I b);

  /// <summary>
  /// Fires once a construction site is promoted to its finished form. Carries the
  /// new DefId (e.g. <c>"house"</c>) so the view can re-skin without reading sim state.
  /// </summary>
  [Signal]
  public delegate void BuildingFinalizedEventHandler(string defId, Vector2I hexCell);
}
