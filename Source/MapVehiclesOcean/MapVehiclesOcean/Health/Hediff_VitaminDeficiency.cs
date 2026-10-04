using Verse;

namespace MapVehiclesOcean;

public class Hediff_VitaminDeficiency : HediffWithComps
{
	public override void Notify_IngestedThing(Thing thing, int amount)
	{
		this.TryGetComp<HediffComp_VitaminC>()?.Notify_IngestedThing(thing, amount);
	}
}