using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class ThoughtWorker_Ocean : ThoughtWorker_Precept
{
  protected override ThoughtState ShouldHaveThought(Pawn p)
  {
    return p.Tile.Tile is { WaterCovered: true } ? ThoughtState.ActiveAtStage(0) : ThoughtState.ActiveAtStage(1);
  }
}