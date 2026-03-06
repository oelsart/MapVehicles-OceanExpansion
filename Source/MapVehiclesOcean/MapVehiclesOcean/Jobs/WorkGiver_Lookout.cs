using RimWorld;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

public class WorkGiver_Lookout : WorkGiver_OperateScanner
{
    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(MVO_DefOf.MVO_JobLookout, t, 1500, true);
    }
}