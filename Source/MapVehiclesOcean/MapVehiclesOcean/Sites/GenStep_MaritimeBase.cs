using RimWorld;
using Verse;

namespace MapVehiclesOcean;

[HotSwap]
public class GenStep_MaritimeBase : GenStep
{
    public override int SeedPart => 1871699328;
    private static readonly IntVec2 DefaultSize = new (100, 100);

    public override void Generate(Map map, GenStepParams parms)
    {
        var sketch = MVO_DefOf.MVO_MaritimeBase.Worker.GenerateStructureSketch(new StructureGenParams
        {
            size = DefaultSize,
            faction = map.ParentFaction
        });
        map.layoutStructureSketches.Add(sketch);
        var cellRect = CellRect.CenteredOn(map.Center, DefaultSize);
        Dictionary<IntVec3, TerrainDef> terrains = [];
        foreach (var cell in cellRect)
        {
            terrains[cell] = map.terrainGrid.BaseTerrainAt(cell);
            map.terrainGrid.SetTerrain(cell, TerrainDefOf.WaterShallow);
        }
        MVO_DefOf.MVO_MaritimeBase.Worker.Spawn(sketch, map, cellRect.Min, faction: map.ParentFaction);
        foreach (var cell in cellRect)
        {
            if (map.terrainGrid.TopTerrainAt(cell) is { layerable: true })
            {
                map.terrainGrid.SetUnderTerrain(cell, terrains[cell]);
            }
            else
            {
                map.terrainGrid.SetTerrain(cell, terrains[cell]);
            }
        }
        terrains.Clear();
        MapGenerator.SetVar("SpawnRect", cellRect);
    }
}