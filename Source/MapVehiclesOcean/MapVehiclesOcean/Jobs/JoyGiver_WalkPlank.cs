using RimWorld;
using RimWorld.Planet;
using VehicleMapFramework;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

public class JoyGiver_WalkPlank : JoyGiver_InteractBuilding
{
  protected override Job TryGivePlayJob(Pawn pawn, Thing t)
  {
    if (t.TryGetComp<CompGangplank>() is null or { AvailableAccessSpot.IsValid: true })
      return null;
    
    if (t.Position.Standable(t.Map) && !t.Position.IsForbidden(pawn, t.Map) &&
        TryFindBestSpot(pawn, t, out var spot))
    {
      var job = JobMaker.MakeJob(def.jobDef, t, spot);
      job.globalTarget = new GlobalTargetInfo(spot, t.Map);
      return job;
    }

    return null;
  }

  private static bool TryFindBestSpot(Pawn pawn, Thing t, out IntVec3 spot)
  {
    var adj = GenAdj.CardinalDirectionsAndInside;
    for (var i = adj.Length - 1; i >= 0; i--)
    {
      var c = t.Position + adj[i];
      if (c.Standable(t.Map) && !c.IsForbidden(pawn, t.Map) &&
          !t.Map.pawnDestinationReservationManager.IsReserved(c) &&
          t.Map.reservationManager.CanReserve(pawn, c))
      {
        spot = c;
        return true;
      }
    }

    spot = IntVec3.Invalid;
    return false;
  }
}