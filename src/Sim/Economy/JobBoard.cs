using SettlersX.Sim.World;

namespace SettlersX.Sim.Economy;

/// <summary>
/// Stateless dispatcher. TransportSystem asks the board for a job per idle carrier;
/// the board scans the world, reserves the resource at the source (decrements
/// Output or Stock), and returns a <see cref="Job"/>. Reservation prevents two
/// carriers being sent for the same stack on the same tick.
///
/// F2a scope: storehouse is the central hub. Producers push outputs to the
/// nearest storehouse; consumers and construction sites pull from any storehouse
/// holding the resource. Picking is by hex distance — proper road-graph distance
/// arrives in F2b once multi-sector routing matters.
/// </summary>
public static class JobBoard
{
  /// <summary>
  /// Try to find a job for the carrier at <paramref name="carrierHex"/>. On success
  /// the source's stock is decremented before returning, so the job is valid even
  /// if subsequent ticks empty the source.
  /// </summary>
  public static bool TryFindJob(WorldState state, HexCoord carrierHex, out Job job)
  {
    if (TryFindPushJob(state, carrierHex, out job)) { return true; }
    if (TryFindPullToConstructionJob(state, carrierHex, out job)) { return true; }
    if (TryFindPullToInputJob(state, carrierHex, out job)) { return true; }
    job = default;
    return false;
  }

  /// <summary>
  /// Restore the reservation made by <see cref="TryFindJob"/> when the carrier
  /// cannot complete the trip (e.g. no road connection between source and dest).
  /// </summary>
  public static void Refund(Job job)
  {
    switch (job.Kind)
    {
      case JobKind.PushToStock:
        job.Source.Production?.Output.Add(job.Stack.Kind, job.Stack.Count);
        break;
      case JobKind.PullToInput:
      case JobKind.PullToConstruction:
        job.Source.Stock?.Add(job.Stack.Kind, job.Stack.Count);
        break;
    }
  }

  private static bool TryFindPushJob(WorldState state, HexCoord carrierHex, out Job job)
  {
    Building? bestProducer = null;
    Building? bestStorehouse = null;
    var bestKind = ResourceKind.None;
    var bestDistance = int.MaxValue;

    foreach (var producer in state.Buildings.All)
    {
      if (producer.Production == null) { continue; }
      var outKind = producer.Production.Recipe.Output.Kind;
      if (producer.Production.Output.Get(outKind) <= 0) { continue; }

      var storehouse = NearestStorehouse(state, producer.Origin);
      if (storehouse == null) { continue; }

      var distance = carrierHex.Distance(producer.Origin)
          + producer.Origin.Distance(storehouse.Origin);
      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestProducer = producer;
        bestStorehouse = storehouse;
        bestKind = outKind;
      }
    }

    if (bestProducer == null || bestStorehouse == null)
    {
      job = default;
      return false;
    }

    var stack = new ResourceStack(bestKind, 1);
    if (!bestProducer.Production!.Output.TryRemove(stack.Kind, stack.Count))
    {
      job = default;
      return false;
    }
    job = new Job(bestProducer, bestStorehouse, stack, JobKind.PushToStock);
    return true;
  }

  private static bool TryFindPullToConstructionJob(
      WorldState state, HexCoord carrierHex, out Job job)
  {
    Building? bestSite = null;
    Building? bestStorehouse = null;
    var bestKind = ResourceKind.None;
    var bestDistance = int.MaxValue;

    foreach (var site in state.Buildings.All)
    {
      var construction = site.Construction;
      if (construction == null || construction.IsComplete) { continue; }
      if (construction.Outstanding <= 0) { continue; }

      var kind = construction.Cost.Kind;
      var storehouse = NearestStorehouseWithStock(state, site.Origin, kind);
      if (storehouse == null) { continue; }

      var distance = carrierHex.Distance(storehouse.Origin)
          + storehouse.Origin.Distance(site.Origin);
      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestSite = site;
        bestStorehouse = storehouse;
        bestKind = kind;
      }
    }

    if (bestSite == null || bestStorehouse == null)
    {
      job = default;
      return false;
    }

    var stack = new ResourceStack(bestKind, 1);
    if (!bestStorehouse.Stock!.TryRemove(stack.Kind, stack.Count))
    {
      job = default;
      return false;
    }
    job = new Job(bestStorehouse, bestSite, stack, JobKind.PullToConstruction);
    return true;
  }

  private static bool TryFindPullToInputJob(
      WorldState state, HexCoord carrierHex, out Job job)
  {
    Building? bestConsumer = null;
    Building? bestStorehouse = null;
    var bestKind = ResourceKind.None;
    var bestDistance = int.MaxValue;

    foreach (var consumer in state.Buildings.All)
    {
      var production = consumer.Production;
      if (production == null) { continue; }
      if (!production.Recipe.HasInput) { continue; }
      var inKind = production.Recipe.Input.Kind;
      if (production.Input.Get(inKind) >= production.InputCapacity) { continue; }

      var storehouse = NearestStorehouseWithStock(state, consumer.Origin, inKind);
      if (storehouse == null) { continue; }

      var distance = carrierHex.Distance(storehouse.Origin)
          + storehouse.Origin.Distance(consumer.Origin);
      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestConsumer = consumer;
        bestStorehouse = storehouse;
        bestKind = inKind;
      }
    }

    if (bestConsumer == null || bestStorehouse == null)
    {
      job = default;
      return false;
    }

    var stack = new ResourceStack(bestKind, 1);
    if (!bestStorehouse.Stock!.TryRemove(stack.Kind, stack.Count))
    {
      job = default;
      return false;
    }
    job = new Job(bestStorehouse, bestConsumer, stack, JobKind.PullToInput);
    return true;
  }

  private static Building? NearestStorehouse(WorldState state, HexCoord from)
  {
    Building? best = null;
    var bestDistance = int.MaxValue;
    foreach (var b in state.Buildings.All)
    {
      if (b.Kind != BuildingKind.Storehouse || b.Stock == null) { continue; }
      var d = from.Distance(b.Origin);
      if (d < bestDistance) { bestDistance = d; best = b; }
    }
    return best;
  }

  private static Building? NearestStorehouseWithStock(
      WorldState state, HexCoord from, ResourceKind kind)
  {
    Building? best = null;
    var bestDistance = int.MaxValue;
    foreach (var b in state.Buildings.All)
    {
      if (b.Kind != BuildingKind.Storehouse || b.Stock == null) { continue; }
      if (b.Stock.Get(kind) <= 0) { continue; }
      var d = from.Distance(b.Origin);
      if (d < bestDistance) { bestDistance = d; best = b; }
    }
    return best;
  }
}
