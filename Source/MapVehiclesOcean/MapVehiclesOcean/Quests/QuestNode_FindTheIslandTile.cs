using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class QuestNode_FindTheIslandTile : QuestNode
{
    private const int MinTraversalDistance = 180;
    private const int MaxTraversalDistance = 800;
    [NoTranslate]
    public SlateRef<string> storeAs;

    private bool TryFindRootTile(out PlanetTile tile)
    {
        return TileFinder.TryFindRandomPlayerTile(out tile, false, x => TryFindDestinationTileActual(x, 180, out _));
    }

    private static bool TryFindDestinationTile(PlanetTile rootTile, out PlanetTile tile)
    {
        var minDist = MaxTraversalDistance;
        for (var index = 0; index < 1000; ++index)
        {
            minDist = (int) (minDist * (double) Rand.Range(0.5f, 0.75f));
            if (minDist <= MinTraversalDistance)
                minDist = MinTraversalDistance;
            if (TryFindDestinationTileActual(rootTile, minDist, out tile))
                return true;
            if (minDist <= MinTraversalDistance)
                return false;
        }
        tile = PlanetTile.Invalid;
        return false;
    }

    private static bool TryFindDestinationTileActual(PlanetTile rootTile, int minDist, out PlanetTile tile)
    {
        if (TileFinderSea.TryFindNewSiteTile(out tile, rootTile, minDist, MaxTraversalDistance, tileFinderMode: TileFinderMode.Random,
                validator: x => !Find.WorldObjects.AnyWorldObjectAt(x) && Find.WorldGrid[x].PrimaryBiome.canAutoChoose)) 
            return true;
        tile = PlanetTile.Invalid;
        return false;
    }

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        TryFindRootTile(out var tile1);
        TryFindDestinationTile(tile1, out var tile2);
        slate.Set(storeAs.GetValue(slate), tile2);
    }

    protected override bool TestRunInt(Slate slate)
    {
        return TryFindRootTile(out var tile) && TryFindDestinationTile(tile, out _);
    }
}