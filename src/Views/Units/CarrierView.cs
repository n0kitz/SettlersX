using System;
using System.Collections.Generic;
using Godot;
using SettlersX.Core;
using SettlersX.Sim.Economy;

namespace SettlersX.Views.Units;

/// <summary>
/// MultiMesh-based renderer for all carriers. Pulls a snapshot on every GameTicked
/// and lerps each instance between PrevWorld and NextWorld in _Process using
/// WorldSim.Alpha (ADR 0004 — never CharacterBody3D for crowds).
/// </summary>
public partial class CarrierView : Node3D
{
  private const int MaxInstances = 64;

  private MultiMesh? _multiMesh;
  private MultiMeshInstance3D? _instance;
  private EventBus? _bus;
  private IReadOnlyList<CarrierSnapshot> _snapshot = Array.Empty<CarrierSnapshot>();

  public double HexSize { get; set; } = 1.0;

  public override void _Ready()
  {
    var capsule = new CapsuleMesh { Radius = 0.15f, Height = 0.6f };
    _multiMesh = new MultiMesh
    {
      TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
      Mesh = capsule,
      InstanceCount = MaxInstances,
      VisibleInstanceCount = 0,
    };
    _instance = new MultiMeshInstance3D
    {
      Name = "Carriers",
      Multimesh = _multiMesh,
    };
    var mat = new StandardMaterial3D
    {
      AlbedoColor = new Color(0.95f, 0.85f, 0.3f),
      ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
    };
    _instance.MaterialOverride = mat;
    AddChild(_instance);

    _bus = EventBus.Instance;
    _bus.GameTicked += OnGameTicked;
  }

  public override void _ExitTree()
  {
    if (IsInstanceValid(_bus))
    {
      _bus!.GameTicked -= OnGameTicked;
    }
  }

  private void OnGameTicked(int _)
  {
    var sim = WorldSim.Instance;
    if (sim.World == null) { return; }
    _snapshot = sim.World.SnapshotCarriers(HexSize);
    if (_multiMesh == null) { return; }
    _multiMesh.VisibleInstanceCount = Math.Min(_snapshot.Count, MaxInstances);
  }

  public override void _Process(double delta)
  {
    if (_multiMesh == null || _snapshot.Count == 0) { return; }
    var alpha = (float)WorldSim.Instance.Alpha;
    var visible = Math.Min(_snapshot.Count, MaxInstances);
    var yOffset = new Vector3(0f, 0.3f, 0f);
    for (var i = 0; i < visible; i++)
    {
      var s = _snapshot[i];
      var pos = s.PrevWorld.Lerp(s.NextWorld, alpha) + yOffset;
      _multiMesh.SetInstanceTransform(i, new Transform3D(Basis.Identity, pos));
    }
  }
}
