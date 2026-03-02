using Verse;

namespace MapVehicles;

public class Settings : ModSettings
{
    public float hiddenIslandChance = Default.hiddenIslandChance;
    
    public override void ExposeData()
    {
        Scribe_Values.Look(ref hiddenIslandChance, "hiddenIslandChance", Default.hiddenIslandChance);
    }

    public void Reset()
    {
        hiddenIslandChance = Default.hiddenIslandChance;
    }

    private static class Default
    {
        public const float hiddenIslandChance = 0.01f;
    }
}