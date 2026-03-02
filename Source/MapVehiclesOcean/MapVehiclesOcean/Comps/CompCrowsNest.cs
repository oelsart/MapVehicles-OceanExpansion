using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehicles;

public class CompCrowsNest : CompScanner
{
    protected override void DoFind(Pawn worker)
    {
        if (ModsConfig.OdysseyActive &&
            HiddenIslandManager.HiddenIslandTileIDs(worker.Map?.Tile.Layer) is { Count: > 0 } hashSet &&
            TileFinder.TryFindTileWithDistance(worker.Tile, 1, 9, out var tile, t => hashSet.Contains(t.tileId), TileFinderMode.Near))
        {
            HiddenIslandManager.DiscoverHiddenIsland(tile);
            return;
        }
        var slate = new Slate();
        slate.Set("map", parent.Map);
        slate.Set("worker", worker);
        slate.Set("siteDistRange", new IntRange(1, 9));
        if (!MVO_DefOf.MV_Shipwreck.CanRun(slate, parent.Map))
        {
            return;
        }
        var quest = QuestUtility.GenerateQuestAndMakeAvailable(MVO_DefOf.MV_Shipwreck, slate);
        Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest);
    }
}