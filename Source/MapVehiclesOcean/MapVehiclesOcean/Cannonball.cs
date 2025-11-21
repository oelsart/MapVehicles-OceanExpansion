using RimWorld;
using UnityEngine;
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
            var scale = Mathf.Sqrt(DamageAmount) * 2f;
            if (hitThing != null)
            {
                FleckMaker.ThrowDustPuffThick(ExactPosition, map, scale, Color.white);
                MVO_DefOf.MV_CannonballImpact.PlayOneShot(new TargetInfo(Position, map));
            }
            else if (Position.GetTerrain(map).takeSplashes)
            {
                Delay.AfterNTicks(10, () => FleckMaker.WaterSplash(ExactPosition, map, scale, 20f));
                MVO_DefOf.MV_WaterSplash.PlayOneShot(new TargetInfo(Position, map));
            }
        }
    }
}