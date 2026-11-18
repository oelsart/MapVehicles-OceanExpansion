using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace MapVehiclesOcean;

public class CutsceneController(World world) : WorldComponent(world)
{
	private static readonly AccessTools.FieldRef<bool> cutsceneInProgress = AccessTools.StaticFieldRefAccess<bool>(
			AccessTools.Field(typeof(WorldComponent_GravshipController), "cutsceneInProgress"));

	private Action<float> CutsceneUpdate;
	private Action OnCutsceneEnd;
	private float startTime;
	private float duration;
	
	public void BeginCutscene(Action onCutsceneStart, Action<float> cutsceneUpdate, Action onCutsceneEnd, float _duration)
	{
		CutsceneUpdate = cutsceneUpdate;
		OnCutsceneEnd = onCutsceneEnd;
		duration = _duration;
		startTime = RealTime.LastRealTime;
		
		// 独自のカットシーンフラグを持つと複数個所へのパッチが必要となるため、
		// WorldComponent_GravshipControllerのカットシーンフラグを借りている。
		cutsceneInProgress() = true;
		onCutsceneStart?.Invoke();
	}

	public override void WorldComponentUpdate()
	{
		if (!cutsceneInProgress()) return;
		var elapsed = RealTime.LastRealTime - startTime;
		if (elapsed >= duration)
		{
			cutsceneInProgress() = false;
			OnCutsceneEnd?.Invoke();
			Clear();
			return;
		}
		CutsceneUpdate?.Invoke(elapsed);
	}

	private void Clear()
	{
		CutsceneUpdate = null;
		OnCutsceneEnd = null;
		duration = -1f;
		startTime = -1f;
	}
}