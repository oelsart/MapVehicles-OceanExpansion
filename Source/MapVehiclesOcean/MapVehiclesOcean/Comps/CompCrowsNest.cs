using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class CompCrowsNest : CompScanner
{
    protected override void DoFind(Pawn worker)
    {
        var slate = new Slate();
        slate.Set("map", parent.Map);
        slate.Set("worker", worker);
        
        if (ModsConfig.OdysseyActive &&
            Find.QuestManager.ActiveQuestsListForReading.Exists(q => q.root == MVO_DefOf.MVO_TheIsland) &&
            HiddenIslandManager.HiddenIslandTileIDs(worker.Map?.Tile.Layer) is { Count: > 0 } hashSet &&
            TileFinder.TryFindTileWithDistance(worker.Tile, 1, 9, out var tile, t => hashSet.Contains(t.tileId), TileFinderMode.Near))
        {
            HiddenIslandManager.DiscoverHiddenIsland(tile);
            slate.Set("siteTile", tile);
            if (!MVO_DefOf.MVO_DesertedIsland.CanRun(slate, parent.Map))
                return;
            
            var quest = QuestUtility.GenerateQuestAndMakeAvailable(MVO_DefOf.MVO_DesertedIsland, slate);
            Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);
            return;
        }
        
        slate.Set("siteDistRange", new IntRange(1, 9));
        if (!MVO_DefOf.MVO_Shipwreck.CanRun(slate, parent.Map))
            return;
        
        var quest2 = QuestUtility.GenerateQuestAndMakeAvailable(MVO_DefOf.MVO_Shipwreck, slate);
        Find.LetterStack.ReceiveLetter(quest2.name, quest2.description, LetterDefOf.PositiveEvent, null, null, quest2);
    }
}