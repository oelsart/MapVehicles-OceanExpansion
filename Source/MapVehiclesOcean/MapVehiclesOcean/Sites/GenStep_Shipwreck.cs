using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MapVehicles;

public class GenStep_Shipwreck : GenStep_Scatterer
{
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public List<PrefabDef> wrecks;

    [Unsaved]
    private readonly List<CellRect> usedRects = [];

    [Unsaved]
    private PrefabDef tmpWreck;

    [Unsaved]
    private Rot4 tmpRot;

    public override int SeedPart => 651968188;

    private static readonly SimpleCurve NumWrecksCurve =
    [
        new CurvePoint(0.6f, 1),
        new CurvePoint(0.8f, 2),
        new CurvePoint(0.95f, 3),
        new CurvePoint(1f, 5)
    ];

    private static readonly SimpleCurve NumCorpsesCurve =
    [
        new CurvePoint(0.5f, 0),
        new CurvePoint(0.75f, 1),
        new CurvePoint(0.95f, 3),
        new CurvePoint(1f, 5)
    ];

    private static readonly IntRange CorpseAgeRangeDays = new (50, 10000);

    protected override int CalculateFinalCount(Map map) => (int)NumWrecksCurve.Evaluate(Rand.Value);

    public override void Generate(Map map, GenStepParams parms)
    {
        base.Generate(map, parms);
        usedRects.Clear();
        tmpWreck = null;
        tmpRot = default;
    }

    protected override bool TryFindScatterCell(Map map, out IntVec3 result)
    {
        tmpWreck = wrecks.RandomElement();
        tmpRot = Rot4.Random;
        return base.TryFindScatterCell(map, out result);
    }

    protected override bool CanScatterAt(IntVec3 loc, Map map)
    {
        var size = tmpRot.IsVertical ? tmpWreck.size : new IntVec2(tmpWreck.size.z, tmpWreck.size.x);
        var cellRect = CellRect.CenteredOn(loc, size);
        return usedRects.All(r => !r.Overlaps(cellRect));
    }

    protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int _count = 1)
    {
        SpawnWrecks(loc, map, parms);
    }

    private void SpawnWrecks(IntVec3 loc, Map map, GenStepParams parms)
    {
        var size = tmpRot.IsVertical ? tmpWreck.size : new IntVec2(tmpWreck.size.z, tmpWreck.size.x);
        var cellRect = CellRect.CenteredOn(loc, size);
        usedRects.Add(cellRect.ExpandedBy(1));
        
        PrefabUtility.SpawnPrefab(tmpWreck, map, loc, tmpRot, onSpawned: thing =>
        {
            if (thing.def.useHitPoints)
                thing.HitPoints = Rand.Range(Mathf.CeilToInt(thing.MaxHitPoints * 0.05f), thing.HitPoints);
            if (thing.TryGetComp<CompRefuelable>(out var fuelComp))
                fuelComp.ConsumeFuel(fuelComp.Fuel);
        });

        var sitePart = parms.sitePart;
        if (sitePart?.things == null || sitePart.things.Count == 0) return;

    
        var containers = map.listerBuildings.allBuildingsNonColonist
            .Where(b => b is IThingHolder && b.Faction is null)
            .Cast<IThingHolder>()
            .ToList();

        foreach (var t in sitePart.things.ToArray())
        {
            var placed = false;
            for (var i = 0; i < containers.Count; i++)
            {
                var container = containers[i];
                if (container.GetDirectlyHeldThings().CanAcceptAnyOf(t))
                {
                    sitePart.things.TryTransferToContainer(t, container.GetDirectlyHeldThings());
                    placed = true;
                    containers.RemoveAt(i);
                    containers.Add(container);
                    break;
                }
            }

            if (!placed)
            {
                sitePart.things.TryDrop(t, loc, map, ThingPlaceMode.Near, out _);
            }
        }
    }

    private void SpawnCorpses(Map map)
    {
        for (var i = 0; i < Rand.Range(0, NumCorpsesCurve.Evaluate(Rand.Value)); i++)
        {
            var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Drifter);
            if (!CellFinder.TryFindRandomSpawnCellForPawnNear(usedRects.RandomElement().RandomCell, map, out var cell))
                continue;
            pawn.health.SetDead();
            pawn.apparel.DestroyAll();
            pawn.equipment.DestroyAllEquipment();
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            var corpse = pawn.MakeCorpse(null, null);
            corpse.Age = Mathf.RoundToInt(CorpseAgeRangeDays.RandomInRange * 60000);
            corpse.GetComp<CompRottable>().RotProgress += corpse.Age;
            GenSpawn.Spawn(pawn.Corpse, cell, map);
        }
    }
}