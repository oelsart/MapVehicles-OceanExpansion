using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace MapVehicles;

public class SitePartWorker_Shipwreck : SitePartWorker
{
    public override void Init(Site site, SitePart sitePart)
    {
        base.Init(site, sitePart);
        var makerDef = MVO_DefOf.MV_ShipwreckCargo;
        ThingSetMakerParams setParams = default;
        setParams.totalMarketValueRange = new FloatRange(8000f, 16000f);
        
        var generatedThings = makerDef.root.Generate(setParams);
        sitePart.things ??= new ThingOwner<Thing>(sitePart);
        sitePart.things.TryAddRangeOrTransfer(generatedThings);
    }

    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return site.MainSitePartDef == def ? null : base.GetPostProcessedThreatLabel(site, sitePart);
    }

    public override void Notify_GeneratedByQuestGen(SitePart part, Slate slate, List<Rule> outExtraDescriptionRules, Dictionary<string, string> outExtraDescriptionConstants)
    {
        base.Notify_GeneratedByQuestGen(part, slate, outExtraDescriptionRules, outExtraDescriptionConstants);
        if (part.site.ActualThreatPoints > 0f)
        {
            outExtraDescriptionRules.Add(new Rule_String("MV_ShipwreckThreatDescription", "\n\n" + "MV_ShipwreckHostileThreat".Translate()));
            return;
        }
        outExtraDescriptionRules.Add(new Rule_String("MV_ShipwreckThreatDescription", ""));
    }
}