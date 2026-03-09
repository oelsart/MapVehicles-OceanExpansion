using RimWorld;
using Verse;

namespace MapVehiclesOcean;

public class GenStep_IslandTreasure : GenStep
{
    private const string SteleQuestTag = "MVO_Stele";
    
    public override int SeedPart => 848961748;

    public override void Generate(Map map, GenStepParams parms)
    {
        var elevation = MapGenerator.Elevation;
        var top = map.AllCells.MaxBy(c => elevation[c]);

        var quest = Find.QuestManager.ActiveQuestsListForReading.Find(q => q.root == MVO_DefOf.MVO_TheIsland);
        if (quest is not null)
        {
            Predicate<IntVec3> validator = c => c.Roofed(map) && GenSpawn.CanSpawnAt(MVO_DefOf.MVO_Stele, c, map, Rot4.North, false);
            if (RCellFinder.TryFindRandomCellNearWith(top, validator, map, out var cell, 3) ||
                RCellFinder.TryFindRandomCellNearWith(top, validator, map, out cell) ||
                RCellFinder.TryFindRandomCellNearWith(top, validator, map, out cell, 7))
            {
                var stele = GenSpawn.Spawn(MVO_DefOf.MVO_Stele, cell, map, Rot4.North);
                QuestUtility.AddQuestTag(stele, $"Quest{quest.id}.{SteleQuestTag}");
            }
        }
        
        var rot = Rot4.Random;
        Predicate<IntVec3> validator2 = c => c.Roofed(map) && GenSpawn.CanSpawnAt(MVO_DefOf.MVO_StoneCoffer, c, map, rot, false);
        if (RCellFinder.TryFindRandomCellNearWith(top, validator2, map, out var cell2, 3) ||
            RCellFinder.TryFindRandomCellNearWith(top, validator2, map, out cell2) ||
            RCellFinder.TryFindRandomCellNearWith(top, validator2, map, out cell2, 7))
        {
            var coffer = (Building_Crate)ThingMaker.MakeThing(MVO_DefOf.MVO_StoneCoffer, GenStuff.RandomStuffFor(MVO_DefOf.MVO_StoneCoffer));
            GenSpawn.Spawn(coffer, cell2, map, rot);
            coffer.GetDirectlyHeldThings().TryAddRangeOrTransfer(parms.sitePart.things);
        }
    }
}