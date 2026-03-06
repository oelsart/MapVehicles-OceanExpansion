using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace MapVehiclesOcean;

public class SitePartWorker_GeneratedThings : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return site.MainSitePartDef == def ? null : base.GetPostProcessedThreatLabel(site, sitePart);
    }
    
    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules,
        Dictionary<string, string> outExtraDescriptionConstants)
    {
        base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        outExtraDescriptionRules.Add(part.site.ActualThreatPoints > 0f
            ? new Rule_String("MVO_ShipwreckThreatDescription", "\n\n" + "MVO_ShipwreckHostileThreat".Translate())
            : new Rule_String("MVO_ShipwreckThreatDescription", ""));
        part.things ??= new ThingOwner<Thing>(part);
        if (slate.TryGet<List<Thing>>("generatedThings", out var list))
            part.things.TryAddRangeOrTransfer(list);
    }
}