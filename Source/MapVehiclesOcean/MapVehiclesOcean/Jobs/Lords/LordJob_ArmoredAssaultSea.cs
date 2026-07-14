using RimWorld;
using VehicleMapFramework;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class LordJob_ArmoredAssaultSea : LordJob_VehicleNPC
{
  private static readonly IntRange AssaultTimeBeforeGiveUp = new (26000, 38000);

  private static readonly IntRange SapTimeBeforeGiveUp = new (33000, 38000);

  private Faction assaulterFaction;

  private LordJob_ArmoredAssault.RaiderPermissions permission = LordJob_ArmoredAssault.RaiderPermissions.All;
  private LordJob_ArmoredAssault.RaiderBehavior behavior = LordJob_ArmoredAssault.RaiderBehavior.None;

  public LordJob_ArmoredAssaultSea()
  {
  }

  public LordJob_ArmoredAssaultSea(SpawnedPawnParams parms)
  {
    assaulterFaction = parms.spawnerThing.Faction;
    permission = LordJob_ArmoredAssault.RaiderPermissions.All;
  }

  public LordJob_ArmoredAssaultSea(Faction assaulterFaction,
    LordJob_ArmoredAssault.RaiderPermissions permission)
  {
    this.assaulterFaction = assaulterFaction;
    this.permission = permission;
  }

  public override float MaxVehicleSpeed => 4;

  public override bool GuiltyOnDowned => true;

  public override bool AddFleeToil => false;

  public override StateGraph CreateGraph()
  {
    StateGraph stateGraph = new();

    // Main assault loop
    LordToil assaultColonyToil = new LordToil_AssaultColonyArmoredSea();

    stateGraph.AddToil(assaultColonyToil);
    LordToil_ExitMapVehicle exitMapToil = new(LocomotionUrgency.Jog, interruptCurrentJob: true);
    exitMapToil.useAvoidGrid = true;
    stateGraph.AddToil(exitMapToil);

    if (assaulterFaction.def.humanlikeFaction)
    {
      // Exit map promptly
      AddTimeoutOrFleeToil(stateGraph, assaultColonyToil, exitMapToil);
      // Kidnap someone and leave
      AddKidnapToil(stateGraph, assaultColonyToil);
      // Steal stuff and leave
      AddCanStealToil(stateGraph, assaultColonyToil);
    }

    // Exit map leisurely (Non-hostile)
    Transition leaveMapTransition = new(assaultColonyToil, exitMapToil);
    leaveMapTransition.AddTrigger(new Trigger_BecameNonHostileToPlayer());
    leaveMapTransition.AddPreAction(new TransitionAction_Message(
      "MessageRaidersLeaving".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
        assaulterFaction.Name)));
    stateGraph.AddTransition(leaveMapTransition);

    return stateGraph;
  }

  private void AddTimeoutOrFleeToil(StateGraph stateGraph, LordToil assaultColonyToil, LordToil exitMapToil)
  {
    if (!permission.canTimeoutOrFlee) return;

    Transition giveUpAndLeaveTransition = new(assaultColonyToil, exitMapToil);
    //giveUpAndLeaveTransition.AddTrigger(new Trigger_TicksPassed(sappers ? SapTimeBeforeGiveUp.RandomInRange : AssaultTimeBeforeGiveUp.RandomInRange));
    giveUpAndLeaveTransition.AddPreAction(new TransitionAction_Message(
      "MessageRaidersGivenUpLeaving".Translate(
        assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
    stateGraph.AddTransition(giveUpAndLeaveTransition);
    
    Transition satisfiedLeaveTransition = new(assaultColonyToil, exitMapToil);
    var desiredColonyDamagePct = new FloatRange(0.5f, 0.75f).RandomInRange;
    satisfiedLeaveTransition.AddTrigger(
      new Trigger_FractionColonyDamageTaken(desiredColonyDamagePct, 900f));
    satisfiedLeaveTransition.AddPreAction(new TransitionAction_Message(
      "MessageRaidersSatisfiedLeaving".Translate(
        assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
    stateGraph.AddTransition(satisfiedLeaveTransition);
    
    var fleeTransition = new Transition(assaultColonyToil, exitMapToil);
    fleeTransition.AddPreAction(new TransitionAction_Message(
      "MessageFightersFleeing".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(), assaulterFaction.Name)));
    fleeTransition.AddTrigger(new Trigger_FractionPawnsLost(
      assaulterFaction.def.attackersDownPercentageRangeForAutoFlee.RandomInRangeSeeded(lord.loadID)));
    fleeTransition.AddPostAction(new TransitionAction_Custom(
      () => QuestUtility.SendQuestTargetSignals(lord.questTags, "Fleeing", lord.Named("SUBJECT"))));
    stateGraph.AddTransition(fleeTransition, true);
  }

  private void AddKidnapToil(StateGraph stateGraph, LordToil assaultColonyToil)
  {
    if (!permission.canKidnap) return;

    var kidnapToil =
      stateGraph.AttachSubgraph(new LordJob_KidnapToSeaMapVehicle().CreateGraph()).StartingToil;
    Transition kidnapAndLeaveTransition = new(assaultColonyToil, kidnapToil);

    kidnapAndLeaveTransition.AddPreAction(new TransitionAction_Message(
      "MessageRaidersKidnapping".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
        assaulterFaction.Name)));
    kidnapAndLeaveTransition.AddTrigger(new Trigger_KidnapVictimPresentCrossMap());
    stateGraph.AddTransition(kidnapAndLeaveTransition);
  }

  private void AddCanStealToil(StateGraph stateGraph, LordToil assaultColonyToil)
  {
    if (!permission.canSteal) return;

    var stealThingToil =
      stateGraph.AttachSubgraph(new LordJob_StealToSeaMapVehicle().CreateGraph()).StartingToil;
    Transition stealThingTransition = new(assaultColonyToil, stealThingToil);

    stealThingTransition.AddPreAction(new TransitionAction_Message(
      "MessageRaidersStealing".Translate(assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
        assaulterFaction.Name)));
    stealThingTransition.AddTrigger(new Trigger_HighValueThingsAroundCrossMap());
    stateGraph.AddTransition(stealThingTransition);
  }

  public override void ExposeData()
  {
    Scribe_References.Look(ref assaulterFaction, nameof(assaulterFaction));
    Scribe_Deep.Look(ref permission, nameof(permission));
    Scribe_Values.Look(ref behavior, nameof(behavior));
  }
}