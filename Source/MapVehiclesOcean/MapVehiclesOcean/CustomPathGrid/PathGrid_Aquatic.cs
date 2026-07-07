using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

public class PathGrid_Aquatic(Map map, PathGridDef def) : PathGrid(map, def)
{
  public override int CalculatedCostAt(IntVec3 c, bool perceivedStatic, IntVec3 prevCell, int? baseCostOverride = null)
  {
    return c.GetTerrain(map).IsWater || !perceivedStatic
      ? base.CalculatedCostAt(c, perceivedStatic, prevCell, baseCostOverride)
      : ImpassableCost;
  }
}