using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class RoomPart_ConnectConduits(RoomPartDef def) : RoomPartWorker(def)
{
    public override bool FillOnPost => true;

    public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
    {
        var outerRect = room.Boundary.ExpandedBy(1);
        foreach (var c in outerRect.EdgeCells)
        {
            if (GenConstruct.CanPlaceBlueprintAt(ThingDefOf.HiddenConduit, c, Rot4.North, map))
                GenSpawn.Spawn(ThingDefOf.HiddenConduit, c, map)?.SetFaction(faction);
        }
        
        map.powerNetManager.UpdatePowerNetsAndConnections_First();
        var powerNetGrid = map.powerNetGrid;
        var outerNet = powerNetGrid.TransmittedPowerNetAt(outerRect.Min);
        if (outerNet is null) return;

        foreach (var c in room.Boundary.ContractedBy(1))
        {
            var innerNet = powerNetGrid.TransmittedPowerNetAt(c);
            if (innerNet is null || innerNet == outerNet) continue;

            var rot = outerRect.GetClosestEdge(c);
            var cell = c + rot.FacingCell;
            while (outerRect.Contains(cell))
            {
                if (GenConstruct.CanPlaceBlueprintAt(ThingDefOf.HiddenConduit, cell, Rot4.North, map))
                    GenSpawn.Spawn(ThingDefOf.HiddenConduit, cell, map)?.SetFaction(faction);
                cell += rot.FacingCell;
            }
        }
    }
}