using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace MapVehiclesOcean;

public class SitePartWorker_GeneratedPawns : SitePartWorker
{
    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules,
        Dictionary<string, string> outExtraDescriptionConstants)
    {
        base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        
        part.things ??= new ThingOwner<Thing>(part);
        if (slate.TryGet<Pawn>("generatedPawn", out var pawn))
            part.things.TryAddOrTransfer(pawn);
        if (slate.TryGet<List<Pawn>>("generatedPawns", out var pawns))
            part.things.TryAddRangeOrTransfer(pawns);
    }
}