using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

public class MapVehiclesOcean : Mod
{
    public const string ModName = "Map Vehicles - Ocean Expansion";
    
    public static MapVehiclesOcean Mod { get; private set; }

    public readonly Settings settings;

    public MapVehiclesOcean(ModContentPack content) : base(content)
    {
        Mod = this;
        settings = GetSettings<Settings>();
        
        LongEventHandler.ExecuteWhenFinished(() =>
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.modContentPack == Mod.Content &&
                    def.thingCategories.NotNullAndAny(c => c == ThingCategoryDefOf.Techprints))
                {
                    def.thingCategories.Add(MVO_DefOf.MVO_Techprints);
                }
            }
        });
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        listing_Standard.SliderPercentLabeled("MVO_HiddenIslandChance".Translate(), null, null, ref settings.hiddenIslandChance, 0f, 1f, 1);
        listing_Standard.End();

        var bottomRight = inRect.BottomPartPixels(30f).RightPartPixels(30f);
        if (Widgets.ButtonImageFitted(bottomRight, TexButton.HotReloadDefs))
        {
            settings.Reset();
        }
    }

    public override string SettingsCategory()
    {
        return ModName;
    }
}