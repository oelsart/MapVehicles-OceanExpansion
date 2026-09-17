using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MapVehiclesOcean;

public class HiddenIslandManager(World world) : WorldComponent(world)
{
  private Dictionary<int, HashSet<int>> hiddenIslandTileIDs;
  
  private PlanetTile specialIslandTile = PlanetTile.Invalid;
  
  public PlanetTile SpecialIslandTile
  {
	  get => specialIslandTile;
	  set => specialIslandTile = value;
  }

  public HashSet<int> HiddenIslandTileIDs(PlanetLayer layer)
  {
    return hiddenIslandTileIDs.GetValueOrDefault(layer.LayerID);
  }

  public void DiscoverHiddenIsland(PlanetTile tile)
  {
    var layer = tile.Layer;
    if (ModsConfig.OdysseyActive)
    {
	    world.landmarks.AddLandmark(MVO_DefOf.MVO_OceanIsland, tile, layer, true);
    }
    else
    {
	    tile.Tile.AddMutator(MVO_DefOf.MVO_Island);
    }

    tile.Tile.elevation = 5f;
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
            Rand.ChanceSeeded(MapVehiclesOcean.Mod.settings.hiddenIslandChance,
              world.ConstantRandSeed ^ layer.LayerID ^ tile.tile.tileId))
        {
          hiddenIslandTileIDs[layer.LayerID].Add(tile.tile.tileId);
        }
      }
    }
  }

  public override void ExposeData()
  {
	  Scribe_Values.Look(ref specialIslandTile, nameof(specialIslandTile), PlanetTile.Invalid);
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

  [DebugAction("MapVehiclesOcean", hideInSubMenu: true, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
  public static void RegenerateHiddenIslands() =>
	  Find.World.GetComponent<HiddenIslandManager>()?.InitHiddenIslandTileIDs();

  [DebugAction("MapVehiclesOcean", hideInSubMenu: true, allowedGameStates = AllowedGameStates.PlayingOnWorld)]
  public static void FlashHiddenIslands()
  {
    var world = Find.World;
    foreach (var id in world.GetComponent<HiddenIslandManager>().hiddenIslandTileIDs[world.grid.Surface.LayerID])
    {
      world.debugDrawer.FlashTile(world.grid[id].tile, 0.5f);
    }
  }
}