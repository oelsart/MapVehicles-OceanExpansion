using MapVehiclesOcean.HarmonyPatches;
using RimWorld;
using VehicleMapFramework;
using Verse;

namespace MapVehiclesOcean;

public class CompFishingSpot : ThingComp
{
  protected IntVec3 FishingCell => parent.Position + parent.Rotation.FacingCell;

  protected virtual Type Zone_FishingOnVehicle => typeof(Zone_FishingOnVehicle);

  protected virtual bool RequiredModActive => ModsConfig.OdysseyActive;

  public override void PostSpawnSetup(bool respawningAfterLoad)
  {
    if (!RequiredModActive || !parent.IsOnVehicleMapOf(out var vehicle)) return;

    var cell = FishingCell;
    if (!cell.InBounds(parent.Map) || !vehicle.ImpassableCellGrid[cell] || parent.Map.zoneManager.ZoneAt(cell) is not null) return;

    var zone = (Zone)Activator.CreateInstance(Zone_FishingOnVehicle, parent.Map.zoneManager)!;
    zone.AddCell(cell);
    parent.Map.zoneManager.RegisterZone(zone);
  }

  public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
  {
    if (!RequiredModActive || !map.IsVehicleMap) return;

    var cell = FishingCell;
    for (var i = 0; i < GenAdj.CardinalDirections.Length; i++)
    {
      var cell2 = cell + GenAdj.CardinalDirections[i];
      if (!cell2.InBounds(map) || cell2 == parent.Position) continue;

      if (cell2.GetFirstThingWithComp<CompFishingSpot>(map) is { } thingWithComp &&
          cell2 + thingWithComp.Rotation.FacingCell == cell)
        return;
    }

    map.terrainGrid.SetTerrain(cell, VMF_DefOf.VMF_ImpassableFloor);
  }

  public override void CompTickRare()
  {
    if (!RequiredModActive || !parent.IsOnVehicleMapOf(out var vehicle)) return;

    var cell = FishingCell;
    var map = parent.Map;
    WaterBodyType waterBodyType;
    bool polluted;
    IntVec3 baseCell;
    Map map2;
    if (vehicle.Spawned && (baseCell = cell.ToBaseMapCoord(vehicle)).InBounds(map2 = vehicle.Map))
    {
      waterBodyType = baseCell.GetTerrain(map2)?.waterBodyType ?? WaterBodyType.None;
      polluted = baseCell.IsPolluted(map2);
    }
    else
    {
      var biome = map.TileInfo.PrimaryBiome;
      if (biome == BiomeDefOf.Ocean)
        waterBodyType = WaterBodyType.Saltwater;
      else if (biome.isWaterBiome)
        waterBodyType = WaterBodyType.Freshwater;
      else waterBodyType = WaterBodyType.None;
      polluted = map.TileInfo.PollutionLevel() > PollutionLevel.None;
    }

    var terrain = waterBodyType switch
    {
      WaterBodyType.Freshwater => MVO_DefOf.MVO_WaterDeepVirtual,
      WaterBodyType.Saltwater => MVO_DefOf.MVO_WaterOceanDeepVirtual,
      _ => MVO_DefOf.MVO_NoWaterVirtual
    };

    var terrain2 = cell.GetTerrain(map);
    if (terrain2 != terrain)
    {
      if (terrain2.passability == Traversability.Impassable)
      {
	      // 車両マップ上に1マスのWaterBodyが形成されるにはパッチを一度スキップさせる必要がある
	      Patch_WaterBodyTracker_TryGetWaterBodyAt.Initialize = true;
        map.terrainGrid.SetTerrain(cell, terrain);
        Patch_WaterBodyTracker_TryGetWaterBodyAt.Initialize = false;
        if (cell.GetZone(map) is { } zone)
          Find.Selector.Deselect(zone);
      }
      else
      {
        parent.Destroy();
        return;
      }
    }

    if (cell.IsPolluted(map) != polluted)
      map.pollutionGrid.SetPolluted(cell, polluted, true);
  }
}