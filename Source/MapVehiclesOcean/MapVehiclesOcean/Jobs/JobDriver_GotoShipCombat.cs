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
    toil.defaultCompleteMode = ToilCompleteMode.Never;
    toil.tickAction += () =>
    {
      if (!toil.actor.IsHashIntervalTick(300)) return;
      
      if (toil.actor is not VehiclePawn vehicle ||
          !CombatPositionUtility.TryFindShipCombatPosition(vehicle, out var dest, out var endRot))
	      return;

      var curTarget = vehicle.jobs.curJob.GetTarget(TargetIndex.A);
      if (curTarget != dest)
      {
        vehicle.jobs.curJob.SetTarget(TargetIndex.A, dest);
        if (vehicle.Position == dest)
        {
          vehicle.jobs.curDriver.ReadyForNextToil();
          return;
        }
        
	      var data = new PathOrderData
	      {
		      destination = dest,
		      endRotation = endRot
	      };
	      vehicle.vehiclePather.TryOrderMoveTo(in data);
      }
    };
    yield return toil;
  }
}