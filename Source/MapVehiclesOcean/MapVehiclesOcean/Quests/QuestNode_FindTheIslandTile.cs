using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class QuestNode_FindTheIslandTile : QuestNode
{
  private const int MinTraversalDistance = 100;
  private const int MaxTraversalDistance = 200;
  [NoTranslate] public SlateRef<string> storeAs;
  
  protected override void RunInt()
  {
    var slate = QuestGen.slate;
    if (TryFindIslandTile(out var tile))
	    slate.Set(storeAs.GetValue(slate), tile);
  }

  protected override bool TestRunInt(Slate slate)
  {
	  return TryFindIslandTile(out _);
  }

  private static bool TryFindIslandTile(out PlanetTile tile)
  {
	  List<PlanetTile> neighbors = [];
	  tile = TileFinderSea.RandomSettlementTileFor(Find.WorldGrid.Surface, null, tile =>
	  {
		  if (Find.WorldObjects.AnyWorldObjectAt(tile)) return false;
		  Find.WorldGrid.GetTileNeighbors(tile, neighbors);
		  foreach (var neighbor in neighbors)
		  {
			  if (Find.WorldObjects.AnyWorldObjectAt(neighbor) ||
			      !neighbor.Tile.WaterCovered) return false;
		  }
		  return true;
	  });
	  return tile.Valid;
  }
}