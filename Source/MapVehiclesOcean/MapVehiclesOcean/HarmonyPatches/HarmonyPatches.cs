using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using VehicleMapFramework;
using Vehicles.World;
using Verse;

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
    if (map.Tile.Tile.WaterCovered)
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