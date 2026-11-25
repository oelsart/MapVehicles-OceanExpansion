using SmashTools;
using UnityEngine;
using VehicleMapFramework;
using Vehicles;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

public static class CombatPositionUtility
{
  public static bool TryFindShipCombatPosition(VehiclePawn vehicle, out IntVec3 dest, out Rot8 endRot)
  {
    endRot = Rot8.Invalid;
    dest = vehicle.Position;
    var target = vehicle.mindState.enemyTarget;
    if (target is null)
    {
      dest = vehicle.Position;
      return false;
    }

    var radius = Mathf.Max(target.def.Size.x, target.def.Size.z) * 2;
    var offset = vehicle is VehiclePawnWithMap { CompNpcVehicleMap.Params.preferredDir.IsVertical: true }
      ? vehicle.VehicleDef.Size.z
      : vehicle.VehicleDef.Size.x;
    var collisionRect = target.OccupiedRect().ExpandedBy(offset);
    var root = target.Position + Rot8.FromAngle((vehicle.Position - target.Position).AngleFlat).FacingCell *
      Mathf.Min(target.def.Size.x, target.def.Size.z) / 2;
    var num = GenRadial.NumCellsInRadius(radius);
    for (var i = 0; i < num; i++)
    {
      var intVec = GenRadial.RadialPattern[i] + root;
      if (collisionRect.Contains(intVec)) continue;
      if (vehicle.CanReachVehicle(intVec, PathEndMode.OnCell, Danger.Deadly))
      {
	      Rot8? rot = null;
        if (vehicle is VehiclePawnWithMap { CompNpcVehicleMap: { } compNpcVehicleMap })
        {
          var dir = compNpcVehicleMap.Params.preferredDir;
          var angle = (target.PositionOnBaseMap - intVec).AngleFlat;
          var rot2 = Rot8.FromAngle(angle);
          rot = new Rot8(Rot8.FromIntClockwise(GenMath.PositiveMod(rot2.AsIntClockwise - dir.AsIntClockwise, 8)));
        }

        if (vehicle.CellRectStandable(vehicle.Map, intVec, rot))
        {
	        endRot = rot ?? Rot8.Invalid;
	        dest = intVec;
	        return true;
        }
      }
    }

    return false;
  }
}