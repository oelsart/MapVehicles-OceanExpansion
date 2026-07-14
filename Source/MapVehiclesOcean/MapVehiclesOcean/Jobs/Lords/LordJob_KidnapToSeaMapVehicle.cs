using RimWorld;
using VehicleMapFramework;
using Verse.AI;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class LordJob_KidnapToSeaMapVehicle : LordJob_Kidnap
{
  public override StateGraph CreateGraph()
  {
    var stateGraph = new StateGraph();
    
    var lordToilKidnapToSeaMapVehicle = new LordToil_KidnapToSeaMapVehicle();
    lordToilKidnapToSeaMapVehicle.useAvoidGrid = true;
    stateGraph.AddToil(lordToilKidnapToSeaMapVehicle);
    
    var lordToilKidnapToSeaMapVehicle2 = new LordToil_KidnapToSeaMapVehicle();
    lordToilKidnapToSeaMapVehicle2.cover = false;
    lordToilKidnapToSeaMapVehicle2.useAvoidGrid = true;
    stateGraph.AddToil(lordToilKidnapToSeaMapVehicle2);
    
    var transition = new Transition(lordToilKidnapToSeaMapVehicle, lordToilKidnapToSeaMapVehicle2);
    transition.AddTrigger(new Trigger_TicksPassed(1200));
    stateGraph.AddTransition(transition);
    
    var lordToil_ExitMap = new LordToil_ExitMapVehicle(LocomotionUrgency.Jog, interruptCurrentJob: true);
    stateGraph.AddToil(lordToil_ExitMap);
    
    var transition2 = new Transition(lordToilKidnapToSeaMapVehicle2, lordToil_ExitMap);
    transition2.AddTrigger(new Trigger_TicksPassed(1200));
    stateGraph.AddTransition(transition2);
    
    return stateGraph;
  }
}