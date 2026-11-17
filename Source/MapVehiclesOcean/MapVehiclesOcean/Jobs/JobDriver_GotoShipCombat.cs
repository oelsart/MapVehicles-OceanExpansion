using Vehicles;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

public class JobDriver_GotoShipCombat : JobDriver_Goto
{
  protected override IEnumerable<Toil> MakeNewToils()
  {
    this.FailOn(() => job.GetTarget(TargetIndex.A).Thing is Pawn { ParentHolder: Corpse });
    this.FailOn(() => job.GetTarget(TargetIndex.A).Thing is { Destroyed: true });
    var toil = ToilMaker.MakeToil();
    toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
    toil.tickAction += () =>
    {
      if (!toil.actor.IsHashIntervalTick(300)) return;
      
      if (toil.actor is not VehiclePawn { CanMoveFinal: true } vehicle ||
          !CombatPositionUtility.TryFindShipCombatPosition(vehicle, out var dest, out var endRot))
	      return;

      var curTarget = vehicle.jobs.curJob.GetTarget(TargetIndex.B);
      if (curTarget != dest)
      {
        vehicle.jobs.curJob.SetTarget(TargetIndex.B, dest);
        if (vehicle.Position == dest)
        {
          vehicle.jobs.curDriver.ReadyForNextToil();
          return;
        }
        
        vehicle.vehiclePather.StartPath(dest, PathEndMode.OnCell);
        vehicle.vehiclePather.SetEndRotation(endRot);
      }
    };
    yield return toil;
  }
}