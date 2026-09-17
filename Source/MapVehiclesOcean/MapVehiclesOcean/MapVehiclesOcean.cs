using HarmonyLib;
using RimWorld;
using SmashTools;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

public class MapVehiclesOcean : Mod
{
  public const string ModName = "Map Vehicles - Ocean Expansion";
  private const string HarmonyId = "OELS.MapVehiclesOcean";

  public static MapVehiclesOcean Mod { get; private set; }

  public readonly Settings settings;

  public MapVehiclesOcean(ModContentPack content) : base(content)
  {
    Mod = this;
    settings = GetSettings<Settings>();
    new Harmony(HarmonyId).PatchAll();
    RegisterTKeys();

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

  private static void RegisterTKeys()
  {
	  var dictionary = new Dictionary<string, string>
	  {
		  {"MVO_Castaway.LetterLabelQuestExpired", "MVO_Castaway.root.nodes.9.node.nodes.0.label"},
		  {"MVO_Castaway.LetterTextQuestExpired", "MVO_Castaway.root.nodes.9.node.nodes.0.text"},
		  {"MVO_DesertedIsland.LetterLabelDiscovery", "MVO_DesertedIsland.root.nodes.3.elseNode.node.nodes.4.label"},
		  {"MVO_DesertedIsland.LetterTextDiscovery", "MVO_DesertedIsland.root.nodes.3.elseNode.node.nodes.4.text"},
		  {"MVO_DesertedIsland.LetterLabelQuestExpired", "MVO_DesertedIsland.root.nodes.8.node.nodes.0.label"},
		  {"MVO_DesertedIsland.LetterTextQuestExpired", "MVO_DesertedIsland.root.nodes.8.node.nodes.0.text"},
		  {"MVO_MaritimeBase.LetterLabelQuestExpired", "MVO_MaritimeBase.root.nodes.6.node.nodes.0.label"},
		  {"MVO_MaritimeBase.LetterTextQuestExpired", "MVO_MaritimeBase.root.nodes.6.node.nodes.0.text"},
		  {"MVO_MechanoidPlatform.LetterLabelQuestExpired", "MVO_MechanoidPlatform.root.nodes.11.node.nodes.0.label"},
		  {"MVO_MechanoidPlatform.LetterTextQuestExpired", "MVO_MechanoidPlatform.root.nodes.11.node.nodes.0.text"},
		  {"MVO_MechanoidPlatform.LetterLabelQuestFailed", "MVO_MechanoidPlatform.root.nodes.12.node.nodes.0.label"},
		  {"MVO_MechanoidPlatform.LetterTextQuestFailed", "MVO_MechanoidPlatform.root.nodes.12.node.nodes.0.text"},
		  {"MVO_MechanoidPlatform.LetterLabelPaymentArrived", "MVO_MechanoidPlatform.root.nodes.13.node.nodes.1.customLetterLabel"},
		  {"MVO_MechanoidPlatform.LetterTextPaymentArrived", "MVO_MechanoidPlatform.root.nodes.13.node.nodes.1.customLetterText"},
		  {"MVO_MechanoidPlatform.LetterTextFavorReceiver", "MVO_MechanoidPlatform.root.nodes.13.node.nodes.1.nodeIfChosenPawnSignalUsed.text"},
		  {"MVO_Shipwreck.LetterLabelQuestExpired", "MVO_Shipwreck.root.nodes.8.node.nodes.0.label"},
		  {"MVO_Shipwreck.LetterTextQuestExpired", "MVO_Shipwreck.root.nodes.8.node.nodes.0.text"},
		  {"MVO_TheIsland.LetterLabelIslandExistence", "MVO_TheIsland.root.nodes.2.node.nodes.0.node.label"},
		  {"MVO_TheIsland.LetterTextIslandExistence", "MVO_TheIsland.root.nodes.2.node.nodes.0.node.text"},
		  {"MVO_TheIsland.LetterLabelIslandKingdom", "MVO_TheIsland.root.nodes.2.node.nodes.1.node.label"},
		  {"MVO_TheIsland.LetterTextIslandKingdom", "MVO_TheIsland.root.nodes.2.node.nodes.1.node.text"},
		  {"MVO_TheIsland.LetterLabelIslandQuestions", "MVO_TheIsland.root.nodes.2.node.nodes.2.node.label"},
		  {"MVO_TheIsland.LetterTextIslandQuestions", "MVO_TheIsland.root.nodes.2.node.nodes.2.node.text"},
		  {"MVO_TheIsland.LetterLabelIslandDiscovery", "MVO_TheIsland.root.nodes.2.node.nodes.3.node.nodes.7.label"},
		  {"MVO_TheIsland.LetterTextIslandDiscovery", "MVO_TheIsland.root.nodes.2.node.nodes.3.node.nodes.7.text"},
		  {"MVO_TheIsland.EndGameIntroText", "MVO_TheIsland.root.nodes.4.introText"},
		  {"MVO_TheIsland.EndGameEndingText", "MVO_TheIsland.root.nodes.4.endingText"}
	  };
	  TKeySystemHardcodedMapping.TKeyToNormalizedTranslationKey.AddRange(dictionary);
	  TKeySystemHardcodedMapping.TranslationKeyToTKey
		  .AddRange(dictionary.ToDictionary(p => p.Value, p => p.Key));
  }

  public override void DoSettingsWindowContents(Rect inRect)
  {
    var listing_Standard = new Listing_Standard();
    listing_Standard.Begin(inRect);
    listing_Standard.SliderPercentLabeled("MVO_HiddenIslandChance".Translate(), null, null,
      ref settings.hiddenIslandChance, 0f, 1f, 1);
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