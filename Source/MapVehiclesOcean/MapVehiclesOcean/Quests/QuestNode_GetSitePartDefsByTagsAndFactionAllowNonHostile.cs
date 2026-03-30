using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class QuestNode_GetSitePartDefsByTagsAndFactionAllowNonHostile : QuestNode
{
    public SlateRef<IEnumerable<QuestNode_GetSitePartDefsByTagsAndFaction.SitePartOption>> sitePartsTags;
    [NoTranslate] public SlateRef<string> storeAs;
    [NoTranslate] public SlateRef<string> storeFactionAs;
    public SlateRef<Thing> mustBeHostileToFactionOf;
    private static readonly List<string> tmpTags = [];

    protected override bool TestRunInt(Slate slate) => TrySetVars(slate);

    protected override void RunInt()
    {
        if (TrySetVars(QuestGen.slate))
            return;
        Log.Error("Could not resolve site parts.");
    }

    private bool TrySetVars(Slate slate)
    {
        var points = slate.Get<float>("points");
        var factionToUse = slate.Get<Faction>("enemyFaction");
        var asker = slate.Get<Pawn>("asker");
        var mustBeHostileToFactionOfResolved = mustBeHostileToFactionOf.GetValue(slate);
        for (var index1 = 0; index1 < 2; ++index1)
        {
            tmpTags.Clear();
            foreach (var sitePartOption in sitePartsTags.GetValue(slate))
            {
                if (Rand.Chance(sitePartOption.chance) && (index1 != 1 || sitePartOption.chance >= 1.0))
                    tmpTags.Add(sitePartOption.tag);
            }

            if (SiteMakerHelper.TryFindSiteParams_MultipleSiteParts(tmpTags.Where(x => x != null).Select(x =>
                    {
                        var source1 = SiteMakerHelper.SitePartDefsWithTag(x).ToList();
                        var source2 = source1.Where(y => points >= (double)y.minThreatPoints).ToList();
                        return !source2.Any() ? source1 : source2;
                    }), out var siteParts, out var faction, factionToUse, false,
                    x => (asker?.Faction == null || asker.Faction != x) &&
                         (mustBeHostileToFactionOfResolved?.Faction == null ||
                          x != mustBeHostileToFactionOfResolved.Faction &&
                          x.HostileTo(
                              mustBeHostileToFactionOfResolved.Faction))))
            {
                slate.Set(storeAs.GetValue(slate), siteParts);
                slate.Set("sitePartCount", siteParts.Count);

                if (QuestGen.Working)
                {
                    var constants = new Dictionary<string, string>();
                    for (var index2 = 0; index2 < siteParts.Count; ++index2)
                        constants[siteParts[index2].defName + "_exists"] = "True";
                    QuestGen.AddQuestDescriptionConstants(constants);
                }

                if (!storeFactionAs.GetValue(slate).NullOrEmpty())
                    slate.Set(storeFactionAs.GetValue(slate), faction);
                return true;
            }
        }

        return false;
    }
}