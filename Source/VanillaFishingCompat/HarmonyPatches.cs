using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using VehicleMapFramework.VMF_HarmonyPatches;
using Verse;
using Zone_Fishing = VCE_Fishing.Zone_Fishing;

namespace MapVehiclesOcean.VanillaFishingCompat;

[StaticConstructorOnStartup]
internal static class HarmonyPatches
{
	static HarmonyPatches()
	{
		new Harmony("OELS.MapVehiclesOcean.VanillaFishingCompat").PatchAll(Assembly.GetExecutingAssembly());
	}
}

[HarmonyPatch(typeof(Zone_Fishing), nameof(Zone_Fishing.InitialSetZoneFishList))]
public static class Patch_Zone_Fishing_InitialSetZoneFishList
{
	public static bool Execute { private get; set; }

	public static bool Prefix(Zone_Fishing __instance)
	{
		if (Execute)
		{
			Execute = false;
			return true;
		}
		if (__instance is Zone_FishingOnVehicle zone)
		{
			zone.InitialSetZoneFishList();
			return false;
		}
		return true;
	}

	public static List<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		// isOcean判定がWaterOceanDeepあるいはShallowのみにされているので、IsOceanなら判定にかかるようにする
		return PatchHelper.CreateCodeMatcherFast(instructions)
			.MatchStartForward(
				CodeMatch.LoadsField(AccessTools.Field(typeof(TerrainDefOf), nameof(TerrainDefOf.WaterOceanDeep))))
			.Insert(((Delegate)OceanToDeepOcean).Method.CallInstruction)
			.Instructions();
	}
	
	private static TerrainDef OceanToDeepOcean(TerrainDef terrainDef)
	{
		return terrainDef.IsOcean ? TerrainDefOf.WaterOceanDeep : terrainDef;
	}
}

[HarmonyPatch]
public static class Patch_WorkGiver_Fish_PotentialWorkCellsGlobal
{
	private static MethodInfo TargetMethod()
	{
		return GenTypes.GetTypeInAnyAssembly("VCE_Fishing.WorkGiver_Fish", "VCE_Fishing")
			.FindIncludingInnerTypes(t =>
				!t.Name.Contains("PotentialWorkCellsGlobal") ? null : AccessTools.Method(t, "MoveNext"));
	}
	
	public static List<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		return PatchHelper.CreateCodeMatcherFast(instructions)
			.MatchEndForward(
				CodeMatch.Calls(AccessTools.Method(typeof(LocalTargetInfo), "op_Implicit", [typeof(IntVec3)])),
				new CodeMatch(OpCodes.Ldc_I4_1))
			.Set(OpCodes.Ldc_I4_2, null)
			.Instructions();
	}
}