using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class GenStep_MaritimeBasePawns : GenStep
{
    public FactionDef factionDef;
    public PawnGroupKindDef pawnGroupKindDef;
    private const float FixedPoints = 400f;

    public override int SeedPart => 178914836;

    private PawnGroupMakerParms GroupMakerParms(PlanetTile tile, Faction faction, float points, int seed)
    {
        var pawnGroupKindDef2 = pawnGroupKindDef;
        if (faction.def.pawnGroupMakers.All(
                maker => maker.kindDef != pawnGroupKindDef))
            pawnGroupKindDef2 = PawnGroupKindDefOf.Settlement;
        var parms = new PawnGroupMakerParms
        {
            groupKind = pawnGroupKindDef2,
            tile = tile,
            faction = faction,
            inhabitants = true,
            generateFightersOnly = true,
            seed = seed,
            points = points
        };
        return parms;
    }

    public override void Generate(Map map, GenStepParams parms)
    {
        var faction = Find.FactionManager.FirstFactionOfDef(factionDef);
        var lord = LordMaker.MakeNewLord(faction, new LordJob_MaritimeBasePawns(faction), map);
        var pawnGroupMakerSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
        var parms1 =
            GroupMakerParms(map.Tile, faction, FixedPoints, pawnGroupMakerSeed);
        var cellRect2 = MapGenerator.GetVar<CellRect>("SpawnRect");

        for (var i = 0; i < 3; i++)
        {
            SpawnPawns();
        }
        return;
        
        void SpawnPawns()
        {
            foreach (var pawn in PawnGroupMakerUtility.GeneratePawns(parms1))
            {
                if (pawn.RaceProps.Animal && cellRect2.TryFindRandomCell(out var cell, ValidatorAnimal))
                {
                    Spawn(pawn, cell);
                    continue;
                }
                if (cellRect2.TryFindRandomCell(out cell, Validator))
                {
                    Spawn(pawn, cell);
                }
            }
            return;

            bool Validator(IntVec3 cell) => cell.Standable(map) && cell.Roofed(map);
            bool ValidatorAnimal(IntVec3 cell) => Validator(cell) && cell.GetTerrain(map)?.defName == "StrawMatting";
            void Spawn(Pawn pawn, IntVec3 cell)
            {
                GenSpawn.Spawn(pawn, cell, map);
                lord.AddPawn(pawn);
            }
        } 
    }
}