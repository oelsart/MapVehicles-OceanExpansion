using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class RoomPart_FillBatteries(RoomPartDef def) : RoomPartWorker(def)
{
    public override bool FillOnPost => true;

    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
    {
        foreach (var rect in room.rects)
        {
            foreach (var c in rect)
            {
                foreach (var thing in c.GetThingList(map))
                {
                    if (thing.TryGetComp<CompPowerBattery>() is { } compBattery)
                    {
                        compBattery.SetStoredEnergyPct(1f);
                    }
                }
            }
        }
    }
}