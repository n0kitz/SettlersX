using Godot;
using SettlersX.Core;
using SettlersX.Sim.World.Intents;

namespace SettlersX.Views.World;

/// <summary>
/// Phase 1 placement controller: hotkey "1" places a storehouse at the hovered hex.
/// Submits a <see cref="PlaceBuildingIntent"/> via GameManager; the sim decides
/// whether to accept and emits BuildingConstructed via EventBus on success.
/// </summary>
public partial class PlacementController : Node
{
  public HexPicker? Picker { get; set; }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (Picker == null || !Picker.HasHover) { return; }
    if (@event is not InputEventKey key) { return; }
    if (!key.Pressed || key.Echo) { return; }
    if (key.Keycode != Key.Key1) { return; }
    GameManager.Instance.SubmitIntent(
        new PlaceBuildingIntent("storehouse", Picker.HoveredHex, OwnerPlayerId: 0));
  }
}
