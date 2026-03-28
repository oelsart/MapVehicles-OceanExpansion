using RimWorld;
using VehicleMapFramework;
using Verse;

namespace MapVehiclesOcean;

[HotSwap]
public class ExtendedRoomGenUtility
{
	public static void FillPrefabs(RoomPart_PrefabDef def, LayoutRoom room, Map map,
		Func<IntVec3, Rot4, bool> validator = null, int contractedBy = 1, List<Thing> spawned = null,
        bool avoidDoors = true, Faction faction = null)
	{
		var size = def.prefab.size;
        int[] rots = [0, 1, 2, 3];
		foreach (var intVec in room.Cells)
		{
			if (room.Contains(intVec, contractedBy))
            {
                rots.Shuffle();
                for (var i = 0; i < rots.Length; i++)
                {
                    var rot = new Rot4(rots[i]);
                    if ((def.prefab.rotations & rot) > 0)
                    {
                        if (def.alignWithRect && room.TryGetRectContainingCell(intVec, out var cellRect))
                        {
                            var rot2 = cellRect.Width >= cellRect.Height ? Rot4.East : Rot4.North;
                            if (rot != rot2)
                            {
                                continue;
                            }
                        }

                        if ((!def.snapToGrid || intVec.x % (size.x + 1) == 0 && intVec.z % (size.z + 1) == 0) &&
                            (validator == null || validator(intVec, rot)))
                        {
                            var flag = true;

                            foreach (var (data, cell, thingRot) in PrefabUtility.GetThings(def.prefab, intVec, rot))
                            {
                                foreach (var cell2 in cell.RectAbout(data.def.Size, thingRot))
                                {
                                    if (avoidDoors && RoomGenUtility.IsDoorAdjacentTo(cell2, map) ||
                                        !def.forceSpawn &&
                                        !IsFillValidCell(cell2, room, map, contractedBy, def.ignoreAdjacent))
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }

                            if (flag)
                            {
                                PrefabUtility.SpawnPrefab(def.prefab, map, intVec, rot, faction, spawned);
                            }
                        }
                    }
                }
            }
		}
	}


	public static void FillPrefabsAroundEdges(RoomPart_PrefabDef def, LayoutRoom room, Map map,
        Func<IntVec3, Rot4, bool> validator = null, List<Thing> spawned = null, int contractedBy = 1,
        bool avoidDoors = true, Faction faction = null)
    {
        var edgeSpawnOptions = GetEdgeSpawnOptions(room, map, contractedBy);

        var curDir = Rot4.Invalid;
        var interval = def.intervalRange.RandomInRange;
        var index = -1;
        
        
        for (var i = 0; i < edgeSpawnOptions.Count; i++)
        {
            var dir = edgeSpawnOptions[i].rot;
            if (index != -1 && i < index + interval && curDir.IsValid && dir == curDir)
                continue;

            var cell = edgeSpawnOptions[i].cell;
            var rot = dir.Rotated(def.rotOffset);
        
            if ((def.prefab.rotations & rot) > 0)
            {
                var cellRect = cell.RectAbout(def.prefab.size, rot);
                var offset = -dir.FacingCell;
                var sideLength = room.Boundary.GetSideLength(dir.Rotated(RotationDirection.Clockwise));
                while (!cellRect.GetEdgeRect(dir).Contains(cell) && --sideLength > 0)
                    cellRect = cellRect.MovedBy(offset);
            
                var flag = false;
                foreach (var rect in room.rects)
                {
                    if (cellRect.FullyContainedWithin(rect.ContractedBy(def.contractedBy)))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag && (validator == null || validator(cellRect.CenterCell, rot)))
                {
                    var center = cellRect.CenterCell;
                    var size = cellRect.Size;
                    if (rot.IsHorizontal) size = size.Rotated();
                    GenAdj.AdjustForRotation(ref center, ref size, rot.Opposite);
                    foreach (var (data, pos, rot2) in PrefabUtility.GetThings(def.prefab, center, rot))
                    {
                        foreach (var cell2 in pos.RectAbout(data.def.Size, rot2))
                        {
                            if (avoidDoors && RoomGenUtility.IsDoorAdjacentTo(cell2, map, false) ||
                                (!def.forceSpawn || curDir.IsValid && curDir != dir) &&
                                !IsFillValidCell(cell2, room, map, contractedBy, def.ignoreAdjacent, edgeSpawnOptions[i].wall))
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        PrefabUtility.SpawnPrefab(def.prefab, map, center, rot, faction, spawned);
                        index = i;
                        curDir = dir;
                    }
                }
            }
        }
    }
    
    private static List<(IntVec3 cell, Rot4 rot, CellRect wall)> GetEdgeSpawnOptions(LayoutRoom room, Map map, int contractedBy)
    {
        var edgeSpawnOptions = new List<(IntVec3, Rot4, CellRect)>();
        foreach (var rect in room.rects)
        {
            var cellRect = rect.ContractedBy(contractedBy);
            for (var i = 0; i < 4; ++i)
            {
                var rot = new Rot4(i);
                foreach (var edgeCell in cellRect.EdgeRectClockwise(rot))
                {
                    var flag = true;
                    if (!edgeSpawnOptions.Select(o => o.Item1).Contains(edgeCell) &&
                        (edgeCell + rot.FacingCell).GetEdifice(map) != null)
                    {
                        foreach (var rect2 in room.rects)
                        {
                            if (rect2 != rect && rect2.Contains(edgeCell))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                            edgeSpawnOptions.Add((edgeCell, rot, rect));
                    }
                }
            }
        }
        return edgeSpawnOptions;
    }
    
    private static bool IsFillValidCell(IntVec3 cell, LayoutRoom room, Map map, int contractedBy,
        bool ignoreAdjacent, CellRect ignore = default)
    {
        var onIgnoreCell = ignore != default && ignore.IsOnEdge(cell);
        if (!room.Contains(cell, contractedBy) && !onIgnoreCell)
        {
            return false;
        }
        if (cell.GetEdifice(map) != null && !onIgnoreCell)
        {
            return false;
        }
        if (!ignoreAdjacent)
        {
            for (var i = 0; i < 8; i++)
            {
                var intVec = cell + GenAdj.AdjacentCellsAround[i];
                var edifice = intVec.GetEdifice(map);
                if (edifice != null && edifice.def.passability != Traversability.Standable &&
                    (ignore == default || !ignore.IsOnEdge(intVec)))
                {
                    return false;
                }
            }
        }
        return true;
    }
}