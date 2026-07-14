using RimWorld;
using VehicleMapFramework;
using Verse.AI;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class LordJob_StealToSeaMapVehicle : LordJob_Steal
{
  public override StateGraph CreateGraph()
  {
    var stateGraph = new StateGraph();
    
    var lordToilStealToSeaMapVehicle = new LordToil_StealToSeaMapVehicle();
    lordToilStealToSeaMapVehicle.useAvoidGrid = true;
    stateGraph.AddToil(lordToilStealToSeaMapVehicle);
    
    var lordToilStealToSeaMapVehicle2 = new LordToil_StealToSeaMapVehicle();
    lordToilStealToSeaMapVehicle2.cover = false;
    lordToilStealToSeaMapVehicle2.useAvoidGrid = true;
    stateGraph.AddToil(lordToilStealToSeaMapVehicle2);
    
    var transition = new Transition(lordToilStealToSeaMapVehicle, lordToilStealToSeaMapVehicle2);
    transition.AddTrigger(new Trigger_TicksPassed(1200));
    stateGraph.AddTransition(transition);
    
    var lordToil_ExitMap = new LordToil_ExitMapVehicle(LocomotionUrgency.Jog, interruptCurrentJob: true);
    stateGraph.AddToil(lordToil_ExitMap);
    var transition2 = new Transition(lordToilStealToSeaMapVehicle2, lordToil_ExitMap);
    
    transition2.AddTrigger(new Trigger_TicksPassed(1200));
    stateGraph.AddTransition(transition2);
    return stateGraph;
  }
}