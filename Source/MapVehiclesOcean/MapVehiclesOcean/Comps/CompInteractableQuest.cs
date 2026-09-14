using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class CompInteractableQuest : CompInteractable
{
	protected new CompProperties_InteractableQuest Props => (CompProperties_InteractableQuest)props;
	protected QuestTrialWorker TrialWorker { get; private set; }

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		TrialWorker = (QuestTrialWorker)Activator.CreateInstance(Props.trialWorkerType);
	}

	public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
	{
		var report = base.CanInteract(activateBy, checkOptionalItems);
		return !report.Accepted ? report : TrialWorker.CanInteract(this);
	}

	public override string GetDescriptionPart()
	{
		return "\n\n" + (Active ? Props.descPartActive : Props.descPartInactive);
	}
}