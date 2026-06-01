namespace SettlersX.Sim.World.Intents;

public sealed record BuildRoadIntent(HexCoord A, HexCoord B) : IIntent;
