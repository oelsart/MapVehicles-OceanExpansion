using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class QuestPart_CountPass : QuestPartCanDisable
{
    public string inSignal;
    public string outSignal;
    private int count;
    public QuestEndOutcome? outSignalOutcomeArg;

    protected override void ProcessQuestSignal(Signal signal)
    {
        if (signal.tag != inSignal)
            return;
        var args = new SignalArgs(signal.args);
        if (outSignalOutcomeArg.HasValue)
            args.Add(outSignalOutcomeArg.Value.Named("OUTCOME"));
        Find.SignalManager.SendSignal(new Signal($"{outSignal}.{++count}", args));
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref inSignal, "inSignal");
        Scribe_Values.Look(ref outSignal, "outSignal");
        Scribe_Values.Look(ref outSignalOutcomeArg, "outSignalOutcomeArg");
        Scribe_Values.Look(ref count, "count");
    }

    public override void AssignDebugData()
    {
        base.AssignDebugData();
        inSignal = "DebugSignal" + Rand.Int;
        outSignal = "DebugSignal" + Rand.Int;
    }
}