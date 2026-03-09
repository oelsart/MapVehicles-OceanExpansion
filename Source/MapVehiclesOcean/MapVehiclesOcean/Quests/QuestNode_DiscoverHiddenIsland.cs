using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class QuestNode_DiscoverHiddenIsland : QuestNode
{
    [NoTranslate]
    public SlateRef<PlanetTile> tile;

    protected override bool TestRunInt(Slate slate)
    {
        return true;
    }

    protected override void RunInt()
    {
        if (tile.TryGetValue(QuestGen.slate, out var tile1))
            HiddenIslandManager.DiscoverHiddenIsland(tile1);
    }
}