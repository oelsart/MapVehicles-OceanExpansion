using RimWorld;

namespace MapVehiclesOcean;

public class SitePartWorker_MarineSite : SitePartWorker
{
    public override bool FactionCanOwn(Faction faction)
    {
        return faction is null || faction.def == MVO_DefOf.MVO_Maritime;
    }
}