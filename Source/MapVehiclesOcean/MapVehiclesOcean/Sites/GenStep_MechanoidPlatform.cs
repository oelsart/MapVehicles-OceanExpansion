using JetBrains.Annotations;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GenStep_MechanoidPlatform : GenStep
{
	public override int SeedPart => 671636476;

    private static readonly IntVec2 DefaultSize = new(100, 100);

    public override void Generate(Map map, GenStepParams parms)
    {
      var sketch = MVO_DefOf.MVO_MechanoidOceanPlatform.Worker.GenerateStructureSketch(new StructureGenParams
      {
        size = DefaultSize,
        faction = map.ParentFaction
      });
      map.layoutStructureSketches.Add(sketch);
      var cellRect = CellRect.CenteredOn(map.Center, DefaultSize);
      MVO_DefOf.MVO_MechanoidOceanPlatform.Worker.Spawn(sketch, map, cellRect.Min, faction: map.ParentFaction);
      MapGenerator.SetVar("SpawnRect", cellRect);
    }
}