using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GenStep_Dungeon : GenStep
{
	public override int SeedPart => 189674696;
	public PrefabDef prefabDef;
	private int SharkCount = 8;
	private string AllTrialPassedSignal = "AllTrialPassed";

	public override void Generate(Map map, GenStepParams parms)
	{
		var groupID = Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID();
		var cellRect = CellRect.CenteredOn(map.Center, prefabDef.size);
		var quest = Find.QuestManager.ActiveQuestsListForReading.Find(q => q.root == MVO_DefOf.MVO_TheIsland);
		SetupArea(map, cellRect);
		PrefabUtility.SpawnPrefab(prefabDef, map, map.Center, Rot4.North, onSpawned: thing =>
		{
			if (thing.def == MVO_DefOf.MVO_Bowl)
			{
				thing.SetFactionDirect(Faction.OfPlayer);
			}
			else if (thing.def == MVO_DefOf.MVO_Lychnos_Midas ||
			         thing.def == MVO_DefOf.MVO_Lychnos_Minotaur ||
			         thing.def == MVO_DefOf.MVO_Lychnos_Athena ||
			         thing.def == MVO_DefOf.MVO_Lychnos_Poseidon)
			{
				QuestUtility.AddQuestTag(thing, thing.def.defName);
				if (quest != null)
				{
					QuestUtility.AddQuestTag(thing, $"Quest{quest.id}.{thing.def.defName}");
				}
			}
			else if (thing.def == MVO_DefOf.MVO_SignalAction_SpawnThing)
			{
				var signalAction = (SignalAction_SpawnThing)thing;
				signalAction.signalTag = $"Quest{quest.id}.{AllTrialPassedSignal}";
				signalAction.thingDef = MVO_DefOf.MVO_DungeonStairDown;
			}
			else if (thing is Building_AncientCryptosleepCasket cryptosleepCasket)
			{
				cryptosleepCasket.groupID = groupID;
				var list = ThingSetMakerDefOf.MapGen_AncientPodContents.root.Generate(default(ThingSetMakerParams) with
				{
					podContentsType = PodContentsType.AncientHostile
				});
				for (var i = 0; i < list.Count; i++)
				{
					if (!cryptosleepCasket.TryAcceptThing(list[i], false))
					{
						if (list[i] is Pawn pawn)
						{
							Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
						}
						else
						{
							list[i].Destroy();
						}
					}
				}

				var trigger = (TriggerUnfogged)ThingMaker.MakeThing(ThingDefOf.TriggerUnfogged);
				trigger.signalTag = thing.ThingID;
				GenSpawn.Spawn(trigger, cryptosleepCasket.Position, map);
				RoomGenUtility.SpawnOpenCryptoCasketSignal(cryptosleepCasket, map, thing.ThingID);
			}
		});

		var root = cellRect.GetCorner(Rot4.West) + new IntVec3(15, 0, -8);
		for (var i = 0; i < SharkCount; i++)
		{
			var shark = PawnGenerator.GeneratePawn(MVO_DefOf.MVO_Shark);
			GenSpawn.Spawn(shark, root, map);
			shark.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
		}
	}

	private void SetupArea(Map map, CellRect rect)
	{
		var terrains = prefabDef.GetTerrain().ToDictionary(tuple => tuple.cell, tuple => tuple.data);
		foreach (var c in rect)
		{
			if (!c.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy))
			{
				map.terrainGrid.SetTerrain(c, TerrainDefOf.Sand);
			}
			
			var c2 = c - rect.Min + IntVec3.SouthWest;
			if (terrains.TryGetValue(c2, out var data) && data?.def is not null)
			{
				map.roofGrid.SetRoof(c, RoofDefOf.RoofConstructed);
				var list = c.GetThingList(map);
				for (var i = list.Count - 1; i >= 0; i--)
				{
					var thing = list[i];
					if (thing.def.destroyable)
					{
						thing.Destroy();
					}
				}
			}
		}
	}
}