using System;
using System.Collections.Generic;

namespace SettlersX.Sim.World;

/// <summary>
/// Rectangular axial-bounded hex grid: Q in [0, Width), R in [0, Height).
/// Owns nothing but bounds; hex content is stored elsewhere (BuildingRegistry, RoadGraph).
/// </summary>
public class HexGrid
{
  public int Width { get; }
  public int Height { get; }

  public HexGrid(int width, int height)
  {
    if (width <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(width), $"width must be > 0, got {width}");
    }
    if (height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(height), $"height must be > 0, got {height}");
    }
    Width = width;
    Height = height;
  }

  public bool InBounds(HexCoord c) =>
      c.Q >= 0 && c.Q < Width && c.R >= 0 && c.R < Height;

  public IEnumerable<HexCoord> All()
  {
    for (var q = 0; q < Width; q++)
    {
      for (var r = 0; r < Height; r++)
      {
        yield return new HexCoord(q, r);
      }
    }
  }
}
