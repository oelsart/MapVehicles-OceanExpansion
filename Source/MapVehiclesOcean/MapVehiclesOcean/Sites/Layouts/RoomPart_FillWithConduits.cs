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
            {
                var conduit = ThingMaker.MakeThing(ThingDefOf.HiddenConduit);
                conduit.SetFactionDirect(faction);
                GenSpawn.Spawn(conduit, c, map);
            }
        }
    }
}