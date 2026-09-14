using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class SignalAction_SpawnThing : SignalAction
{
	public ThingDef thingDef;
	public ThingDef stuff;

	protected override void DoAction(SignalArgs args)
	{
		var thing = ThingMaker.MakeThing(thingDef, stuff);
		GenSpawn.Spawn(thing, Position, Map, Rotation);
	}
}