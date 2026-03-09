using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class QuestNode_SignalCount : QuestNode
{
    [NoTranslate]
    [TranslationHandle(Priority = 100)]
    public SlateRef<string> inSignal;
    [NoTranslate]
    public SlateRef<string> inSignalDisable;
    [NoTranslate]
    public SlateRef<IEnumerable<string>> outSignals;
    public QuestNode node;
    public SlateRef<QuestPart.SignalListenMode?> signalListenMode;
    private const string OuterNodeCompletedSignal = "OuterNodeCompleted";
    
    protected override bool TestRunInt(Slate slate)
    {
        return node == null || node.TestRun(slate);
    }
    
    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        switch ((outSignals.GetValue(slate) != null ? outSignals.GetValue(slate).Count() : 0) + (node != null ? 1 : 0))
        {
            case 0:
                break;
            case 1:
                var part1 = new QuestPart_CountPass
                {
                    inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)),
                    inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate))
                };
                if (node != null)
                {
                    part1.outSignal = QuestGen.GenerateNewSignal(OuterNodeCompletedSignal);
                    QuestGenUtility.RunInnerNode(node, part1.outSignal);
                }
                else
                    part1.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignals.GetValue(slate).First());
                part1.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
                QuestGen.quest.AddPart(part1);
                break;
            default:
                var part2 = new QuestPart_PassOutMany
                {
                    inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)),
                    inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate))
                };
                if (node != null)
                {
                    var newSignal = QuestGen.GenerateNewSignal(OuterNodeCompletedSignal);
                    part2.outSignals.Add(newSignal);
                    QuestGenUtility.RunInnerNode(node, newSignal);
                }
                foreach (var signal in outSignals.GetValue(slate))
                    part2.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(signal));
                part2.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
                QuestGen.quest.AddPart(part2);
                break;
        }
    }
}