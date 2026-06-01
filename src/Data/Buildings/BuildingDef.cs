using Godot;

namespace SettlersX.Data.Buildings;

/// <summary>
/// Static data for a building type. Read-only at runtime; loaded from data/buildings/*.tres
/// by ResourceDatabase. Per CLAUDE.md, no logic — only [Export] properties.
/// </summary>
[GlobalClass]
public partial class BuildingDef : Resource
{
  [Export] public string Id { get; set; } = "";
  [Export] public string DisplayName { get; set; } = "";
  [Export] public int Footprint { get; set; } = 1;
}
