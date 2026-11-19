using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VehicleMapFramework;
using Verse;
using Verse.Sound;

namespace MapVehiclesOcean;

public class CompAncientBook : CompInteractable
{
	private CameraShaker shaker;
	private Sustainer sustainer;
	private PlanetTile planetTile;
	private float estimatedTime = -1f;
	private float startTime = -1f;
	private List<(PlanetTile, int)> tiles;
	
	private const float FadeOutDuration = 4.5f;
	private const float UpliftDuration = 15f;
	private const float AftershockDuration = 4f;
	private const int UpliftCount = 5;
	private const int UpliftRadius = 15;
	private const string ClearSignal = "Clear";
	private const float ElevationStep = 10f;
	private const float MaxElevation = 150f;
	private const float CutsceneDuration = FadeOutDuration + UpliftDuration + AftershockDuration;

	public static bool CutsceneInProgress { get; private set; }

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		planetTile = parent.Map?.PocketMapParent?.sourceMap?.Tile ?? parent.Tile;
		if (!planetTile.Valid)
			VMF_Log.Error("The ancient book is spawned on an invalid tile.");
	}

	protected override void OnInteracted(Pawn _)
	{
		if (Find.World.GetComponent<CutsceneController>() is { } cutsceneController)
		{
			cutsceneController.BeginCutscene(OnCutsceneStart, CutsceneUpdate, OnCutsceneEnd, CutsceneDuration);
		}
	}

	private void OnCutsceneStart()
	{
		tiles = [];
		List<PlanetTile> landmarkTiles = [];
		List<PlanetTile> neighbors = [];
		Find.WorldGrid.GetTileNeighbors(planetTile, neighbors);
		planetTile.Layer.Filler.FloodFill(planetTile, t => t.Tile.WaterCovered, (_, to, distance) =>
		{
			if (distance > UpliftRadius) return true;

			Find.WorldGrid.GetTileNeighbors(to, neighbors);
			foreach (var neighbor in neighbors)
			{
				// coastRotateModeがあるランドマークの周囲1タイルを対象外にしておく
				if (!landmarkTiles.Contains(neighbor) &&
				    neighbor.Tile is { Landmark.def.coastRotateMode: not LandmarkDef.CoastRotateMode.None })
				{
					landmarkTiles.Add(neighbor);
					return false;
				}
			}

			if (to is { Tile.elevation: > MaxElevation })
				return false;
			
			tiles.Add((to, distance));
			return false;
		}, extraRootTiles: neighbors);
		ScreenFader.StartFade(Color.black, FadeOutDuration);
		MapVehiclesOcean.Harmony.Patch(
			AccessTools.PropertyGetter(typeof(WorldRendererUtility), nameof(WorldRendererUtility.CurrentWorldRenderMode)),
			postfix: ((Delegate)Patch_WorldRendererUtility_CurrentWorldRenderMode.Postfix).Method);
	}

	private void CutsceneUpdate(float elapsed)
	{
		if (!CutsceneInProgress)
		{
			if (elapsed >= FadeOutDuration)
			{
				CutsceneInProgress = true;
				CameraJumper.TryShowWorld();
				Find.World.renderer.RegenerateLayersIfDirtyInLongEvent();
				Find.WorldCameraDriver.JumpTo(planetTile);
				shaker = new CameraShaker();
				shaker.DoShake(0.2f);
				FrameDelay.DelayOne<object>(_ => ScreenFader.StartFade(Color.clear, 0f), null);
			}

			return;
		}
		if (LongEventHandler.ShouldWaitForEvent) return;

		var lastRealTime = RealTime.LastRealTime;
		if (startTime < 0f)
		{
			startTime = RealTime.LastRealTime;
			estimatedTime = startTime + 4f;
		}
		if (estimatedTime < lastRealTime)
		{
			if (startTime + UpliftDuration > lastRealTime)
			{
				estimatedTime = lastRealTime + (UpliftDuration - 3f) / UpliftCount;
				DoGroundUplift();
			}
			else
			{
				shaker.CurShakeMag = Mathf.Lerp(0.2f, 0f, (lastRealTime - estimatedTime) / AftershockDuration);
				if (sustainer is { Ended: false})
					sustainer.End();
			}
		}
		sustainer ??= MVO_DefOf.MVO_EarthRumble.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerFrame));
		if (sustainer is { Ended: false})
			sustainer.Maintain();
		
		Find.WorldCamera.transform.Translate(shaker.ShakeOffset.ToVector2());
		return;
		
		void DoGroundUplift()
		{
			foreach (var (tile, distance) in tiles)
			{
				var diff = Mathf.Max(0f, -tile.Tile.elevation) * Rand.Range(0.6f, 0.9f);
				var t = (float)distance / UpliftRadius;
				var factor = 1f - t * t;
				tile.Tile.elevation =
					Mathf.Min(tile.Tile.elevation + (diff + ElevationStep * Rand.Range(0.75f, 1.25f)) * factor, MaxElevation);
				if (tile.Tile.elevation > 0f && tile.Tile.PrimaryBiome.isWaterBiome)
				{
					tile.Tile.PrimaryBiome = BiomeFrom(tile);
				}
			}

			if (Find.World.renderer.GetLayer<WorldDrawLayer_UpliftedTerrain>(planetTile.Layer) is { } layer)
				layer.AppendUpliftedTiles(tiles);
		}
	}

	private static BiomeDef BiomeFrom(PlanetTile tile)
	{
		var allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
		BiomeDef biomeDef = null;
		var num = 0f;
		for (var i = 0; i < allDefsListForReading.Count; i++)
		{
			var biomeDef2 = allDefsListForReading[i];
			if (biomeDef2.implemented && biomeDef2.generatesNaturally && biomeDef2.Worker.CanPlaceOnLayer(biomeDef2, tile.Layer))
			{
				var score = biomeDef2.Worker.GetScore(biomeDef2, tile.Tile, tile);
				if (score > num || biomeDef == null)
				{
					biomeDef = biomeDef2;
					num = score;
				}
			}
		}

		return biomeDef;
	}

	private void OnCutsceneEnd()
	{
		CutsceneInProgress = false;
		estimatedTime = -1f;
		startTime = -1f;
		if (Find.World.renderer.GetLayer<WorldDrawLayer_UpliftedTerrain>(planetTile.Layer) is { } layer)
			layer.Clear();
		if (Find.World.renderer.GetLayer<WorldDrawLayer_Terrain>(planetTile.Layer) is { } layer2)
			layer2.RegenerateNow();
		QuestUtility.SendQuestTargetSignals(parent.questTags, ClearSignal);
		sustainer = null;
		shaker = null;
		MapVehiclesOcean.Harmony.Unpatch(
			AccessTools.PropertyGetter(typeof(WorldRendererUtility), nameof(WorldRendererUtility.CurrentWorldRenderMode)),
			((Delegate)Patch_WorldRendererUtility_CurrentWorldRenderMode.Postfix).Method);
	}
}

public static class Patch_WorldRendererUtility_CurrentWorldRenderMode
{
	public static void Postfix(ref WorldRenderMode __result)
	{
		if (CompAncientBook.CutsceneInProgress) __result = WorldRenderMode.Planet;
	}
}