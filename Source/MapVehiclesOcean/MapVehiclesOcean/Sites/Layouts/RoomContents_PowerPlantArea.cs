using RimWorld;
using Verse;

namespace MapVehiclesOcean;

[HotSwap]
public class RoomContents_PowerPlantArea : RoomContentsWorker
{
    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
    {
        ClearRoom(map, room);
        SpawnWindTurbines(map, room, faction);
        SpawnSolarGenerators(map, room, faction);
        base.FillRoom(map, room, faction, threatPoints);
    }

    private static void ClearRoom(Map map, LayoutRoom room)
    {
        foreach (var rect in room.rects)
        {
            GenDebug.ClearArea(rect, map);
        }
    }

    private static void SpawnWindTurbines(Map map, LayoutRoom room, Faction faction)
    {
        var center = room.Boundary.CenterCell;
        center.x -= ThingDefOf.WindTurbine.Size.x / 2;
        SpawnRow(ThingDefOf.WindTurbine, center, Rot4.South, map, room, faction);
    }

    private static void SpawnSolarGenerators(Map map, LayoutRoom room, Faction faction)
    {
        var center = room.Boundary.CenterCell;
        var curZ = center.z + ThingDefOf.WindTurbine.Size.z;
        var curCenter = new IntVec3(center.x, center.y, curZ);
        var rect = curCenter.RectAbout(ThingDefOf.SolarGenerator.Size);
        while (RoomContainsRect(room, rect))
        {
            SpawnRow(ThingDefOf.SolarGenerator, curCenter, Rot4.North, map, room, faction);
            curZ += ThingDefOf.SolarGenerator.Size.z;
            curCenter = new IntVec3(center.x, center.y, curZ);
            rect = curCenter.RectAbout(ThingDefOf.SolarGenerator.Size);
        }
        
        curZ = center.z - ThingDefOf.SolarGenerator.Size.z;
        curCenter = new IntVec3(center.x, center.y, curZ);
        rect = curCenter.RectAbout(ThingDefOf.SolarGenerator.Size);
        while (RoomContainsRect(room, rect))
        {
            SpawnRow(ThingDefOf.SolarGenerator, curCenter, Rot4.North, map, room, faction);
            curZ -= ThingDefOf.SolarGenerator.Size.z;
            curCenter = new IntVec3(center.x, center.y, curZ);
            rect = curCenter.RectAbout(ThingDefOf.SolarGenerator.Size);
        }
    }
    
    private static void SpawnRow(ThingDef def, IntVec3 center, Rot4 rot, Map map, LayoutRoom room, Faction faction)
    {
        if (!Spawn(center, rot)) return;
        
        var curX = center.x;
        while (Spawn(new IntVec3(curX, center.y, center.z), rot))
        {
            curX += def.Size.x;
        }

        curX = center.x;
        while (Spawn(new IntVec3(curX, center.y, center.z), rot))
        {
            curX -= def.Size.x;
        }

        return;
        
        bool Spawn(IntVec3 cell, Rot4 rot)
        {
            var rect = cell.RectAbout(def.Size);
            if (RoomContainsRect(room, rect))
            {
                var thing = GenSpawn.Spawn(def, cell, map, rot);
                if (thing is null) return false;
                thing.SetFaction(faction);
                return true;
            }
            return false;
        }
    }
    
    private static bool RoomContainsRect(LayoutRoom room, CellRect rect)
    {
        foreach (var r in room.rects)
        {
            if (rect.FullyContainedWithin(r)) return true;
        }
        return false;
    }
}