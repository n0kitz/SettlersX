using Godot;

namespace SettlersX.Core;

public partial class EventBus : Node
{
  public static EventBus Instance { get; private set; } = null!;

  public override void _EnterTree()
  {
    if (Instance != null && Instance != this) { QueueFree(); return; }
    Instance = this;
  }

  public override void _ExitTree()
  {
    if (Instance == this) { Instance = null!; }
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
