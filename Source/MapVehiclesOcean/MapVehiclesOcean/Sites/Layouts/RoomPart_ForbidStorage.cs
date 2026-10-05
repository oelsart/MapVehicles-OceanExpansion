using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class RoomPart_ForbidStorage(RoomPartDef def) : RoomPartWorker(def)
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
					if (thing is Building_Storage storage)
					{
						storage.settings?.Priority = StoragePriority.Unstored;
					}
				}
			}
		}
	}
}