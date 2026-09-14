using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class CompFireOverlayQuest : CompFireOverlayBase, IThingGlower
{
	protected new CompProperties_FireOverlayQuest Props => (CompProperties_FireOverlayQuest)props;

	bool IThingGlower.ShouldBeLitNow() => startedGrowingAtTick >= 0;

	public override void Notify_SignalReceived(Signal signal)
	{
		if (signal.tag == Props.questTag)
		{
			startedGrowingAtTick = GenTicks.TicksAbs;
			parent.BroadcastCompSignal(CompFlickable.FlickedOnSignal);
		}
	}
	
	public override void PostDraw()
	{
		if (startedGrowingAtTick < 0)
			return;

		var comps = parent.AllComps;
		comps.Remove(this);
		comps.Insert(0, this);
		var drawPos = parent.DrawPos;
		drawPos.y += 0.03658537f;
		CompFireOverlay.FireGraphic.Draw(drawPos, parent.Rotation, parent);
	}
}