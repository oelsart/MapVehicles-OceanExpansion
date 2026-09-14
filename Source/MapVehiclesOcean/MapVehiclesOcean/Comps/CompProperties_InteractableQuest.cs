using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_InteractableQuest : CompProperties_Interactable
{
	[MustTranslate] public string descPartActive;
	[MustTranslate] public string descPartInactive;
	public Type trialWorkerType = typeof(QuestTrialWorker);
	
	public CompProperties_InteractableQuest()
	{
		compClass = typeof(CompInteractableQuest);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (var error in base.ConfigErrors(parentDef)) yield return error;
		if (trialWorkerType is null || !trialWorkerType.SameOrSubclassOf<QuestTrialWorker>())
			yield return "trialWorkerType must be a subclass of QuestTrialWorker.";
	}
}