using HarmonyLib;
using RimWorld;
using UnityEngine;
using VehicleMapFramework;
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