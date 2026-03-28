using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class RoomPart_PrefabDef : RoomPartDef
{
    public PrefabDef prefab;

    public bool forceSpawn;

    public bool alignWithRect;

    public bool snapToGrid;

    public bool ignoreAdjacent;

    public bool fillOnPost;
    
    public int contractedBy = 1;
    
    public IntRange intervalRange = new (1, 1);

    public RotationDirection rotOffset;
}