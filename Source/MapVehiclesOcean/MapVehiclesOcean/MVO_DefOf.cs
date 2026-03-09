using RimWorld;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

[DefOf]
public static class MVO_DefOf
{
    static MVO_DefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MVO_DefOf));
    }

    public static ThingDef MVO_Volleyball;

    public static ThingDef MVO_StoneCoffer;

    public static ThingDef MVO_Stele;

    public static ThingDef MVO_Filth_TallyMarks;

    public static SoundDef MVO_WaterSplash;
    
    public static SoundDef MVO_CannonballImpact;

    public static FleckDef MVO_ShockwaveSmall;

    public static FleckDef GroundWaterSplash;

    public static JobDef MVO_JobLookout;

    public static JobDef MVO_GotoShipCombat;

    public static QuestScriptDef MVO_Shipwreck;

    public static QuestScriptDef MVO_DesertedIsland;
    
    public static QuestScriptDef MVO_TheIsland;

    public static TerrainDef MVO_WaterDeepVirtual;
    
    public static TerrainDef MVO_WaterOceanDeepVirtual;
    
    public static TerrainDef MVO_NoWaterVirtual;

    public static DutyDef MVO_RangedBoatAggressive;

    public static PawnGroupKindDef MVO_ShipCombat;

    public static WorldObjectDef MVO_AmbushSea;
    

    [MayRequireOdyssey]
    public static LandmarkDef MVO_OceanIsland;
}