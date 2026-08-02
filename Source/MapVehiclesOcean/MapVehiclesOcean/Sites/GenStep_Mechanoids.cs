using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GenStep_Mechanoids : GenStep
{
  public PawnGroupKindDef pawnGroupKindDef;

  public override int SeedPart => 89787965;

  private PawnGroupMakerParms GroupMakerParms(PlanetTile tile, Faction faction, float points, int seed)
  {
    var pawnGroupKindDef2 = pawnGroupKindDef;
    if (faction.def.pawnGroupMakers.All(maker => maker.kindDef != pawnGroupKindDef))
      pawnGroupKindDef2 = PawnGroupKindDefOf.Combat;
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
    var faction = Faction.OfMechanoids;
    // var lord = LordMaker.MakeNewLord(faction, new LordJob_MaritimeBasePawns(faction), map);
    var pawnGroupMakerSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
    var parms1 =
      GroupMakerParms(map.Tile, faction, parms.sitePart.parms.threatPoints, pawnGroupMakerSeed);
    var cellRect2 = MapGenerator.GetVar<CellRect>("SpawnRect");

    SpawnPawns();

    return;

    void SpawnPawns()
    {
      foreach (var pawn in PawnGroupMakerUtility.GeneratePawns(parms1))
      {
        if (cellRect2.TryFindRandomCell(out var cell, Validator))
        {
          Spawn(pawn, cell);
        }
      }

      return;

      bool Validator(IntVec3 cell) => cell.Standable(map) && cell.GetTerrain(map) is { dangerous: false };

      void Spawn(Pawn pawn, IntVec3 cell)
      {
        GenSpawn.Spawn(pawn, cell, map);
        // lord.AddPawn(pawn);
      }
    }
  }
}