using System.Collections.Generic;
using System.Text;
using Godot;
using SettlersX.Core;
using SettlersX.Sim.Economy;
using SettlersX.Sim.World;

namespace SettlersX.Views.UI;

/// <summary>
/// F2a HUD: tick + pause readout, build-key legend, per-sector inventory totals
/// (sum across storehouse stocks), and a chain-failure summary that lists any
/// production building currently starved of input. Reads sim state directly per
/// the F1 precedent — view never writes back, only displays.
/// </summary>
public partial class DebugHud : CanvasLayer
{
  private Label? _label;
  private EventBus? _bus;

  public override void _Ready()
  {
    _label = new Label
    {
      Text = "tick: 0",
      Position = new Vector2(12f, 12f),
    };
    _label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
    _label.AddThemeColorOverride("font_outline_color", new Color(0f, 0f, 0f));
    _label.AddThemeConstantOverride("outline_size", 4);
    AddChild(_label);

    _bus = EventBus.Instance;
    _bus.GameTicked += OnGameTicked;
    Refresh(0);
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(_bus))
    {
      _bus!.GameTicked -= OnGameTicked;
    }
  }

  private void OnGameTicked(int tick) => Refresh(tick);

  private void Refresh(int tick)
  {
    if (_label == null) { return; }
    var sim = WorldSim.Instance;
    var paused = sim.Paused ? "yes" : "no";

    var sb = new StringBuilder();
    sb.Append("tick: ").Append(tick).Append('\n');
    sb.Append("paused: ").Append(paused).Append('\n');
    sb.Append("[1] Store  [2] Lumber  [3] Saw  [4] Site\n");

    var world = sim.World;
    if (world == null)
    {
      _label.Text = sb.ToString();
      return;
    }

    AppendSectorInventories(sb, world);
    AppendChainStatus(sb, world);
    _label.Text = sb.ToString();
  }

  private static void AppendSectorInventories(StringBuilder sb, WorldState world)
  {
    var totals = new Dictionary<int, Dictionary<ResourceKind, int>>();
    foreach (var sector in world.Sectors.Sectors)
    {
      totals[sector.Value] = new Dictionary<ResourceKind, int>();
    }

    foreach (var building in world.Buildings.All)
    {
      if (building.Stock == null) { continue; }
      var sector = world.Sectors.GetSectorOf(building.Origin);
      if (sector == null) { continue; }
      var bucket = totals[sector.Value.Value];
      foreach (var kv in building.Stock.Snapshot())
      {
        bucket[kv.Key] = bucket.GetValueOrDefault(kv.Key, 0) + kv.Value;
      }
    }

    foreach (var (sectorId, bucket) in totals)
    {
      sb.Append("sector ").Append(sectorId).Append(": ");
      if (bucket.Count == 0)
      {
        sb.Append('—');
      }
      else
      {
        var first = true;
        foreach (var kv in bucket)
        {
          if (!first) { sb.Append(' '); }
          sb.Append(kv.Key.ToString().ToLowerInvariant()).Append('=').Append(kv.Value);
          first = false;
        }
      }
      sb.Append('\n');
    }
  }

  private static void AppendChainStatus(StringBuilder sb, WorldState world)
  {
    string? blocked = null;
    foreach (var building in world.Buildings.All)
    {
      var production = building.Production;
      if (production == null) { continue; }
      if (production.IsStarved)
      {
        blocked = $"{building.DefId} at ({building.Origin.Q},{building.Origin.R})";
        break;
      }
    }
    sb.Append("chain: ").Append(blocked == null ? "OK" : "BLOCKED " + blocked);
  }
}
