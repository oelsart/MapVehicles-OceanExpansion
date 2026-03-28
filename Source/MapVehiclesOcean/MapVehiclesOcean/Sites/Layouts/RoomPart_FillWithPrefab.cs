using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class RoomPart_FillWithPrefab(RoomPartDef def) : RoomPartWorker(def)
{
    public new RoomPart_PrefabDef def => (RoomPart_PrefabDef)base.def;

    public override bool FillOnPost => def.fillOnPost;

    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
    {
        if (def.prefab.edgeOnly)
            ExtendedRoomGenUtility.FillPrefabsAroundEdges(def, room, map);
        else ExtendedRoomGenUtility.FillPrefabs(def, room, map);
    }
}