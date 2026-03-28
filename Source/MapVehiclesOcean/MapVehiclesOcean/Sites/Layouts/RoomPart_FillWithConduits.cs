using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class RoomPart_FillWithConduits(RoomPartDef def) : RoomPartWorker(def)
{
    public override bool FillOnPost => true;

    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
    {
        foreach (var rect in room.rects)
        {
            foreach (var c in rect)
                GenSpawn.Spawn(ThingDefOf.HiddenConduit, c, map)?.SetFaction(faction);
        }
    }
}