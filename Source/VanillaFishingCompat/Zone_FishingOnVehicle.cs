using System.Text;
using RimWorld;
using UnityEngine;
using VCE_Fishing;
using VCE_Fishing.Options;
using VehicleMapFramework;
using Verse;
using Zone_Fishing = VCE_Fishing.Zone_Fishing;

namespace MapVehiclesOcean.VanillaFishingCompat;

public class Zone_FishingOnVehicle : Zone_Fishing
{
	private List<IntVec3> baseMapWaterCells = [];
	private Map baseMap;
	
	public Zone_FishingOnVehicle()
	{
	}

	public Zone_FishingOnVehicle(ZoneManager zoneManager) : base(zoneManager)
	{
	}

	public new void InitialSetZoneFishList()
	{
		if (!Map.IsVehicleMapOf(out var vehicle) || cells.NullOrEmpty())
		{
			Patch_Zone_Fishing_InitialSetZoneFishList.Execute = true;
			base.InitialSetZoneFishList();
			return;
		}

		if (vehicle.Spawned)
		{
			var prevBaseMap = baseMap;
			baseMap = vehicle.Map;
			baseMapWaterCells ??= [];
			var root = cells[0].ToBaseMapCoord(vehicle);
			var rootTerr = root.GetTerrain(baseMap);
			if (!rootTerr.IsWater)
			{
				baseMapWaterCells.Clear();
				isZoneEmpty = true;
				isZoneBigEnough = false;
				return;
			}

			if (prevBaseMap == baseMap && !baseMapWaterCells.NullOrEmpty() && baseMapWaterCells.Contains(root))
				return;
			
			baseMapWaterCells.Clear();
			baseMap.floodFiller.FloodFill(root, c => c.GetTerrain(baseMap).IsWater, c => baseMapWaterCells.Add(c));
			(cells, baseMapWaterCells) = (baseMapWaterCells, cells);
			zoneManager = baseMap.zoneManager;
			try
			{
				Patch_Zone_Fishing_InitialSetZoneFishList.Execute = true;
				base.InitialSetZoneFishList();
			}
			finally
			{
				(cells, baseMapWaterCells) = (baseMapWaterCells, cells);
				zoneManager = vehicle.VehicleMap.zoneManager;
			}
			return;
		}
		
		var terrain = cells[0].GetTerrain(Map);
		if (terrain == MVO_DefOf.MVO_WaterOceanDeepVirtual)
		{
			Map.terrainGrid.SetTerrain(cells[0], TerrainDefOf.WaterOceanDeep);
		}

		while (cells.Count < VCE_Fishing_Settings.VCEF_minimumZoneSize)
		{
			cells.Add(cells[0]);
		}

		try
		{
			Patch_Zone_Fishing_InitialSetZoneFishList.Execute = true;
			base.InitialSetZoneFishList();
		}
		finally
		{
			Map.terrainGrid.SetTerrain(cells[0], terrain);
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		yield return FishToCatchSettableUtility.SetFishToCatchCommand(this, Map);
		yield return new Command_Toggle
		{
			defaultLabel = "VCEF_CommandAllowFishing".Translate(),
			defaultDesc = "VCEF_CommandAllowFishingDesc".Translate(),
			hotKey = KeyBindingDefOf.Command_ItemForbid,
			icon = ContentFinder<Texture2D>.Get("UI/Designators/VCEF_AllowFish"),
			isActive = () => allowFishing,
			toggleAction = () =>
			{
				allowFishing = !allowFishing;
			}
		};
	}

	public override IEnumerable<Gizmo> GetZoneAddGizmos() => [];
	
	public override string GetInspectString()
	{
		var cellList = Map.IsVehicleMapOf(out var vehicle) && vehicle.Spawned
			? baseMapWaterCells
			: cells;

		if (!isZoneBigEnough)
			return "VCEF_ZoneTooSmall".Translate(cellList.Count, VCE_Fishing_Settings.VCEF_minimumZoneSize);

		if (isZoneEmpty)
			return "VCEF_NoFish".Translate();
		
		var stringBuilder = new StringBuilder($"{"VCEF_ZoneSetTo".Translate(cellList.Count)}: {GetFishToCatch()}");
		if (fishInThisZone is not null)
		{
			stringBuilder.AppendInNewLine("VCEF_FishesInThisZone".Translate());
			stringBuilder.AppendJoin(", ", fishInThisZone.Select(static item => item.label));
		}

		stringBuilder.AppendInNewLine($"{"VCEF_IsZoneOceanZone".Translate()}{(isOcean ? "VCEF_Yes" : "VCEF_No").Translate()}");
		return stringBuilder.ToString();
	}
}