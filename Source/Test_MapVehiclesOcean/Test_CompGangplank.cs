using DevTools.Testing;
using SmashTools;
using UnityEngine.Assertions;
using VehicleMapFramework;
using Vehicles.Tests;
using Verse;

namespace MapVehiclesOcean.Test;

[TestFixture(TestType.Playing)]
internal sealed class Test_CompGangplank
{
	public VehicleGroup Group1 { get; set; }
	public VehicleGroup Group2 { get; set; }

	[SetUp]
	public void SetUp()
	{
		Group1 = CreateBasicMapVehicleGroup(new VehicleGroup.MockSettings
		{
			size = new IntVec2(4, 8),
			passengers = 0
		});
		Group2 = CreateBasicMapVehicleGroup(new VehicleGroup.MockSettings
		{
			size = new IntVec2(4, 8),
			passengers = 0
		});

		var vehicle1 = (VehiclePawnWithMap)Group1.vehicle;
		var vehicle2 = (VehiclePawnWithMap)Group2.vehicle;

		GenSpawn.Spawn(vehicle1, new IntVec3(20, 0, 20), Find.CurrentMap);
		GenSpawn.Spawn(vehicle2, new IntVec3(25, 0, 20), Find.CurrentMap);
	}

	[TearDown]
	public void TearDown()
	{
		Group1?.Dispose();
		Group1 = null;
		Group2?.Dispose();
		Group2 = null;
	}

	[Test]
	public void Test_Gangplank_RegisterAndDisconnect_OnDeSpawn()
	{
		var crawler1 = (VehiclePawnWithMap)Group1.vehicle;
		var gangplankDef = DefDatabase<ThingDef>.GetNamed("MVO_Gangplank");
		Assert.IsNotNull(gangplankDef);

		// crawler1の東端にEast向きで配置し、隣接するcrawler2に向けてタラップを渡せるようにする
		var gangplank = GenSpawn.Spawn(gangplankDef, new IntVec3(crawler1.VehicleMap.Size.x - 2, 0, 2), crawler1.VehicleMap, Rot4.East);
		var comp = gangplank.TryGetComp<CompGangplank>();
		Assert.IsNotNull(comp);

		var regionA = gangplank.GetRegion();
		Assert.IsNotNull(regionA);

		var links = Find.World.GetComponent<CrossMapRegionLinks>();
		Assert.IsNotNull(links);

		// スポーン直後はまだアンカー・リンクが未作成
		Assert.IsFalse(HasLink(links, regionA));

		// Tickを進めて隣接ビークルを探索し、アンカー生成とリンク登録を実行
		comp.CompTickInterval(30);

		// CrossMapRegionLinksに正常に登録されたことを検証
		Assert.IsTrue(HasLink(links, regionA));

		// Gangplankを破棄（DeSpawn）
		gangplank.Destroy();

		// CrossMapRegionLinksから正常に切断されたことを検証
		Assert.IsFalse(HasLink(links, regionA));
	}

	[Test]
	public void Test_Gangplank_Disconnect_OnVehicleMove()
	{
		var vehicle1 = (VehiclePawnWithMap)Group1.vehicle;
		var vehicle2 = (VehiclePawnWithMap)Group2.vehicle;
		var gangplankDef = DefDatabase<ThingDef>.GetNamed("MVO_Gangplank");
		Assert.IsNotNull(gangplankDef);

		var gangplank = GenSpawn.Spawn(gangplankDef, new IntVec3(vehicle1.VehicleMap.Size.x - 2, 0, 2), vehicle1.VehicleMap, Rot4.East);
		var comp = gangplank.TryGetComp<CompGangplank>();
		Assert.IsNotNull(comp);

		var regionA = gangplank.GetRegion();
		var links = Find.World.GetComponent<CrossMapRegionLinks>();

		// Tickを進めてリンクを確立
		comp.CompTickInterval(30);
		Assert.IsTrue(HasLink(links, regionA));

		var cachedDrawPos = vehicle2.cachedDrawPos;
		// 接続先ビークルを離れた位置へ移動させる
		vehicle2.Position = new IntVec3(50, 0, 50);
		vehicle2.VehicleMap.GetCachedMapComponent<VehiclePawnWithMapCache>().ForceResetDrawPosCache();
		vehicle2.DrawTracker.tweener.ResetTweenedPosToRoot();
		vehicle2.RecacheDrawPos(vehicle2.DrawPos);
		Assert.AreNotEqual(cachedDrawPos, vehicle2.cachedDrawPos);

		// 再度Tickを実行して位置ズレ検知による切断をトリガー
		comp.CompTickInterval(30);

		// リンクが切断されたことを検証
		Assert.IsFalse(HasLink(links, regionA));
	}

	[Test]
	public void Test_Gangplank_Disconnect_OnVehicleRotate()
	{
		var vehicle1 = (VehiclePawnWithMap)Group1.vehicle;
		var vehicle2 = (VehiclePawnWithMap)Group2.vehicle;
		var gangplankDef = DefDatabase<ThingDef>.GetNamed("MVO_Gangplank");
		Assert.IsNotNull(gangplankDef);

		var gangplank = GenSpawn.Spawn(gangplankDef, new IntVec3(vehicle1.VehicleMap.Size.x - 2, 0, 2), vehicle1.VehicleMap, Rot4.East);
		var comp = gangplank.TryGetComp<CompGangplank>();
		Assert.IsNotNull(comp);

		var regionA = gangplank.GetRegion();
		var links = Find.World.GetComponent<CrossMapRegionLinks>();

		// Tickを進めてリンクを確立
		comp.CompTickInterval(30);
		Assert.IsTrue(HasLink(links, regionA));

		// 接続先ビークルを離れた位置へ移動させる
		vehicle2.Rotation = Rot4.East;
		vehicle2.VehicleMap.GetCachedMapComponent<VehiclePawnWithMapCache>().ForceResetDrawPosCache();

		// 再度Tickを実行して位置ズレ検知による切断をトリガー
		comp.CompTickInterval(30);

		// リンクが切断されたことを検証
		Assert.IsFalse(HasLink(links, regionA));

		vehicle2.Rotation = Rot4.South;
		vehicle2.VehicleMap.GetCachedMapComponent<VehiclePawnWithMapCache>().ForceResetDrawPosCache();
		
		comp.CompTickInterval(30);
		
		Assert.IsTrue(HasLink(links, regionA));
	}

	private static bool HasLink(CrossMapRegionLinks links, Region region)
	{
		if (region == null) return false;
		var connected = links.GetConnectedRegions(region);
		return connected.Length > 0;
	}
}