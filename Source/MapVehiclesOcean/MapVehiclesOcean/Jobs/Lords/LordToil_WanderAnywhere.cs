using Verse.AI;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class LordToil_WanderAnywhere : LordToil
{
    public override void UpdateAllDuties()
    {
        for (var i = 0; i < lord.ownedPawns.Count; i++)
        {
            var pawnDuty = new PawnDuty(MVO_DefOf.MVO_WanderAnywhere);
            lord.ownedPawns[i].mindState.duty = pawnDuty;
        }
    }
}