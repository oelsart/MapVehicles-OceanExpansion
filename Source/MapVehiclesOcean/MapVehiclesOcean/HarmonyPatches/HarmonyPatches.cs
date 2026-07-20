using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using UnityEngine;
using VehicleMapFramework;
using Vehicles;
using Vehicles.World;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean.HarmonyPatches;

[HarmonyPatch(typeof(WaterBodyTracker), nameof(WaterBodyTracker.Notify_Fished))]
public static class Patch_WaterBodyTracker_Notify_Fished
{
  public static void Postfix(IntVec3 c, float amount, Map ___map)
  {
    if (___map.IsVehicleMapOf(out var vehicle) && vehicle.Spawned)
      c.ToBaseMapCoord(vehicle).GetWaterBody(vehicle.Map)?.Population -= amount;
  }
}

// 島タイルでCampした時用
[HarmonyPatch(typeof(EnterMapUtilityVehicles), nameof(EnterMapUtilityVehicles.EnterMap))]
public static class Patch_EnterMapUtilityVehicles_EnterMap
{
  public static void Prefix(Map map, ref EnterMapUtilityVehicles.SpawnParams spawnParams)
  {
    if (map?.Tile.Tile?.Landmark?.def == MVO_DefOf.MVO_OceanIsland)
    {
      spawnParams.enterMode = CaravanEnterMode.Edge;
    }
  }
}

// SCOSからのパッチだとインライン化の可能性あり
[HarmonyPatch(typeof(Pawn), nameof(Pawn.Swimming), MethodType.Getter)]
public static class Patch_Pawn_Swimming
{
  public static void Postfix(Pawn __instance, ref bool __result)
  {
    if (__result) return;
    if (__instance.Spawned)
    {
      var terrain = __instance.Position.GetTerrain(__instance.Map);
      __result = terrain == MVO_DefOf.MVO_WaterDeepPassable || terrain == MVO_DefOf.MVO_WaterOceanDeepPassable;
    }
  }
}

// VFのレイダーパッチにより車両の端でExit判定を行えるようになっているが、IsExitCellはMapUsesExitGridチェックを使用するため
// 非プレイヤー派閥のチェックに向かない。マップ端からの距離による簡易チェックを行う
[HarmonyPatch(typeof(JobDriver_Goto), "MakeNewToils")]
public static class Patch_JobDriver_Goto_MakeNewToils
{
  public static IEnumerable<Toil> Postfix(IEnumerable<Toil> values, JobDriver_Goto __instance)
  {
    foreach (var toil in values) yield return toil;
    yield return Toils_General.Do(() =>
    {
      if (__instance is { pawn: VehiclePawn { Spawned: true} vehicle, job.exitMapOnArrival: true })
      {
        var vehicleDef = vehicle.VehicleDef;
        var largestSize = Mathf.Max(vehicleDef.Size.x, vehicleDef.Size.z);
        var even = largestSize % 2 == 0;
        var padding = Mathf.CeilToInt(largestSize / 2f);
        if (even) padding++;
        var rot = CellRect.WholeMap(vehicle.Map).GetClosestEdge(vehicle.Position);
        if (vehicle.PawnOccupiedCells(vehicle.Position, rot).Corners.Any(cell =>
              cell.CloseToEdge(vehicle.Map, padding)))
        {
          PathingHelper.ExitMapForVehicle(vehicle, __instance.job);
        }
      }
    });
  }
}