using JetBrains.Annotations;
using RimWorld;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_FireOverlayQuest : CompProperties_FireOverlay
{
	public string questTag;

	public CompProperties_FireOverlayQuest()
	{
		compClass = typeof(CompFireOverlayQuest);
	}
}