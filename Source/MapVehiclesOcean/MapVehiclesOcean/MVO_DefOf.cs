using RimWorld;
using Verse;

namespace MapVehicles;

[DefOf]
public static class MVO_DefOf
{
    static MVO_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MVO_DefOf));
    }

    public static SoundDef MV_WaterSplash;
    
    public static SoundDef MV_CannonballImpact;

    public static FleckDef MV_ShockwaveSmall;

    public static FleckDef GroundWaterSplash;
}