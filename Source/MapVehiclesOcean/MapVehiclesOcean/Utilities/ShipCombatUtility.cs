using RimWorld;

namespace MapVehiclesOcean;

public class ShipCombatUtility
{
	public static PawnGroupKindDef PawnGroupKindFor(Faction faction)
	{
		if (faction?.def.pawnGroupMakers is not { } makers)
			return PawnGroupKindDefOf.Combat;

		return makers.Exists(m => m.kindDef == MVO_DefOf.MVO_ShipCombat)
			? MVO_DefOf.MVO_ShipCombat
			: PawnGroupKindDefOf.Combat;
	}
}