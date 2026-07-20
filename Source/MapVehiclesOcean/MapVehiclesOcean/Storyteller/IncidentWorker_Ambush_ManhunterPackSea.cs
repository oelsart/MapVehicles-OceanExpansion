using RimWorld;
using RimWorld.Planet;
using VehicleMapFramework;
using Vehicles.World;
using Verse;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class IncidentWorker_Ambush_ManhunterPackSea : IncidentWorker_Ambush
{
  protected override bool CanFireNowSub(IncidentParms parms)
  {
    return parms.target.Tile.Tile.WaterCovered && base.CanFireNowSub(parms) &&
           AggressiveSeaAnimalIncidentUtility.TryFindAggressiveAnimalKind(parms.points, parms.target.Tile, out _);
  }

  protected override List<Pawn> GeneratePawns(IncidentParms parms)
  {
    if (!AggressiveSeaAnimalIncidentUtility.TryFindAggressiveAnimalKind(parms.points, parms.target.Tile, out var pawnKindDef))
    {
      Log.Error($"Could not find any valid animal kind for {def} incident.");
      return [];
    }
    return AggressiveAnimalIncidentUtility.GenerateAnimals(pawnKindDef, parms.target.Tile, parms.points);
  }

  protected override void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
  {
    for (var i = 0; i < generatedPawns.Count; i++)
    {
      // generatedPawns[i].health.AddHediff(HediffDefOf.Scaria);
      generatedPawns[i].mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
    }
  }

  protected override bool TryExecuteWorker(IncidentParms parms)
  {
    var map = parms.target as Map;
    var existingMapEdgeCell = IntVec3.Invalid;
    if (map is not null && !TryFindEntryCell(map, out existingMapEdgeCell))
    {
      return false;
    }
    var generatedEnemies = GeneratePawns(parms);
    if (!generatedEnemies.Any())
    {
      return false;
    }
    if (map is not null)
    {
      return DoExecute(parms, generatedEnemies, existingMapEdgeCell);
    }

    if (parms.target is VehicleCaravan)
    {
      LongEventHandler.QueueLongEvent(delegate
      {
        DoExecute(parms, generatedEnemies, existingMapEdgeCell);
      }, "GeneratingMapForNewEncounter", false, null);
      return true;
    }

    return false;
  }
  
  private bool DoExecute(IncidentParms parms, List<Pawn> generatedEnemies, IntVec3 existingMapEdgeCell)
  {
    var map = parms.target as Map;
    var flag = false;
    if (map is null)
    {
      map = VehicleCaravanIncidentUtility.SetupCaravanAttackMap((VehicleCaravan)parms.target, generatedEnemies, false,
        MVO_DefOf.MVO_AmbushSea, CaravanEnterMode.Center);
      flag = true;
    }
    else
    {
      for (var i = 0; i < generatedEnemies.Count; i++)
      {
        var intVec = CellFinder.RandomSpawnCellForPawnNear(existingMapEdgeCell, map);
        GenSpawn.Spawn(generatedEnemies[i], intVec, map, Rot4.Random);
      }
    }
    PostProcessGeneratedPawnsAfterSpawning(generatedEnemies);
    var lordJob = CreateLordJob(generatedEnemies, parms);
    if (lordJob != null)
    {
      LordMaker.MakeNewLord(parms.faction, lordJob, map, generatedEnemies);
    }
    TaggedString taggedString = GetLetterLabel(generatedEnemies[0], parms);
    TaggedString taggedString2 = GetLetterText(generatedEnemies[0], parms);
    PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(generatedEnemies, ref taggedString, ref taggedString2, GetRelatedPawnsInfoLetterText(parms), true);
    SendStandardLetter(taggedString, taggedString2, GetLetterDef(generatedEnemies[0], parms), parms, generatedEnemies[0]);
    if (flag)
    {
      Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
    }
    return true;
  }
  
  private static bool TryFindEntryCell(Map map, out IntVec3 cell)
  {
    return CellFinder.TryFindRandomEdgeCellWith(
      x => x.Standable(map) && map.reachability.CanReachColony(x),
      map, CellFinder.EdgeRoadChance_Hostile, out cell);
  }

  protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
  {
    return def.letterText.Formatted(parms.target is Caravan caravan ? caravan.Name : "yourCaravan".TranslateSimple(), anyPawn.GetKindLabelPlural()).CapitalizeFirst();
  }
}