using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace MapVehiclesOcean;

public class QuestNode_UnknownTreasureReward : QuestNode
{
    [NoTranslate]
    public SlateRef<string> inSignalChoiceUsed;

    protected override bool TestRunInt(Slate slate) => true;

    protected override void RunInt()
    {
        var slate = QuestGen.slate;
        QuestGen.quest.AddPart(new QuestPart_Choice
        {
            inSignalChoiceUsed = QuestGenUtility.HardcodedSignalWithQuestID(inSignalChoiceUsed.GetValue(slate)),
            choices = 
            [
                new QuestPart_Choice.Choice
                {
                    rewards = [new Reward_UnknownTreasure()]
                }
            ]
        });
    }
}