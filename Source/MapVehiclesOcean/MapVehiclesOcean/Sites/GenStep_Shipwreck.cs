using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapVehicles;

public class GenStep_Shipwreck : GenStep
{
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public List<PrefabDef> wrecks;

    public override int SeedPart => 651968188;
    
    public override void Generate(Map map, GenStepParams parms)
    {
        var rot = Rot4.Random;
        var wreck = wrecks.RandomElement();
        // if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(x =>
        //         PrefabUtility.CanSpawnPrefab(wreck, map, x, rot, false), map, out var center))
        //     return;
        var center = map.Center;
        PrefabUtility.SpawnPrefab(wreck, map, center, rot, onSpawned: thing =>
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
                sitePart.things.TryDrop(t, center, map, ThingPlaceMode.Near, out _);
            }
        }
    }
}