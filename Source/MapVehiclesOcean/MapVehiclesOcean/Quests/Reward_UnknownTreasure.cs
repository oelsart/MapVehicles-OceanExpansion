using RimWorld;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

public class Reward_UnknownTreasure : Reward_Unknown
{
    public override IEnumerable<GenUI.AnonymousStackElement> StackElements
    {
        get
        {
            yield return QuestPartUtility.GetStandardRewardStackElement("Reward_Unknown_Label".Translate(), Icon, () => GetDescription(default).CapitalizeFirst() + ".");
        }
    }
    
    private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("MapVehiclesOcean/UI/Icons/UnknownTreasure");
}