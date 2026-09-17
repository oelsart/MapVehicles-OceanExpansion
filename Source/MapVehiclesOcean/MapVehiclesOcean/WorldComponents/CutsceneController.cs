using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace MapVehiclesOcean;

public class CutsceneController(World world) : WorldComponent(world)
{
	private static readonly AccessTools.FieldRef<bool> cutsceneInProgress = AccessTools.StaticFieldRefAccess<bool>(
			AccessTools.Field(typeof(WorldComponent_GravshipController), "cutsceneInProgress"));

	private Action CutsceneUpdate;
	
	public void BeginCutscene(Action onCutsceneStart, Action cutsceneUpdate, Action onCutsceneEnd, float duration)
	{
		CutsceneUpdate = cutsceneUpdate;
		
		// 独自のカットシーンフラグを持つと複数個所へのパッチが必要となるため、
		// WorldComponent_GravshipControllerのカットシーンフラグを借りている。
		cutsceneInProgress() = true;
		onCutsceneStart?.Invoke();
		Delay.AfterNSeconds(duration, () =>
		{
			onCutsceneEnd?.Invoke();
			CutsceneUpdate = null;
			cutsceneInProgress() = false;
		});
	}

	public override void WorldComponentUpdate()
	{
		if (!cutsceneInProgress()) return;
		CutsceneUpdate?.Invoke();
	}
}