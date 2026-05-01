namespace SettlersX.Sim.World.Intents;

/// <summary>
/// Marker for player/AI-issued intents drained at the Input phase of each tick.
/// Intents are immutable; rejection is silent and reported only by absence of effect
/// (no exceptions thrown in the tick loop).
/// </summary>
public interface IIntent
{
}
