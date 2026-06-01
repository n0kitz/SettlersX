using System;
using System.Collections.Generic;
using GdUnit4;
using SettlersX.Sim.World;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Sim.World;

[TestSuite]
public class SectorGraphTests
{
  [TestCase]
  public void AddSector_NewSector_RegistersOwnerAndHexes()
  {
    var graph = new SectorGraph();
    var hexes = new List<HexCoord> { new(0, 0), new(1, 0), new(0, 1) };
    graph.AddSector(new SectorId(0), ownerPlayerId: 0, hexes);
    AssertThat(graph.Count).IsEqual(1);
    AssertThat(graph.GetOwner(new SectorId(0))).IsEqual(0);
    AssertThat(graph.HexesOf(new SectorId(0)).Count).IsEqual(3);
  }

  [TestCase]
  public void AddSector_DuplicateId_Throws()
  {
    var graph = new SectorGraph();
    graph.AddSector(new SectorId(0), 0, new[] { new HexCoord(0, 0) });
    AssertThrown(() => graph.AddSector(new SectorId(0), 0, new[] { new HexCoord(1, 0) }))
        .IsInstanceOf<ArgumentException>();
  }

  [TestCase]
  public void AddSector_OverlappingHex_Throws()
  {
    var graph = new SectorGraph();
    graph.AddSector(new SectorId(0), 0, new[] { new HexCoord(0, 0) });
    AssertThrown(() =>
        graph.AddSector(new SectorId(1), 0, new[] { new HexCoord(0, 0) }))
        .IsInstanceOf<ArgumentException>();
  }

  [TestCase]
  public void GetSectorOf_KnownHex_ReturnsId()
  {
    var graph = new SectorGraph();
    graph.AddSector(new SectorId(7), 0, new[] { new HexCoord(2, 3) });
    AssertThat(graph.GetSectorOf(new HexCoord(2, 3)) == new SectorId(7)).IsTrue();
  }

  [TestCase]
  public void GetSectorOf_UnknownHex_ReturnsNull()
  {
    var graph = new SectorGraph();
    graph.AddSector(new SectorId(0), 0, new[] { new HexCoord(0, 0) });
    AssertThat(graph.GetSectorOf(new HexCoord(99, 99)).HasValue).IsFalse();
  }

  [TestCase]
  public void GetOwner_UnknownSector_Throws()
  {
    var graph = new SectorGraph();
    AssertThrown(() => graph.GetOwner(new SectorId(99)))
        .IsInstanceOf<KeyNotFoundException>();
  }
}
