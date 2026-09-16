using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class QuestTrialWorker_Midas : QuestTrialWorker
{
	private readonly ThingDef MVO_Bowl = DefDatabase<ThingDef>.GetNamed("MVO_Bowl");
	private const int GoldRequirement = 2800;
	
	public override AcceptanceReport CanInteract(CompInteractableQuest comp)
	{
		var amount = comp.parent.GetRoom().ContainedThings(MVO_Bowl).Cast<Building_Storage>()
			.SelectMany(b => b.GetSlotGroup().HeldThings.Where(t => t.def== ThingDefOf.Gold))
			.Sum(t => t.stackCount);

		return amount < GoldRequirement ? "MVO_Midas_NotFulfilled".Translate(amount) : AcceptanceReport.WasAccepted;
	}
}