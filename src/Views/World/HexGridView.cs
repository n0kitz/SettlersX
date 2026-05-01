using System;
using Godot;
using SettlersX.Sim.World;

namespace SettlersX.Views.World;

/// <summary>
/// Renders the hex grid: a flat ground plane plus hex outline lines at y ≈ 0.
/// Phase 1 placeholder — replaced by Terrain3D in F5/polish.
/// </summary>
public partial class HexGridView : Node3D
{
  private MeshInstance3D? _ground;
  private MeshInstance3D? _outlines;

  public void BuildMesh(HexGrid grid, double hexSize)
  {
    BuildGround(grid, hexSize);
    BuildOutlines(grid, hexSize);
  }

  private void BuildGround(HexGrid grid, double hexSize)
  {
    var span = (float)(Math.Max(grid.Width, grid.Height) * hexSize * 2.5);
    var plane = new PlaneMesh { Size = new Vector2(span, span) };
    var mat = new StandardMaterial3D
    {
      AlbedoColor = new Color(0.18f, 0.32f, 0.18f),
      ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
    };
    _ground = new MeshInstance3D { Name = "Ground", Mesh = plane };
    _ground.MaterialOverride = mat;
    AddChild(_ground);
  }

  private void BuildOutlines(HexGrid grid, double hexSize)
  {
    var st = new SurfaceTool();
    st.Begin(Mesh.PrimitiveType.Lines);
    var corners = new Vector3[6];
    foreach (var hex in grid.All())
    {
      var center = hex.ToWorld(hexSize);
      for (var i = 0; i < 6; i++)
      {
        var angle = i * Math.PI / 3.0;
        corners[i] = center + new Vector3(
            (float)(hexSize * Math.Cos(angle)),
            0.02f,
            (float)(hexSize * Math.Sin(angle)));
      }
      for (var i = 0; i < 6; i++)
      {
        st.AddVertex(corners[i]);
        st.AddVertex(corners[(i + 1) % 6]);
      }
    }
    var mesh = st.Commit();
    var mat = new StandardMaterial3D
    {
      AlbedoColor = new Color(0.05f, 0.05f, 0.05f),
      ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
    };
    _outlines = new MeshInstance3D { Name = "Outlines", Mesh = mesh };
    _outlines.MaterialOverride = mat;
    AddChild(_outlines);
  }
}
