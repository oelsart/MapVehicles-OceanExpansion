using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class RoomPart_OuterCornerThings(RoomPartDef def) : RoomPartWorker(def)
{
    public new RoomPart_ThingDef def => (RoomPart_ThingDef)base.def;

    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
    {
        foreach (var c in room.Boundary.ExpandedBy(1).Corners)
        {
            var flag = false;
            foreach (var c2 in GenAdjFast.AdjacentCells8Way(c))
            {
                if (room.sketch.AnyRoomContains(c2, room))
                {
                    flag = true;
                    break;
                }
            }
            if (flag) continue;
            var thing = ThingMaker.MakeThing(def.thingDef, def.stuffDef);
            thing.SetFactionDirect(faction);
            GenSpawn.Spawn(thing, c, map);
        }
    }
}