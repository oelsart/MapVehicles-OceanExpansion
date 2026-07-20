using Verse;

namespace MapVehiclesOcean;

public class PawnRenderNodeWorker_Swim : PawnRenderNodeWorker
{
  public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
  {
    return parms.swimming && base.CanDrawNow(node, parms);
  }
}