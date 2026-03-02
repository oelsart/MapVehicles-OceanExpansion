using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using VehicleMapFramework;
using Vehicles.World;
using Verse;

namespace MapVehicles.HarmonyPatches;

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

[HarmonyPatch(typeof(EnterMapUtilityVehicles), nameof(EnterMapUtilityVehicles.EnterMap))]
public static class Patch_EnterMapUtilityVehicles_EnterMap
{
    public static void Prefix(Map map, ref EnterMapUtilityVehicles.SpawnParams spawnParams)
    {
        if (map.Tile.Tile.WaterCovered && spawnParams.enterMode == CaravanEnterMode.Center)
            spawnParams.enterMode = CaravanEnterMode.Edge;
    }
}