using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class QuestTrialWorker_Athena : QuestTrialWorker
{
	public override AcceptanceReport CanInteract(CompInteractableQuest comp)
	{
		return comp.parent.GetRoom().ContainedThings<Pawn>()
			.Where(p => p.HostileTo(Faction.OfPlayer)).Any(p => !p.Downed && !p.Dead)
			? "MVO_Athena_NotDefeated".Translate()
			: AcceptanceReport.WasAccepted;
	}
}