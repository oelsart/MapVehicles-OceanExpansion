using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class GenStep_HumanTraces : GenStep
{
    public override int SeedPart => 169876513;

    private static readonly IntRange CorpseAgeRangeDays = new (30, 6000);

    public override void Generate(Map map, GenStepParams parms)
    {
        if (!TryFindShelterCell(map, out var cell))
            return;

        var pawns = parms.sitePart.things.OfType<Pawn>().ToList();
        foreach (var pawn in pawns)
            SpawnPawn(pawn, map, cell);

        if (CellFinder.TryFindRandomCellNear(cell, map, 4, c =>
                    !c.Fogged(map) && GenSpawn.CanSpawnAt(ThingDefOf.Campfire, c, map) && c.GetRoof(map) is not null,
                out var cell2))
        {
            var campfire = GenSpawn.Spawn(ThingDefOf.Campfire, cell2, map);
            if (campfire.TryGetComp<CompRefuelable>(out var comp))
                comp.ConsumeFuel(comp.Fuel);
        }

        var rot = Rot4.Random;
        if (CellFinder.TryFindRandomCellNear(cell, map, 4, c =>
                    !c.Fogged(map) && GenSpawn.CanSpawnAt(ThingDefOf.Bedroll, c, map, rot) && c.GetRoof(map) is not null,
                out var cell3))
        {
            var bedroll = ThingMaker.MakeThing(ThingDefOf.Bedroll, GenStuff.RandomStuffFor(ThingDefOf.Bedroll));
            GenSpawn.Spawn(bedroll, cell3, map, rot);
        }
        
        if (CellFinder.TryFindRandomCellNear(cell, map, 4, c =>
                    !c.Fogged(map) && GenSpawn.CanSpawnAt(MVO_DefOf.MVO_Volleyball, c, map, Rot4.North),
                out var cell4))
        {
            GenSpawn.Spawn(MVO_DefOf.MVO_Volleyball, cell4, map, Rot4.North);
        }
        
        if (CellFinder.TryFindRandomCellNear(cell, map, 4, c =>
                    !c.Fogged(map) && GenSpawn.CanSpawnAt(MVO_DefOf.MVO_Filth_TallyMarks, c, map, Rot4.North) && c.GetRoof(map) is not null,
                out var cell5))
        {
            GenSpawn.Spawn(MVO_DefOf.MVO_Filth_TallyMarks, cell5, map, Rot4.North);
        }

        if (ModsConfig.BiotechActive)
        {
            for (var i = 0; i < Rand.Range(0, 2); i++)
            {
                if (CellFinder.TryFindRandomCellNear(cell, map, 4, c =>
                            !c.Fogged(map) && GenSpawn.CanSpawnAt(ThingDefOf.Filth_Floordrawing, c, map, Rot4.North) && c.GetRoof(map) is not null,
                        out var cell6))
                {
                    GenSpawn.Spawn(ThingDefOf.Filth_Floordrawing, cell6, map, Rot4.North);
                }
            }
        }
        
        return;
        
        static bool TryFindShelterCell(Map map, out IntVec3 result)
        {
            return CellFinder.TryFindRandomCell(map,
                c => c.Standable(map) && !c.Fogged(map) && c.GetRoof(map) is not null,
                out result);
        }

        static void SpawnPawn(Pawn pawn, Map map, IntVec3 cell)
        {
            if (!CellFinder.TryFindRandomSpawnCellForPawnNear(cell, map, out var cell2))
                return;
            if (pawn.Dead)
                GenSpawn.Spawn(pawn.Corpse, cell2, map);
            else
            {
                pawn.mindState.WillJoinColonyIfRescued = true;
                GenSpawn.Spawn(pawn, cell2, map);
            }
        }
    }
}