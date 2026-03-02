using JetBrains.Annotations;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MapVehicles;

public class HiddenIslandManager(World world) : WorldComponent(world)
{
    private Dictionary<int, HashSet<int>> hiddenIslandTileIDs;

    public static HiddenIslandManager Instance { get; private set; }

    [CanBeNull]
    public static HashSet<int> HiddenIslandTileIDs(PlanetLayer layer)
    {
        return Instance.hiddenIslandTileIDs.GetValueOrDefault(layer.LayerID);
    }

    public static void DiscoverHiddenIsland(PlanetTile tile)
    {
        var world = Instance.world;
        var layer = tile.Layer;
        world.landmarks.AddLandmark(MVO_DefOf.MV_OceanIsland, tile, layer, true);
        tile.Tile.PrimaryBiome = NonWaterBiomeFrom(tile.Tile, tile, layer);
        HiddenIslandTileIDs(layer)?.Remove(tile.tileId);
        world.renderer.GetLayer<WorldDrawLayer_Terrain>(layer).SetDirty();
        return;
        
        static BiomeDef NonWaterBiomeFrom(Tile ws, PlanetTile tile, PlanetLayer layer)
        {
            var allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
            BiomeDef biomeDef = null;
            var num = 0f;
            for (var i = 0; i < allDefsListForReading.Count; i++)
            {
                var biomeDef2 = allDefsListForReading[i];
                if (biomeDef2.isWaterBiome) continue;
                if (biomeDef2.implemented && biomeDef2.generatesNaturally && biomeDef2.Worker.CanPlaceOnLayer(biomeDef2, layer))
                {
                    var score = biomeDef2.Worker.GetScore(biomeDef2, ws, tile);
                    if (score > num || biomeDef == null)
                    {
                        biomeDef = biomeDef2;
                        num = score;
                    }
                }
            }
            return biomeDef;
        }
    }

    public override void FinalizeInit(bool fromLoad)
    {
        Instance = this;
        if (hiddenIslandTileIDs is null)
        {
            InitHiddenIslandTileIDs();
        }
    }

    private void InitHiddenIslandTileIDs()
    {
        hiddenIslandTileIDs = [];
        foreach (var layer in world.grid.PlanetLayers.Values)
        {
            if (layer.Def.backgroundBiome != BiomeDefOf.Ocean)
                continue;

            hiddenIslandTileIDs[layer.LayerID] = [];
            foreach (var tile in layer.Tiles)
            {
                if (tile.PrimaryBiome == BiomeDefOf.Ocean &&
                    Rand.ChanceSeeded(MapVehiclesOcean.Mod.settings.hiddenIslandChance, world.ConstantRandSeed ^ layer.LayerID ^ tile.tile.tileId))
                {
                    hiddenIslandTileIDs[layer.LayerID].Add(tile.tile.tileId);
                }
            }
        }
    }

    public override void ExposeData()
    {
        switch (Scribe.mode)
        {
            case LoadSaveMode.Saving when hiddenIslandTileIDs is not null:
            {
                foreach (var layer in world.grid.PlanetLayers.Values)
                {
                    if (hiddenIslandTileIDs.TryGetValue(layer.LayerID, out var hashSet))
                    {
                        Scribe_Collections.Look(ref hashSet, $"hiddenIslandTileIDs{layer.LayerID.ToString()}", LookMode.Value);
                    }
                }

                break;
            }
            case LoadSaveMode.LoadingVars:
            {
                hiddenIslandTileIDs = [];
                foreach (var layer in world.grid.PlanetLayers.Values)
                {
                    HashSet<int> hashSet = null;
                    Scribe_Collections.Look(ref hashSet, $"hiddenIslandTileIDs{layer.LayerID.ToString()}", LookMode.Value);
                    if (hashSet is not null)
                        hiddenIslandTileIDs[layer.LayerID] = hashSet;
                }

                break;
            }
            case LoadSaveMode.Inactive:
            case LoadSaveMode.ResolvingCrossRefs:
            case LoadSaveMode.PostLoadInit:
            default:
                break;
        }
    }

    [DebugAction("MapVehiclesOcean", hideInSubMenu: true)]
    public static void RegenerateHiddenIslands() => Instance?.InitHiddenIslandTileIDs();

    [DebugAction("MapVehiclesOcean", hideInSubMenu: true)]
    public static void FlashHiddenIslands()
    {
        var world = Find.World;
        foreach (var id in Instance.hiddenIslandTileIDs[world.grid.Surface.LayerID])
        {
            world.debugDrawer.FlashTile(world.grid[id].tile, 0.5f);
        }
    }
}