using RimWorld;
using Verse;

namespace MapVehiclesOcean;

[HotSwap]
public class GenStep_MaritimeBase : GenStep
{
  public override int SeedPart => 1871699328;
  
  private static readonly IntVec2 DefaultSize = new(100, 100);

  public override void Generate(Map map, GenStepParams parms)
  {
    var sketch = MVO_DefOf.MVO_MaritimeBase.Worker.GenerateStructureSketch(new StructureGenParams
    {
      size = DefaultSize,
      faction = map.ParentFaction
    });
    map.layoutStructureSketches.Add(sketch);
    var cellRect = CellRect.CenteredOn(map.Center, DefaultSize);
    MVO_DefOf.MVO_MaritimeBase.Worker.Spawn(sketch, map, cellRect.Min, faction: map.ParentFaction);
    MapGenerator.SetVar("SpawnRect", cellRect);
  }
}