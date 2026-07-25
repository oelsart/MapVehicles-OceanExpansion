using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class LayoutWorker_MechanoidPlatform(LayoutDef def) : LayoutWorker_MaritimeBase(def)
{
  protected override IntRange RoomSizeRange => new(8, 64);
  
  protected override IntRange RoomCountRange => new(3, 5);
  
  protected override IntRange RoomSpacingRange => new(4, 6);
  
  protected override LayoutSketch GenerateSketch(StructureGenParams parms)
  {
    var sketch = new LayoutSketchComplexTerrain
    {
      floor = MVO_DefOf.MVO_MechanoidPlatformSub,
      importantFloor = MVO_DefOf.MVO_MechanoidPlatform,
      importantFloorSpacing = 1,
      defaultAffordanceTerrain = MVO_DefOf.MVO_WaterOceanDeepPassable,
      structureLayout = GenerateBase(parms)
    };

    return sketch;
  }

  protected override StructureLayout GenerateBase(StructureGenParams parms)
  {
    var layout = base.GenerateBase(parms);
    
    foreach (var room in layout.Rooms)
    {
      foreach (var rect in room.rects)
      {
        foreach (var cell in rect)
        {
          layout.Add(cell, RoomLayoutCellType.Floor);
        }
      }
    }
    return layout;
  }
}