using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class QuestNode_KillPawns : QuestNode
{
    public SlateRef<float> chance;
    public SlateRef<IntRange> corpseAgeRangeDays;
    public SlateRef<Pawn> pawn;
    public SlateRef<IEnumerable<Pawn>> pawns;

    protected override bool TestRunInt(Slate slate)
    {
        return pawn.TryGetValue(slate, out _) || pawns.TryGetValue(slate, out _);
    }

    protected override void RunInt()
    {
        if (chance.TryGetValue(QuestGen.slate, out var chance2)) return;
        var age = corpseAgeRangeDays.TryGetValue(QuestGen.slate, out var range)
            ? range.RandomInRange * 60000
            : 1800000;
        if (pawn.TryGetValue(QuestGen.slate, out var pawn2) && Rand.Chance(chance2))
            KillPawn(pawn2, age);
        if (pawns.TryGetValue(QuestGen.slate, out var pawns2))
            foreach (var pawn3 in pawns2)
                if (Rand.Chance(chance2)) KillPawn(pawn3, age);
    }

    private static void KillPawn(Pawn pawnInt, int age)
    {
        pawnInt.health.SetDead();
        pawnInt.apparel.DestroyAll();
        pawnInt.equipment.DestroyAllEquipment();
        Find.WorldPawns.PassToWorld(pawnInt, PawnDiscardDecideMode.Discard);
        var corpse = pawnInt.MakeCorpse(null, null);
        corpse.Age = age;
        corpse.GetComp<CompRottable>().RotProgress += corpse.Age;
    }
}