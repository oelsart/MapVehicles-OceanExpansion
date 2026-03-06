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
        return vehicleDef.thingClass.SameOrSubclassOf<VehiclePawnWithMap>() && vehicleDef.HasComp<CompNpcVehicleMap>() &&
               vehicleDef.GetModExtension<VehicleMapProps_Unique>() is null or { baseDef: null } &&
               vehicleDef.type == VehicleType.Sea && (vehicleDef.vehicleCategory & category) == category &&
               vehicleDef.combatPower <= points && faction.def.techLevel >= vehicleDef.techLevel &&
               (vehicleDef.enabled & VehicleEnabled.For.Raiders) != VehicleEnabled.For.None &&
               vehicleDef.npcProperties != null && (vehicleDef.npcProperties.raidParams == null ||
                                                    vehicleDef.npcProperties.raidParams.Allows(faction,
                                                        arrivalModeDef));
    }

    protected override List<Pawn> GeneratePawns(Faction faction, SitePart sitePart)
    {
        return PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = sitePart.site.Tile,
            faction = faction,
            points = Mathf.Max(sitePart.parms.points, faction.def.MinPointsToGeneratePawnGroup(MVO_DefOf.MVO_ShipCombat))
        }).ToList();
    }
}