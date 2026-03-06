using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

public class ThingSetMaker_SumWithMarketValueWeight : ThingSetMaker
{
    public List<Option> options;
    public bool resolveInOrder;
    private readonly List<Option> optionsInResolveOrder = [];

    protected override bool CanGenerateSub(ThingSetMakerParams parms)
    {
        return options.Any(t => t.chance > 0.0 && t.thingSetMaker.CanGenerate(parms));
    }

    protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
    {
        var totalCount = 0;
        var totalNutrition = 0f;
        var totalMass = 0f;
        var totalWeight = 0f;

        if (parms.totalMarketValueRange.HasValue)
        {
            foreach (var option in options)
            {
                option.isHit = Rand.Chance(option.chance);
                if (!option.isHit) continue;
                
                totalWeight += option.marketValueWeight = option.marketValueWeightRange.RandomInRange;
            }
        }
        
        optionsInResolveOrder.Clear();
        optionsInResolveOrder.AddRange(options);
        if (!resolveInOrder)
            optionsInResolveOrder.Shuffle();
        for (var i = 0; i < optionsInResolveOrder.Count; ++i)
        {
            if (!optionsInResolveOrder[i].isHit) continue;
            
            var parms1 = parms;
            if (parms1.countRange.HasValue)
            {
                parms1.countRange = new IntRange(Mathf.Max(parms1.countRange.Value.min - totalCount, 0),
                    parms1.countRange.Value.max - totalCount);
                if (parms1.countRange.Value.max <= 0)
                    continue;
            }

            if (parms1.totalMarketValueRange.HasValue)
            {
                parms1.totalMarketValueRange *= optionsInResolveOrder[i].marketValueWeight / totalWeight;
            }

            if (parms1.totalNutritionRange.HasValue)
                parms1.totalNutritionRange =
                    new FloatRange(Mathf.Max(parms1.totalNutritionRange.Value.min - totalNutrition, 0.0f),
                        parms1.totalNutritionRange.Value.max - totalNutrition);
            if (parms1.maxTotalMass.HasValue)
            {
                ref var local = ref parms1.maxTotalMass;
                var nullable = local;
                local = nullable - totalMass;
            }

            if (optionsInResolveOrder[i].thingSetMaker.CanGenerate(parms1))
            {
                var collection = optionsInResolveOrder[i].thingSetMaker.Generate(parms1);
                totalCount += collection.Count;
                for (var index2 = 0; index2 < collection.Count; ++index2)
                {
                    if (collection[index2].def.IsIngestible)
                        totalNutrition += collection[index2].GetStatValue(StatDefOf.Nutrition) * collection[index2].stackCount;
                    if (collection[index2] is not Pawn)
                        totalMass += collection[index2].GetStatValue(StatDefOf.Mass) * collection[index2].stackCount;
                }

                outThings.AddRange(collection);
            }
        }
    }

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        for (var i = 0; i < options.Count; ++i)
            options[i].thingSetMaker.ResolveReferences();
    }

    protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
    {
        return options.Where(t => t.chance > 0.0).SelectMany(t => t.thingSetMaker.AllGeneratableThingsDebug(parms));
    }

    public override IEnumerable<string> ConfigErrors()
    {
        if (options.NullOrEmpty())
        {
            yield return "no options.";
        }
        else
        {
            for (var i = 0; i < options.Count; ++i)
            {
                if (options[i].thingSetMaker == null)
                {
                    yield return "null thingSetMaker";
                }
                else
                {
                    foreach (var configError in options[i].thingSetMaker.ConfigErrors())
                        yield return configError;
                }
            }
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Option
    {
        public ThingSetMaker thingSetMaker;
        public float chance = 1f;
        public FloatRange marketValueWeightRange = FloatRange.One;

        [Unsaved] public bool isHit;
        [Unsaved] public float marketValueWeight;
    }
}