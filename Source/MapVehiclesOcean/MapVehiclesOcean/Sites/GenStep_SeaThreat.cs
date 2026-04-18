using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VehicleMapFramework;
using Vehicles;
using Verse;

namespace MapVehiclesOcean;

public class GenStep_SeaThreat : GenStep_MapVehicleThreat
{
    public override int SeedPart => 616154648;

    protected override bool ValidRaiderVehicle(VehicleDef vehicleDef, VehicleCategory category, PawnsArrivalModeDef arrivalModeDef,
        Faction faction, float points)
    {
        return VehicleCaravanIncidentUtility.ValidSeaThreatVehicle(vehicleDef, category, arrivalModeDef, faction, points);
    }

    protected override List<Pawn> GeneratePawns(Faction faction, SitePart sitePart)
    {
        return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = sitePart.site.Tile,
            faction = faction,
            points = Mathf.Max(sitePart.parms.points,
                faction.def.MinPointsToGeneratePawnGroup(MVO_DefOf.MVO_ShipCombat))
        }).ToList();
    }
}