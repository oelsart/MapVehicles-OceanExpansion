using RimWorld;
using VehicleMapFramework;
using Vehicles;
using Verse;
using Verse.AI.Group;

namespace MapVehiclesOcean;

public class IncidentWorker_Ambush_EnemyBoats : IncidentWorker_Ambush_EnemyMapVehicle
{
    protected override WorldObjectDef MapParentDef => MVO_DefOf.MVO_AmbushSea;

    protected override List<Pawn> GeneratePawns(IncidentParms parms)
    {
        var defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(MVO_DefOf.MVO_ShipCombat, parms);
        defaultPawnGroupMakerParms.generateFightersOnly = true;
        defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = true;
        return PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
    }
    
    protected override bool ValidRaiderVehicle(VehicleDef vehicleDef, VehicleCategory category, PawnsArrivalModeDef arrivalModeDef,
        Faction faction, float points)
    {
        return VehicleCaravanIncidentUtility.ValidSeaThreatVehicle(vehicleDef, category, arrivalModeDef, faction, points);
    }

    protected override LordJob CreateLordJob(List<VehiclePawnWithMap> generatedVehicles, IncidentParms parms)
    {
        return new LordJob_ArmoredAssaultSea(parms.faction, LordJob_ArmoredAssault.RaiderPermissions.All);
    }
}