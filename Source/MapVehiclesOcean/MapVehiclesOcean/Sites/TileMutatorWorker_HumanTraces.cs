using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MapVehicles;

public class TileMutatorWorker_HumanTraces(TileMutatorDef def) : TileMutatorWorker(def)
{
    private static readonly IntRange CorpseAgeRangeDays = new (30, 6000);
    
    public override void GeneratePostFog(Map map)
    {
        base.GeneratePostFog(map);
        if (!TryFindShelterCell(map, out var cell))
            return;
        SpawnCorpse(map, cell);

        if (CellFinder.TryFindRandomCellNear(cell, map, 8, c =>
                    !c.Fogged(map) && GenSpawn.CanSpawnAt(ThingDefOf.Campfire, c, map) && c.GetRoof(map) is not null,
                out var cell2))
        {
            var campfire = GenSpawn.Spawn(ThingDefOf.Campfire, cell2, map);
            if (campfire.TryGetComp<CompRefuelable>(out var comp))
                comp.ConsumeFuel(comp.Fuel);
        }

        var rot = Rot4.Random;
        if (CellFinder.TryFindRandomCellNear(cell, map, 8, c =>
                    !c.Fogged(map) && GenSpawn.CanSpawnAt(ThingDefOf.Bedroll, c, map, rot) && c.GetRoof(map) is not null,
                out var cell3))
        {
            var bedroll = ThingMaker.MakeThing(ThingDefOf.Bedroll, GenStuff.RandomStuffFor(ThingDefOf.Bedroll));
            GenSpawn.Spawn(bedroll, cell3, map, rot);
        }
        
        return;
        
        static bool TryFindShelterCell(Map map, out IntVec3 result)
        {
            return CellFinder.TryFindRandomCell(map,
                c => c.Standable(map) && !c.Fogged(map) && c.GetRoof(map) is not null,
                out result);
        }

        static void SpawnCorpse(Map map, IntVec3 cell)
        {
            var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Drifter);
            if (!CellFinder.TryFindRandomSpawnCellForPawnNear(cell, map, out var cell2))
                return;
            pawn.health.SetDead();
            pawn.apparel.DestroyAll();
            pawn.equipment.DestroyAllEquipment();
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            var corpse = pawn.MakeCorpse(null, null);
            corpse.Age = Mathf.RoundToInt(CorpseAgeRangeDays.RandomInRange * 60000);
            corpse.GetComp<CompRottable>().RotProgress += corpse.Age;
            GenSpawn.Spawn(pawn.Corpse, cell2, map);
        }
    }
}