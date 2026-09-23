using System.Runtime.CompilerServices;
using HarmonyLib;
using VehicleMapFramework;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean.HarmonyPatches;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetPathContext))]
public static class Patch_Pawn_GetPathContext
{
  public static void Postfix(Pawn __instance, Pathing pathing, ref PathingContext __result)
  {
    if (AquaticCreature.IsAquatic(__instance))
      __result = pathing.Get(MVO_DefOf.MVO_Aquatic);
  }
}

[HarmonyPatch(typeof(Pathing), nameof(Pathing.For), typeof(TraverseParms))]
public static class Patch_Pathing_For
{
  public static void Postfix(Pathing __instance, TraverseParms parms, ref PathingContext __result)
  {
    if (parms.pawn is { } pawn && AquaticCreature.IsAquatic(pawn))
      __result = __instance.Get(MVO_DefOf.MVO_Aquatic);
  }
}

[HarmonyPatch(typeof(Reachability), nameof(Reachability.CanReach),
  typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms))]
public static class Patch_Reachability_CanReach
{
  public static void Postfix(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode,
    TraverseParms traverseParams, Map ___map, ref bool __result)
  {
    if (traverseParams.pawn is { } pawn && AquaticCreature.IsAquatic(pawn))
    {
      __result = __result && ForceCheckCellBasedReachability(___map, start, dest, peMode, traverseParams);
    }
  }

  private static bool ForceCheckCellBasedReachability(Map map, IntVec3 start, LocalTargetInfo dest,
    PathEndMode peMode, TraverseParms traverseParams)
  {
    var result = false;
    var directRegionGrid = map.regionGrid.DirectGrid;
    var pathGrid = map.pathing.For(traverseParams).pathGrid;
    var cellIndices = map.cellIndices;
    
    map.floodFiller.FloodFill(start, c =>
    {
      var num = cellIndices.CellToIndex(c);
      if (!pathGrid.WalkableFast(num))
      {
        return false;
      }

      var region = directRegionGrid[num];
      return region == null || region.Allows(traverseParams, false);
    }, c =>
    {
      if (ReachabilityImmediate.CanReachImmediate(c, dest, map, peMode, traverseParams.pawn))
      {
        result = true;
        return true;
      }

      return false;
    });
    return result;
  }
}

[HarmonyPatch(typeof(Region), nameof(Region.Allows))]
public static class Patch_Region_Allows
{
  public static void Postfix(Region __instance, TraverseParms tp, ref bool __result)
  {
    if (!__result) return;
    if (__instance.Map.IsVehicleMap &&
        tp.pawn is { } pawn && AquaticCreature.IsAquatic(pawn))
      __result = false;
  }
}

[HarmonyPatch(typeof(PathFinderMapData), nameof(PathFinderMapData.ParameterizeGridJob))]
public static class Patch_PathFinderMapData_ParameterizeGridJob
{
	private static readonly ConditionalWeakTable<Map, CostSource> waterCostTable = [];
	
	public static void Postfix(PathRequest request, Map ___map, ref PathGridJob job)
	{
		if (request.pawn is not null && AquaticCreature.IsAquatic(request.pawn))
		{
			var waterCost = waterCostTable.GetValue(___map, m =>
			{
				var source = new CostSource(m, m.pathing.Get(MVO_DefOf.MVO_Aquatic));
				m.pathFinder.MapData.RegisterSource(source);
				source.ComputeAll(null);
				return source;
			});
			job.pathGridDirect = waterCost.Data;
		}
	}
}