using RimWorld;
using Verse;

namespace MapVehicles;

public class CompCrowsNest : CompScanner
{
    protected override void DoFind(Pawn worker)
    {
        Find.LetterStack.ReceiveLetter("DummyLetter", "DummyText", LetterDefOf.PositiveEvent);
    }
}