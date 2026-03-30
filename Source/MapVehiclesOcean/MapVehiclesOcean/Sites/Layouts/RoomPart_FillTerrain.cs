using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class RoomPart_FillTerrain(RoomPartDef def) : RoomPartWorker(def)
{
    public new RoomPart_TerrainDef def => (RoomPart_TerrainDef)base.def;

    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
    {
        foreach (var rect in room.rects)
        {
            foreach (var c in rect)
            {
                map.terrainGrid.SetTerrain(c, def.terrain);
            }
        }
    }
}