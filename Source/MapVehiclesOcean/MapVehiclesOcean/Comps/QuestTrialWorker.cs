using Verse;

namespace MapVehiclesOcean;

public class QuestTrialWorker
{
	public virtual AcceptanceReport CanInteract(CompInteractableQuest comp)
	{
		return AcceptanceReport.WasAccepted;
	}
}