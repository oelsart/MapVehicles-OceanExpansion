using RimWorld;
using SmashTools;
using Verse;

namespace MapVehiclesOcean;

public class LayoutSketchComplexTerrain : LayoutSketch
{
    public TerrainDef importantFloor;
    public int importantFloorSpacing = 1;
    private int index;
    private List<bool> floorCells;

    protected override TerrainDef FloorTerrain
    {
        get
        {
            var rect = structureLayout.container;
            if (floorCells is null)
            {
                floorCells = [];
                for (var i = rect.minX; i <= rect.maxX; i++)
                {
                    for (var j = rect.minZ; j <= rect.maxZ; j++)
                    {
                        var cell = new IntVec3(i, 0, j);
                        if (structureLayout.IsFloorAt(cell) || structureLayout.IsDoorAt(cell) || structureLayout.IsWallAt(cell))
                        {
                            floorCells.Add(cell.x % (importantFloorSpacing + 1) == 0);
                        }
                    }
                }
            }

            return !floorCells.OutOfBounds(index) && floorCells[index++] ? importantFloor : floor;
        }
    }
}