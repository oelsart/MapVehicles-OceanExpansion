using VehicleMapFramework;
using Verse.AI;

namespace MapVehiclesOcean;

public class LordToil_StealToSeaMapVehicle : LordToil_StealToMapVehicle
{
  protected override DutyDef DutyDefVehicle => MVO_DefOf.MVO_RangedBoatAggressive;

  protected override DutyDef AssaultDutyDef => MVO_DefOf.MVO_AssaultSea;
}