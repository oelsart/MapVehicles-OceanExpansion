using RimWorld;
using Verse;
using Verse.AI;

namespace MapVehicles;

public class WorkGiver_Lookout : WorkGiver_OperateScanner
{
    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(MVO_DefOf.MV_JobLookout, t, 1500, true);
    }
}