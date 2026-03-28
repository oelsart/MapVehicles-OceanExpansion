using DelaunatorSharp;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

[HotSwap]
public class LayoutWorker_MaritimeBase(LayoutDef def) : LayoutWorker(def)
{
    private static readonly IntRange RoomSizeRange = new(12, 16);
    private static readonly IntRange RoomCountRange = new(8, 16);
    private static readonly IntRange RoomSpacingRange = new (4, 6);
    private static readonly PriorityQueue<IntVec3, int> openSet = new();
    private static readonly Dictionary<IntVec3, IntVec3> cameFrom = [];
    private static readonly Dictionary<IntVec3, int> gScore = [];
    private static readonly Dictionary<IntVec3, int> fScore = [];
    private static readonly List<IntVec3> toEnqueue = [];
    private static readonly List<IntVec3> tmpCells = [];
    private static readonly List<List<CellRect>> tmpCorridors = [];
    
    public new StructureLayoutDef Def => (StructureLayoutDef)base.Def;

    protected override LayoutSketch GenerateSketch(StructureGenParams parms)
    {
        var sketch = new LayoutSketchComplexTerrain
        {
            wall = ThingDefOf.Wall,
            door = ThingDefOf.Door,
            floor = MVO_DefOf.MVO_FloatingStructureBridge,
            importantFloor = MVO_DefOf.MVO_FloatingStructure,
            importantFloorSpacing = 1,
            defaultAffordanceTerrain = TerrainDefOf.WaterOceanDeep,
            wallStuff = ThingDefOf.WoodLog,
            doorStuff = ThingDefOf.WoodLog,
            structureLayout = GenerateBase(parms)
        };

        return sketch;
    }

    private StructureLayout GenerateBase(StructureGenParams parms)
    {
        var cellRect = new CellRect(0, 0, parms.size.x, parms.size.z);
        var layout = new StructureLayout(parms.sketch, cellRect);
        ScatterSquareRooms(cellRect, layout);
        GenerateGraphs(layout);
        layout.FinalizeRooms(false);
        CreateDoors(layout);
        CreateCorridorsAStar(layout);
        FillEmptySpaces(layout);
        AddCorridorRooms(layout);
        return layout;
    }

    private void AddCorridorRooms(StructureLayout layout)
    {
        foreach (var cellRectList in tmpCorridors)
        {
            layout.AddRoom(cellRectList, Def.corridorDef);
        }
        tmpCorridors.Clear();
    }

    private static void FillEmptySpaces(StructureLayout layout)
    {
        HashSet<IntVec3> intVec3Set = [];
        foreach (var cell in layout.container)
        {
            if (layout.IsEmptyAt(cell) && !intVec3Set.Contains(cell))
            {
                foreach (var position in GenAdjFast.AdjacentCells8Way(cell).AsReadOnlySpan())
                {
                    if (layout.IsWallAt(position))
                    {
                        intVec3Set.Add(cell);
                        break;
                    }
                }
            }
        }

        foreach (var position in intVec3Set)
        {
            layout.Add(position, RoomLayoutCellType.Floor);
        }
    }

    private static void GenerateGraphs(StructureLayout layout)
    {
        List<Vector2> vector2List = [];
        foreach (var room in layout.Rooms)
        {
            var zero = Vector3.zero;
            foreach (var rect in room.rects)
                zero += rect.CenterVector3;
            var vector3 = zero / room.rects.Count;
            vector2List.Add(new Vector2(vector3.x, vector3.z));
        }

        layout.delaunator = new Delaunator(vector2List.ToArray());
        layout.neighbours = new RelativeNeighborhoodGraph(layout.delaunator);
    }

    private static void ScatterSquareRooms(CellRect size, StructureLayout layout)
    {
        var randomInRange1 = RoomCountRange.RandomInRange;
        var num = 0;
        for (var index = 0; index < 300 && num < randomInRange1; ++index)
        {
            var randomInRange2 = RoomSizeRange.RandomInRange;
            var randomInRange3 = RoomSizeRange.RandomInRange;
            var rect = new CellRect(Rand.Range(1, size.Width - 2 - randomInRange2),
                Rand.Range(1, size.Height - 2 - randomInRange3), randomInRange2, randomInRange3);
            if (!OverlapsWithAnyRoom(layout, rect) && CloseWithAnyRoom(layout, rect))
            {
                
                layout.AddRoom([rect]);
                ++num;
            }
        }
    }

    private static void CreateCorridorsAStar(StructureLayout layout)
    {
        foreach (var room in layout.Rooms)
        {
            foreach (var (b, _, _) in layout.GetLogicalRoomConnections(room))
            {
                if (!room.connections.Contains(b))
                    ConnectRooms(layout, room, b);
            }
        }
    }

    private static void ConnectRooms(StructureLayout layout, LayoutRoom a, LayoutRoom b)
    {
        var priorityQueue = new PriorityQueue<(IntVec3, IntVec3), int>();
        foreach (var rect1 in a.rects)
        {
            foreach (var rect2 in b.rects)
            {
                foreach (var c1 in (IEnumerable<IntVec3>)a.entryCells ?? rect1.EdgeCells)
                {
                    if (!rect1.IsCorner(c1) && !rect2.Contains(c1))
                    {
                        var closestEdge1 = rect1.GetClosestEdge(c1);
                        foreach (var c2 in (IEnumerable<IntVec3>)b.entryCells ?? rect2.EdgeCells)
                        {
                            if (!rect2.IsCorner(c2) && !rect1.Contains(c2))
                            {
                                var closestEdge2 = rect2.GetClosestEdge(c2);
                                var lengthManhattan = (c2 - c1).LengthManhattan;
                                var relativeRotation =
                                    Rot4.GetRelativeRotation(closestEdge1, closestEdge2);
                                if (closestEdge1 == Rot4.East && c2.x < rect1.maxX || closestEdge1 == Rot4.West && c2.x > rect1.minX)
                                    lengthManhattan += 4;
                                if (closestEdge1 == Rot4.North && c2.z < rect1.maxZ || closestEdge1 == Rot4.South && c2.z > rect1.minZ)
                                    lengthManhattan += 4;
                                switch (relativeRotation)
                                {
                                    case RotationDirection.None:
                                        lengthManhattan += 2;
                                        break;
                                    case RotationDirection.Clockwise:
                                    case RotationDirection.Counterclockwise:
                                        ++lengthManhattan;
                                        break;
                                    case RotationDirection.Opposite:
                                    default:
                                        break;
                                }

                                priorityQueue.Enqueue((c1, c2), lengthManhattan);
                            }
                        }
                    }
                }
            }
        }

        while (priorityQueue.TryDequeue(out var element, out var priority))
        {
            var intVec3_1 = element.Item1;
            var intVec3_2 = element.Item2;
            if (TryGetPath(layout, intVec3_1, intVec3_2, priority * 2, out var path))
            {
                var intVec3_3 = intVec3_2 - intVec3_1;
                if (Mathf.Max(Mathf.Abs(intVec3_3.x), Mathf.Abs(intVec3_3.z)) <= 4)
                {
                    layout.Add(intVec3_1, RoomLayoutCellType.Floor);
                    layout.Add(intVec3_2, RoomLayoutCellType.Floor);
                    InflatePath(layout, path);
                    var index = 1;
                    if (path.Count == 1 || !layout.IsGoodForDoor(path[index]))
                        index = 0;
                    layout.Add(path[index], RoomLayoutCellType.Door);
                }
                else
                {
                    layout.Add(intVec3_1, RoomLayoutCellType.Door);
                    layout.Add(intVec3_2, RoomLayoutCellType.Door);
                    InflatePath(layout, path);
                }

                a.connections.Add(b);
                b.connections.Add(a);
                break;
            }
        }
    }

    private static void InflatePath(StructureLayout layout, List<IntVec3> cells)
    {
        var cellQueue = new Queue<IntVec3>();
        HashSet<IntVec3> cellSet = [];
        List<CellRect> cellRectList = [];
        foreach (var cell in cells)
        {
            if (layout.IsEmptyAt(cell))
            {
                cellQueue.Enqueue(cell);
                break;
            }
        }

        while (cellQueue.Count != 0)
        {
            var cell = cellQueue.Dequeue();
            if (cells.Contains(cell))
            {
                layout.Add(cell, RoomLayoutCellType.Floor);
                if (layout.Rooms.All(r => !r.Boundary.ExpandedBy(1).Contains(cell)))
                    cellRectList.Add(CellRect.SingleCell(cell));
                foreach (var neighbour in Neighbours(layout, cell))
                {
                    if (layout.IsEmptyAt(neighbour) && !cellSet.Contains(neighbour))
                    {
                        cellQueue.Enqueue(neighbour);
                        cellSet.Add(neighbour);
                    }
                }
            }
        }

        tmpCorridors.Add(cellRectList);
    }

    private static int CountAdjacentWalls(StructureLayout layout, IntVec3 cell)
    {
        var num = 0;
        for (var newRot = 0; newRot < 4; ++newRot)
        {
            var position = cell + new Rot4(newRot).FacingCell;
            if (position is { x: > 0, z: > 0 } && position.x < layout.Width && position.z < layout.Height &&
                layout.IsWallAt(position))
                ++num;
        }

        return num;
    }

    private static List<IntVec3> ReconstructPath(Dictionary<IntVec3, IntVec3> from, IntVec3 current)
    {
        List<IntVec3> intVec3List = [current];
        while (from.ContainsKey(current))
        {
            current = from[current];
            intVec3List.Add(current);
        }

        intVec3List.Reverse();
        return intVec3List;
    }

    private static void ResetPathVars()
    {
        openSet.Clear();
        cameFrom.Clear();
        gScore.Clear();
        fScore.Clear();
        toEnqueue.Clear();
    }

    private static bool TryGetPath(
        StructureLayout layout,
        IntVec3 start,
        IntVec3 goal,
        int max,
        out List<IntVec3> path)
    {
        ResetPathVars();
        gScore.Add(start, 0);
        fScore.Add(start, Heuristic(start, goal));
        openSet.Enqueue(start, fScore[start]);
        while (openSet.Count != 0)
        {
            var intVec3_1 = openSet.Dequeue();
            if (intVec3_1 == goal)
            {
                path = ReconstructPath(cameFrom, intVec3_1);
                ResetPathVars();
                return true;
            }

            toEnqueue.Clear();
            foreach (var neighbour in Neighbours(layout, intVec3_1, goal))
            {
                if (neighbour == goal)
                {
                    cameFrom[neighbour] = intVec3_1;
                    path = ReconstructPath(cameFrom, neighbour);
                    ResetPathVars();
                    return true;
                }

                var num = gScore[intVec3_1] + 1;
                if (num <= max)
                {
                    if (!gScore.ContainsKey(neighbour) ||
                        num < gScore[neighbour])
                    {
                        cameFrom[neighbour] = intVec3_1;
                        gScore[neighbour] = num;
                        fScore[neighbour] =
                            num + Heuristic(neighbour, goal);
                        toEnqueue.Add(neighbour);
                    }
                }
                else
                    break;
            }

            toEnqueue.Sort((x, z) =>
            {
                if (x == z)
                    return 0;
                var intVec3_2 = x - start;
                if (intVec3_2.x == 0 || intVec3_2.z == 0)
                    return -1;
                var intVec3_3 = x - goal;
                if (intVec3_3.x == 0 || intVec3_3.z == 0)
                    return -1;
                var intVec3_4 = z - start;
                if (intVec3_4.x == 0 || intVec3_4.z == 0)
                    return 1;
                var intVec3_5 = z - goal;
                return intVec3_5.x == 0 || intVec3_5.z == 0 ? 1 : 0;
            });
            foreach (var intVec3_6 in toEnqueue)
                openSet.Enqueue(intVec3_6, fScore[intVec3_6]);
        }

        ResetPathVars();
        path = null;
        return false;
    }

    private static IEnumerable<IntVec3> Neighbours(StructureLayout layout, IntVec3 cell)
    {
        for (var i = 0; i < 4; i++)
        {
            var position = cell + new Rot4(i).FacingCell;
            if (position is { x: > 0, z: > 0 } && position.x < layout.Width &&
                position.z < layout.Height && layout.IsEmptyAt(position))
                yield return position;
        }
    }

    private static IEnumerable<IntVec3> Neighbours(StructureLayout layout, IntVec3 cell, IntVec3 goal)
    {
        for (var i = 0; i < 4; ++i)
        {
            var position = cell + new Rot4(i).FacingCell;
            if (position is { x: > 0, z: > 0 } && position.x < layout.Width && position.z < layout.Height &&
                (!(position != goal) || layout.IsEmptyAt(position)))
                yield return position;
        }
    }

    private static int Heuristic(IntVec3 pos, IntVec3 goal) => (goal - pos).LengthManhattan;

    private static bool OverlapsWithAnyRoom(StructureLayout layout, CellRect rect)
    {
        foreach (var room in layout.Rooms)
        {
            foreach (var rect1 in room.rects)
            {
                if (rect1.Overlaps(rect.ExpandedBy(RoomSpacingRange.min)))
                    return true;
            }
        }

        return false;
    }
    
    private static bool CloseWithAnyRoom(StructureLayout layout, CellRect rect)
    {
        if (layout.Rooms.Empty()) return true;
        foreach (var room in layout.Rooms)
        {
            foreach (var rect1 in room.rects)
            {
                if (rect1.Overlaps(rect.ExpandedBy(RoomSpacingRange.max)))
                    return true;
            }
        }

        return false;
    }

    private static void CreateDoors(StructureLayout layout)
    {
        tmpCells.Clear();
        tmpCells.AddRange(layout.container.Cells.InRandomOrder());
        for (var index = 0; index < tmpCells.Count; ++index)
        {
            var tmpCell = tmpCells[index];
            if (layout.IsWallAt(tmpCell))
            {
                if (layout.IsGoodForHorizontalDoor(tmpCell))
                    TryConnectAdjacentRooms(layout, tmpCell, IntVec3.North);
                if (layout.IsGoodForVerticalDoor(tmpCell))
                    TryConnectAdjacentRooms(layout, tmpCell, IntVec3.East);
            }
        }

        tmpCells.Clear();
    }

    private static void TryConnectAdjacentRooms(StructureLayout layout, IntVec3 p, IntVec3 dir)
    {
        if (!layout.TryGetRoom(p + dir, out var room1) || !layout.TryGetRoom(p - dir, out var room2) ||
            room1.connections.Contains(room2))
            return;

        if (layout.GetLogicalRoomConnections(room1).All(logicalRoomConnection => logicalRoomConnection.Item1 != room2) ||
            room1.entryCells != null && !room1.entryCells.Contains(p) ||
            room2.entryCells != null && !room2.entryCells.Contains(p))
            return;
        layout.Add(p, RoomLayoutCellType.Door);
        room1.connections.Add(room2);
        room2.connections.Add(room1);
    }
}