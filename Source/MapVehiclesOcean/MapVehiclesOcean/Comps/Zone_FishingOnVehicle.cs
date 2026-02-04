using HarmonyLib;
using RimWorld;
using Verse;

namespace MapVehicles;

public class Zone_FishingOnVehicle : Zone_Fishing
{
    private static readonly AccessTools.FieldRef<Zone_Fishing, bool> allowed = AccessTools.FieldRefAccess<Zone_Fishing, bool>("allowed");
    
    public Zone_FishingOnVehicle()
    {
    }
    
    public Zone_FishingOnVehicle(ZoneManager zoneManager) : base(zoneManager)
    {
    }

    public override IEnumerable<InspectTabBase> GetInspectTabs()
    {
        if (cells.Empty() || cells[0].GetWaterBody(Map) is null)
            yield break;
        
        foreach (var tab in base.GetInspectTabs())
            yield return tab;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Hide_ZoneFishing(this);
        yield return new Command_Toggle
        {
            defaultLabel = "CommandAllowFish".Translate(),
            defaultDesc = "CommandAllowFishDesc".Translate(),
            icon = TexCommand.ForbidOff,
            hotKey = KeyBindingDefOf.Command_ItemForbid,
            isActive = () => allowed(this),
            toggleAction = delegate
            {
                allowed(this) = !allowed(this);
            }
        };
    }
}