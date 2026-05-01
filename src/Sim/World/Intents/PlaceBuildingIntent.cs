namespace SettlersX.Sim.World.Intents;

public sealed record PlaceBuildingIntent(string DefId, HexCoord Cell, int OwnerPlayerId) : IIntent;
