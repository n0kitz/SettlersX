using Godot;
using SettlersX.Core;
using SettlersX.Views.Buildings;
using SettlersX.Views.UI;
using SettlersX.Views.Units;

namespace SettlersX.Views.World;

public partial class MainView : Node3D
{
  private HexGridView? _gridView;
  private HexPicker? _picker;
  private RoadView? _roadView;
  private BuildingsView? _buildingsView;
  private CarrierView? _carrierView;
  private PlacementController? _placement;
  private RoadBuildController? _roadBuild;
  private DebugHud? _hud;

  public override void _Ready()
  {
    GD.Print("MainView ready");
    var sim = WorldSim.Instance;
    if (sim.World == null) { return; }
    var hexSize = sim.HexSize;

    _gridView = new HexGridView { Name = "HexGridView" };
    AddChild(_gridView);
    _gridView.BuildMesh(sim.World.Grid, hexSize);

    _roadView = new RoadView { Name = "RoadView", HexSize = hexSize };
    AddChild(_roadView);

    _buildingsView = new BuildingsView { Name = "BuildingsView", HexSize = hexSize };
    AddChild(_buildingsView);

    _carrierView = new CarrierView { Name = "CarrierView", HexSize = hexSize };
    AddChild(_carrierView);

    var camera = GetNodeOrNull<Camera3D>("Camera3D");
    _picker = new HexPicker
    {
      Name = "HexPicker",
      Camera = camera,
      HexSize = hexSize,
    };
    AddChild(_picker);

    _placement = new PlacementController { Name = "PlacementController", Picker = _picker };
    AddChild(_placement);

    _roadBuild = new RoadBuildController { Name = "RoadBuildController", Picker = _picker };
    AddChild(_roadBuild);

    _hud = new DebugHud { Name = "DebugHud" };
    AddChild(_hud);
  }

}
