using System;
using Godot;

namespace SettlersX.Core;

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
}
