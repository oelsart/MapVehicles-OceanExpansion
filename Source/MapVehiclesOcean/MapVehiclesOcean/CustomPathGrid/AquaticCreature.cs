using Verse;

namespace MapVehiclesOcean;

[StaticConstructorOnStartup]
public class AquaticCreature : DefModExtension
{
	private static readonly HashSet<ushort> allHashes = [];
	
	public static bool IsAquatic(Pawn pawn) => allHashes.Contains(pawn.kindDef.shortHash);

	public override void ResolveReferences(Def parentDef)
	{
		LongEventHandler.ExecuteWhenFinished(() => allHashes.Add(parentDef.shortHash));
	}
}