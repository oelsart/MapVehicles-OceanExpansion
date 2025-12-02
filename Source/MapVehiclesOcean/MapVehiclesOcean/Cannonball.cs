using RimWorld;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.Sound;

namespace MapVehicles;

public class Cannonball : Bullet
{
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        var map = Map;
        base.Impact(hitThing, blockedByShield);
        if (!blockedByShield)
        {
            var pos = ExactPosition;
            var scale = Mathf.Sqrt(DamageAmount) / 3f;
            
            switch (hitThing)
            {
                case VehiclePawn:
                    FleckMaker.ThrowDustPuff(pos, map, scale);
                    FleckMaker.Static(pos, map, MVO_DefOf.MV_ShockwaveSmall, scale);
                    MVO_DefOf.MV_CannonballImpact.PlayOneShot(new TargetInfo(Position, map));
                    break;
                case null when Position.GetTerrain(map).takeSplashes:
                    FleckMaker.Static(pos, map, MVO_DefOf.GroundWaterSplash, scale);
                    FleckMaker.WaterRipple(pos, map, scale);
                    MVO_DefOf.MV_WaterSplash.PlayOneShot(new TargetInfo(Position, map));
                    break;
                default:
                    FleckMaker.ThrowDustPuff(pos, map, scale);
                    FleckMaker.Static(pos, map, MVO_DefOf.MV_ShockwaveSmall, scale);
                    break;
            }
        }
    }
}