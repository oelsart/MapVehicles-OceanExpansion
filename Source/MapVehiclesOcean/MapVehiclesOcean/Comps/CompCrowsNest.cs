using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class CompCrowsNest : CompScanner
{
    protected new CompProperties_CrowsNest Props => (CompProperties_CrowsNest)props;
    
    protected override void DoFind(Pawn worker)
    {
        var slate = new Slate();
        slate.Set("map", parent.Map);
        slate.Set("worker", worker);
        slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(parent.Map));
        
        if (!TryFindIsland(slate))
            FindQuest(slate, Props.questWeights.RandomElementByWeight(q => q.weight).quest);
    }

    private bool TryFindIsland(Slate slate)
    {
        if (ModsConfig.OdysseyActive &&
            Find.QuestManager.ActiveQuestsListForReading.Exists(q => q.root == MVO_DefOf.MVO_TheIsland) &&
            HiddenIslandManager.HiddenIslandTileIDs(parent.Map?.Tile.Layer) is { Count: > 0 } hashSet &&
            TileFinder.TryFindTileWithDistance(parent.Tile, 1, 9, out var tile, t => hashSet.Contains(t.tileId), TileFinderMode.Near))
        {
            HiddenIslandManager.DiscoverHiddenIsland(tile);
            slate.Set("siteTile", tile);
            if (!MVO_DefOf.MVO_DesertedIsland.CanRun(slate, parent.Map))
                return false;
            
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(MVO_DefOf.MVO_DesertedIsland, slate);
            Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);
            return true;
        }

        return false;
    }

    private void FindQuest(Slate slate, QuestScriptDef quest)
    {
        slate.Set("siteDistRange", new IntRange(1, 9));
        if (!quest.CanRun(slate, parent.Map))
            return;

        var modExtensions = MVO_DefOf.MVO_Maritime.modExtensions;
        MVO_DefOf.MVO_Maritime.modExtensions = null;
        try
        {
            var quest2 = QuestUtility.GenerateQuestAndMakeAvailable(quest, slate);
            Find.LetterStack.ReceiveLetter(quest2.name, quest2.description, LetterDefOf.PositiveEvent, null, null, quest2);
        }
        finally
        {
            MVO_DefOf.MVO_Maritime.modExtensions = modExtensions;
        }
        
    }
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
        if (!DebugSettings.godMode) yield break;

        yield return new Command_Action
        {
            defaultLabel = "DEV: Find island",
            action = () =>
            {
                var slate = new Slate();
                slate.Set("map", parent.Map);
                slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.RandomElement());
                slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(parent.Map));
                TryFindIsland(slate);
            }
        };

        foreach (var questWeight in Props.questWeights)
        {
            yield return new Command_Action
            {
                defaultLabel = $"DEV: Find {questWeight.quest.defName}",
                action = () =>
                {
                    var slate = new Slate();
                    slate.Set("map", parent.Map);
                    slate.Set("worker", PawnsFinder.AllMaps_FreeColonists.RandomElement());
                    slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(parent.Map));
                    FindQuest(slate, questWeight.quest);
                }
            };
        }
    }
}