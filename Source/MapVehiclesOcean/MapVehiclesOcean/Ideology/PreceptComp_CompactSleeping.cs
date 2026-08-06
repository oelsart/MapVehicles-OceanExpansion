using RimWorld;

namespace MapVehiclesOcean;

public class PreceptComp_CompactSleeping : PreceptComp_DisableBadMemory
{
  protected override ThoughtDef ThoughtDef => ThoughtDefOf.SleptInBarracks;
}