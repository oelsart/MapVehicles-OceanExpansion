using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class ScenPart_ForcedSeaMap : ScenPart_ForcedMap // 開始タイル選択をスキップするためScenPart_ForcedMapを継承する必要がある
{
  public override void DoEditInterface(Listing_ScenEdit listing)
  {
  }

  public override void PostWorldGenerate()
  {
    Find.GameInitData.startingTile = TileFinderSea.RandomSeaTile();
    Find.GameInitData.mapGeneratorDef = MVO_DefOf.MVO_MapGeneratorSea;
  }
}