using RimWorld;
using VehicleMapFramework;
using Verse;

namespace MapVehicles;

[HotSwap]
public class CompFishingSpot : ThingComp
{
    protected IntVec3 FishingCell => parent.Position + parent.Rotation.FacingCell;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!ModsConfig.OdysseyActive || !parent.IsOnVehicleMapOf(out var vehicle)) return;
        
        var cell = FishingCell;
        if (!vehicle.CachedImpassableCells.Contains(cell) || parent.Map.zoneManager.ZoneAt(cell) is not null) return;

        var zone = new Zone_FishingOnVehicle(parent.Map.zoneManager);
        parent.Map.zoneManager.RegisterZone(zone);
        zone.AddCell(cell);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        if (!ModsConfig.OdysseyActive || !map.IsVehicleMap) return;
        
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

    public override void CompTickInterval(int delta)
    {
        if (!ModsConfig.OdysseyActive || !parent.IsOnVehicleMapOf(out var vehicle)) return;
        
        var cell = FishingCell;
        var map = parent.Map;
        WaterBodyType waterBodyType;
        bool polluted;
        WaterBody waterBody = null;
        if (vehicle.Spawned)
        {
            var map2 = vehicle.Map;
            var baseCell = cell.ToBaseMapCoord(vehicle);
            waterBodyType = baseCell.GetTerrain(map2)?.waterBodyType ?? WaterBodyType.None;
            waterBody = baseCell.GetWaterBody(map2);
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
            WaterBodyType.Freshwater => MVO_DefOf.MV_WaterDeepVirtual,
            WaterBodyType.Saltwater => MVO_DefOf.MV_WaterOceanDeepVirtual,
            _ => MVO_DefOf.MV_NoWaterVirtual
        };
        if (cell.GetTerrain(map) != terrain)
        {
            map.terrainGrid.SetTerrain(cell, terrain);
            if (cell.GetZone(map) is Zone_FishingOnVehicle zone)
                Find.Selector.Deselect(zone);
        }
        if (cell.IsPolluted(map) != polluted)
            map.pollutionGrid.SetPolluted(cell, polluted, true);

        if (waterBodyType == WaterBodyType.None) return;
        cell.GetWaterBody(map)?.Population = waterBody?.Population ?? map.TileInfo.FishPopulationFactor * 50f;
    }
}