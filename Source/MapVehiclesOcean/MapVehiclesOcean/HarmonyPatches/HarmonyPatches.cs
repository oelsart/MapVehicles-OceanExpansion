using HarmonyLib;
using RimWorld;
using VehicleMapFramework;
using Verse;

namespace MapVehiclesOcean.HarmonyPatches;

public static class HarmonyPatches
{
    private const string HarmonyId = "OELS.MapVehiclesOcean";
    
    static HarmonyPatches()
    {
        new Harmony(HarmonyId).PatchAll();
    }
}

[HarmonyPatch(typeof(WaterBodyTracker), nameof(WaterBodyTracker.Notify_Fished))]
public static class Patch_WaterBodyTracker_Notify_Fished
{
    public static void Postfix(IntVec3 c, float amount, Map ___map)
    {
        if (___map.IsVehicleMapOf(out var vehicle) && vehicle.Spawned)
            c.ToBaseMapCoord(vehicle).GetWaterBody(vehicle.Map)?.Population -= amount;
    }
}

// 島タイルでCampした時のためのパッチだが発火してないかも。まず島マップでキャンプできるか不明。
// [HarmonyPatch(typeof(EnterMapUtilityVehicles), nameof(EnterMapUtilityVehicles.EnterMap))]
// public static class Patch_EnterMapUtilityVehicles_EnterMap
// {
//     public static void Prefix(Map map, ref EnterMapUtilityVehicles.SpawnParams spawnParams)
//     {
//         if (map.Tile.Tile.WaterCovered)
//         {
//             spawnParams.enterMode = CaravanEnterMode.Edge;
//             Log.Message("Entering map with water cover, setting enter mode to edge");
//         }
//     }
// }