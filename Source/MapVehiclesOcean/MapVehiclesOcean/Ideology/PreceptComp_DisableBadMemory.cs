using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public abstract class PreceptComp_DisableBadMemory : PreceptComp
{
  protected abstract ThoughtDef ThoughtDef { get; }
  
  public override void Notify_AddBedThoughts(Pawn pawn, Precept precept)
  {
    var memories = pawn.needs.mood.thoughts.memories;
    if (memories.GetFirstMemoryOfDef(ThoughtDef) is { moodOffset: < 0 })
      memories.RemoveMemoriesOfDef(ThoughtDef);
  }
}