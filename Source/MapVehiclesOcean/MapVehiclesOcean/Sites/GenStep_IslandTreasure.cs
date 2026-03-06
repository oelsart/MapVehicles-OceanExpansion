using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class GenStep_IslandTreasure : GenStep
{
    public override int SeedPart => 848961748;

    public override void Generate(Map map, GenStepParams parms)
    {
        var elevation = MapGenerator.Elevation;
        var top = map.AllCells.MaxBy(c => elevation[c]);
        var rot = Rot4.Random;
        if (RCellFinder.TryFindRandomCellNearWith(top,
                c => c.Roofed(map) && GenSpawn.CanSpawnAt(MVO_DefOf.MVO_StoneCoffer, c, map, rot, false), map,
                out var cell))
        {
            var coffer = (Building_Crate)ThingMaker.MakeThing(MVO_DefOf.MVO_StoneCoffer, GenStuff.RandomStuffFor(MVO_DefOf.MVO_StoneCoffer));
            GenSpawn.Spawn(coffer, cell, map, rot);
            coffer.GetDirectlyHeldThings().TryAddRangeOrTransfer(parms.sitePart.things);
        }
    }
}