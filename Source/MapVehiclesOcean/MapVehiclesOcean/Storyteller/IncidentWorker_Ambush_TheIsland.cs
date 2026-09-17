using RimWorld;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

public class IncidentWorker_Ambush_TheIsland : IncidentWorker_Ambush_EnemyBoats
{
	private const float MaxDistance = 5f;
	private const float PointMultiplier = 1.5f;
	
	protected override bool CanFireNowSub(IncidentParms parms)
	{
		return Find.World.GetComponent<HiddenIslandManager>().SpecialIslandTile is { Valid: true } tile &&
		       Find.WorldGrid.ApproxDistanceInTiles(parms.target.Tile, tile) <= MaxDistance &&
		       base.CanFireNowSub(parms);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		parms.points = Mathf.Min(parms.points * PointMultiplier, StorytellerUtility.GlobalPointsMax);
		return base.TryExecuteWorker(parms);
	}
}