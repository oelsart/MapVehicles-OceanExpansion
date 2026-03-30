using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class LordJob_MaritimeBasePawns(Faction faction) : LordJob
{
    private static readonly IntRange Delay = new (2500, 5000);
    private Faction faction = faction;
    
    [UsedImplicitly]
    public LordJob_MaritimeBasePawns() : this(null)
    {
    }

    public override StateGraph CreateGraph()
    {
        var hostile = faction.HostileTo(Faction.OfPlayer);
        var stateGraph = new StateGraph();
        LordToil startingToil = hostile ? new LordToil_DefendBase(Map.Center) : new LordToil_WanderAnywhere();
        stateGraph.StartingToil = startingToil;
        var lordToil_HuntDownColonists = new LordToil_HuntDownColonists();
        stateGraph.AddToil(lordToil_HuntDownColonists);
        var transition = new Transition(startingToil, lordToil_HuntDownColonists);
        transition.AddTrigger(new Trigger_FractionPawnsLost(0.2f));
        transition.AddTrigger(new Trigger_PawnHarmed(0.4f));
        transition.AddTrigger(new Trigger_ChanceOnPlayerHarmNPCBuilding(0.4f));
        transition.AddTrigger(new Trigger_OnClamor(ClamorDefOf.Ability));
        transition.AddPostAction(new TransitionAction_WakeAll());
        if (hostile)
        {
            transition.AddTrigger(new Trigger_ChanceOnTickInterval(2500, 0.03f));
            transition.AddTrigger(new Trigger_TicksPassed(Delay.RandomInRange));
            transition.AddTrigger(new Trigger_UrgentlyHungry());
        }
        var taggedString = faction.def.messageDefendersAttacking.Formatted(faction.def.pawnsPlural, faction.Name, Faction.OfPlayer.def.pawnsPlural).CapitalizeFirst();
        transition.AddPreAction(new TransitionAction_Message(taggedString, MessageTypeDefOf.ThreatBig));
        stateGraph.AddTransition(transition);
        return stateGraph;
    }
    

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref faction, "faction");
    }
}