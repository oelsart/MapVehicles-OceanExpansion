using SmashTools;
using UnityEngine;
using Verse;

namespace MapVehicles;

public class MapVehiclesOcean : Mod
{
    public const string ModName = "Map Vehicles - Ocean Expansion";
    
    public static MapVehiclesOcean Mod { get; private set; }

    public readonly Settings settings;

    public MapVehiclesOcean(ModContentPack content) : base(content)
    {
        Mod = this;
        settings = GetSettings<Settings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        listing_Standard.SliderPercentLabeled("MV_HiddenIslandChance".Translate(), null, null, ref settings.hiddenIslandChance, 0f, 1f, 1);
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