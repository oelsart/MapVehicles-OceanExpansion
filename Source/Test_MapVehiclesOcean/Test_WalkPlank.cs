global using static VehicleMapFramework.Test.TestUtility;
using System.Collections;
using DevTools.Testing;
using RimWorld;
using UnityEngine.Assertions;
using VehicleMapFramework;
using VehicleMapFramework.Test;
using Vehicles.Tests;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean.Test;

[TestFixture(TestType.Playing)]
internal sealed class Test_WalkPlank
{
	public VehicleGroup Group { get; set; }
	
	[SetUp]
	public void SetUp()
	{
		Group = CreateBasicMapVehicleGroup(new VehicleGroup.MockSettings
		{
			size = new IntVec2(4, 8),
			passengers = 4
		});

		var vehicle = (VehiclePawnWithMap)Group.vehicle;
		TestUtils.ForceSpawn(vehicle);
		var gangplank = DefDatabase<ThingDef>.GetNamed("MVO_Gangplank");
		Assert.IsNotNull(gangplank);
		GenSpawn.Spawn(gangplank, new IntVec3(1, 0, 2), vehicle.VehicleMap, Rot4.West);
		Group.SpawnPawns();
	}

	[TearDown]
	public void TearDown()
	{
		Group.Dispose();
		Group = null;
	}
	
	[Test]
	public IEnumerator WalkPlank()
	{
		var joy = DefDatabase<JoyGiverDef>.GetNamed("MVO_WalkPlank");
		Assert.IsNotNull(joy);
		var jobDef = DefDatabase<JobDef>.GetNamed("MVO_WalkPlank");
		Assert.IsNotNull(jobDef);
		foreach (var pawn in Group.pawns)
		{
			var job = joy.Worker.TryGiveJob(pawn);
			Assert.IsNotNull(job);
			pawn.jobs.StartJob(job, JobCondition.InterruptForced);
			Assert.AreEqual(jobDef, pawn.NextJobOrCurJob?.def);
		}

		using (new TimeSpeedScope(TimeSpeed.Ultrafast))
		{
			yield return Group.pawns[0].WaitJob(jobDef);
		}
	}
}