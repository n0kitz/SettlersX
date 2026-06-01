using Godot;
using SettlersX.Core;
using SettlersX.Sim.World;
using SettlersX.Sim.World.Intents;

namespace SettlersX.Views.World;

/// <summary>
/// F2a placement controller: each number key 1-4 places a different building at the
/// hovered hex. The mapping mirrors <see cref="BuildingCatalog"/> DefIds and avoids
/// a separate selection-mode model — one keypress, one intent.
/// </summary>
public partial class PlacementController : Node
{
  public HexPicker? Picker { get; set; }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (Picker == null || !Picker.HasHover) { return; }
    if (@event is not InputEventKey key) { return; }
    if (!key.Pressed || key.Echo) { return; }

    var defId = DefIdForKey(key.Keycode);
    if (defId == null) { return; }
    GameManager.Instance.SubmitIntent(
        new PlaceBuildingIntent(defId, Picker.HoveredHex, OwnerPlayerId: 0));
  }

  private static string? DefIdForKey(Key keycode) => keycode switch
  {
    Key.Key1 => BuildingCatalog.StorehouseId,
    Key.Key2 => BuildingCatalog.LumberCampId,
    Key.Key3 => BuildingCatalog.SawmillId,
    Key.Key4 => BuildingCatalog.ConstructionSiteId,
    _ => null,
  };
}
