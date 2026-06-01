using System;
using Godot;

namespace SettlersX.Sim.World;

/// <summary>
/// Axial hex coordinate (flat-top). Q is column-axis, R is row-axis.
/// World mapping uses double precision. Vector3 is allow-listed in the Sim layer
/// only as a math struct, not as a Godot scene-tree reference.
/// </summary>
public readonly record struct HexCoord(int Q, int R)
{
  private static readonly double Sqrt3 = Math.Sqrt(3.0);

  private static readonly (int dq, int dr)[] _neighborOffsets = new (int, int)[]
  {
    (+1, 0), (+1, -1), (0, -1), (-1, 0), (-1, +1), (0, +1),
  };

  public HexCoord[] Neighbors()
  {
    var result = new HexCoord[6];
    for (var i = 0; i < 6; i++)
    {
      var (dq, dr) = _neighborOffsets[i];
      result[i] = new HexCoord(Q + dq, R + dr);
    }
    return result;
  }

  public int Distance(HexCoord other)
  {
    var dq = Q - other.Q;
    var dr = R - other.R;
    var ds = -dq - dr;
    return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
  }

  public Vector3 ToWorld(double size)
  {
    var x = size * 1.5 * Q;
    var z = size * (Sqrt3 * 0.5 * Q + Sqrt3 * R);
    return new Vector3((float)x, 0f, (float)z);
  }

  public static HexCoord FromWorld(Vector3 world, double size)
  {
    var qFrac = (2.0 / 3.0 * world.X) / size;
    var rFrac = (-1.0 / 3.0 * world.X + Sqrt3 / 3.0 * world.Z) / size;
    return Round(qFrac, rFrac);
  }

  private static HexCoord Round(double qFrac, double rFrac)
  {
    var sFrac = -qFrac - rFrac;
    var rq = (int)Math.Round(qFrac);
    var rr = (int)Math.Round(rFrac);
    var rs = (int)Math.Round(sFrac);
    var dq = Math.Abs(rq - qFrac);
    var dr = Math.Abs(rr - rFrac);
    var ds = Math.Abs(rs - sFrac);
    if (dq > dr && dq > ds)
    {
      rq = -rr - rs;
    }
    else if (dr > ds)
    {
      rr = -rq - rs;
    }
    return new HexCoord(rq, rr);
  }
}
