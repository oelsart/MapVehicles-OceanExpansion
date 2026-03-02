using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace MapVehicles;

public class TileMutatorWorker_OceanIsland(TileMutatorDef def) : TileMutatorWorker_Coast(def)
{
    protected override FloatRange CoastOffset => new(0.2f, 0.4f);
    protected virtual FloatRange MountainSizeFactor => new(0.4f, 0.5f);

    private readonly List<IntVec3> tmpRidgeCells = [];
    private readonly TileMutatorWorker_IslandCaves caveMutator = new(def);

    private const float MountainNoiseFrequency = 0.015f;
    private const float MountainNoiseStrength = 25f;

    public override void Init(Map map)
    {
        var min = Mathf.Min(map.Size.x, map.Size.z);
        var radius = min - min * CoastOffset.RandomInRange;
        coastNoise = MapNoiseUtility.CreateFalloffRadius(radius, (map.Size / 2).ToVector2());
        coastNoise = MapNoiseUtility.AddDisplacementNoise(coastNoise, 0.006f, 30f);
        coastNoise = MapNoiseUtility.AddDisplacementNoise(coastNoise, 0.015f, 25f);
        NoiseDebugUI.StoreNoiseRender(coastNoise, "coast");
        
        caveMutator.Init(map);
    }

    public override void GeneratePostElevationFertility(Map map)
    {
        if (!ModsConfig.OdysseyActive) return;
        base.GeneratePostElevationFertility(map);
        
        // 山のElevation設定
        var min = Mathf.Min(map.Size.x, map.Size.z);
        var radius = min * MountainSizeFactor.RandomInRange;
        var offset = GenRadial.RadialCellsAround(map.Size / 2, min * 0.05f, true).RandomElement().ToVector2();
        var moduleBase = MapNoiseUtility.CreateFalloffRadius(radius, offset);
        moduleBase = MapNoiseUtility.AddDisplacementNoise(moduleBase, MountainNoiseFrequency, MountainNoiseStrength);
        var elevation = MapGenerator.Elevation;
        foreach (var intVec in map.BoundsRect())
        {
            elevation[intVec] += moduleBase.GetValue(intVec);
        }
        
        caveMutator.GeneratePostElevationFertility(map);
    }

    public override void GeneratePostTerrain(Map map)
    {
        base.GeneratePostTerrain(map);
        
        // 四隅からFloodFillerを使い、海と接する海でない水セルを海の水で満たす処理
        var cells = map.BoundsRect().Corners.ToArray();
        var visited = new bool[4];
        for (var i = 0; i < 4; i++)
        {
            if (visited[i]) continue;
            map.floodFiller.FloodFill(cells[0], c => c.GetTerrain(map).IsWater, c =>
            {
                var index = cells.IndexOf(c);
                if (index != -1) visited[index] = true;
                
                var terrain = c.GetTerrain(map);
                if (terrain != TerrainDefOf.WaterOceanDeep && terrain != TerrainDefOf.WaterOceanShallow)
                {
                    map.terrainGrid.SetTerrain(c, TerrainDefOf.WaterOceanShallow);
                }
            });
        }
        
        caveMutator.GeneratePostTerrain(map);
        GenerateMountain(map);
        ModuleBase freqFactorNoise =
            new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Range(0, 999999), QualityMode.Medium);
        freqFactorNoise = new ScaleBias(1.0, 1.0, freqFactorNoise);
        NoiseDebugUI.StoreNoiseRender(freqFactorNoise, "rock_chunks_freq_factor");
        var num = map.TileInfo.Mutators.Aggregate(0.006f,
            (current, tileMutatorDef) => current * tileMutatorDef.chunkDensityFactor);
        var elevation = MapGenerator.Elevation;
        foreach (var intVec in map.BoundsRect())
        {
            var num2 = num * freqFactorNoise.GetValue(intVec);
            if ((elevation[intVec] < 0.55f || map.generatorDef.isUnderground) && Rand.Value < num2)
            {
                GrowLowRockFormationFrom(intVec, map);
            }
        }

        // マップがWaterDeepで囲まれている時PlayerStartSpotと動物のスポーン地点の取得に失敗するため、一時的にWaterShallowで橋を作る
        if (map.regionGrid.allDistricts.All(d => !d.TouchesMapEdge))
        {
            for (var i = 0; i < 4; i++)
            {
                tmpRidgeCells.Clear();
                var rot = new Rot4(i);
                var opposite = rot.Opposite.FacingCell;
                var cell = map.BoundsRect().GetCenterCellOnEdge(rot);
                while (cell.InBounds(map) && cell.GetTerrain(map) == TerrainDefOf.WaterOceanDeep)
                {
                    tmpRidgeCells.Add(cell);
                    cell += opposite;
                }
                if (cell.GetDistrict(map) is { CellCount: >= 10 })
                {
                    foreach (var cell2 in tmpRidgeCells)
                    {
                        map.terrainGrid.SetTerrain(cell2, TerrainDefOf.WaterOceanShallow);
                    }
                    break;
                }
            }
        }
        return;
        
        // GenStep_RocksFromGridより
        static void GenerateMountain(Map map)
        {
            map.regionAndRoomUpdater.Enabled = false;
            const float num1 = 0.7f;
            var roofThresholdList =
                new List<RoofThreshold>
                {
                    new()
                    {
                        roofDef = RoofDefOf.RoofRockThick,
                        minGridVal = num1 * 1.14f
                    },
                    new()
                    {
                        roofDef = RoofDefOf.RoofRockThin,
                        minGridVal = num1 * 1.04f
                    }
                };
            var elevation = MapGenerator.Elevation;
            var caves = MapGenerator.Caves;
            foreach (var cell in map.BoundsRect())
            {
                var num2 = elevation[cell];
                if ((double)num2 > num1)
                {
                    if (caves[cell] <= 0.0)
                        GenSpawn.Spawn(GenStep_RocksFromGrid.RockDefAt(cell), cell, map);
                    for (var index = 0; index < roofThresholdList.Count; ++index)
                    {
                        if ((double)num2 > roofThresholdList[index].minGridVal)
                        {
                            map.roofGrid.SetRoof(cell, roofThresholdList[index].roofDef);
                            break;
                        }
                    }
                }
            }

            var visited = new BoolGrid(map);
            var toRemove = new List<IntVec3>();
            foreach (var allCell in map.BoundsRect())
            {
                if (!visited[allCell] && IsNaturalRoofAt(allCell, map))
                {
                    toRemove.Clear();
                    map.floodFiller.FloodFill(allCell, x => IsNaturalRoofAt(x, map),
                        x =>
                        {
                            visited[x] = true;
                            toRemove.Add(x);
                        });
                    if (toRemove.Count < 20)
                    {
                        for (var index = 0; index < toRemove.Count; ++index)
                            map.roofGrid.SetRoof(toRemove[index], null);
                    }
                }
            }

            var scatterLumpsMineable = new GenStep_ScatterLumpsMineable
            {
                useNomadicMineables = true
            };
            var per10KcellsForMap = GenStep_RocksFromGrid.GetResourceBlotchesPer10KCellsForMap(map);
            scatterLumpsMineable.countPer10kCellsRange = new FloatRange(per10KcellsForMap, per10KcellsForMap);
            scatterLumpsMineable.Generate(map, new GenStepParams());
            map.regionAndRoomUpdater.Enabled = true;
            return;
            
            static bool IsNaturalRoofAt(IntVec3 c, Map map)
            {
                return c.Roofed(map) && c.GetRoof(map).isNatural;
            }
        }
        
        // GenStep_RockChunksより
        static void GrowLowRockFormationFrom(IntVec3 root, Map map)
        {
            var filth_RubbleRock = ThingDefOf.Filth_RubbleRock;
            var forceRockTypes = map.Biome.forceRockTypes;
            var thingDef = forceRockTypes != null
                ? forceRockTypes.RandomElement()
                : Find.World.NaturalRockTypesIn(map.Tile).RandomElement();

            var mineableThing = thingDef.building.mineableThing;
            if (mineableThing == null)
            {
                return;
            }

            var random = Rot4.Random;
            var elevation = MapGenerator.Elevation;
            var intVec = root;
            for (;;)
            {
                var random2 = Rot4.Random;
                if (!(random2 == random))
                {
                    intVec += random2.FacingCell;
                    if (!intVec.InBounds(map) || intVec.GetEdifice(map) != null || intVec.GetFirstItem(map) != null)
                    {
                        break;
                    }

                    if (!map.generatorDef.isUnderground && elevation[intVec] > 0.55f)
                    {
                        return;
                    }

                    if (!intVec.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy))
                    {
                        return;
                    }

                    if (intVec.GetDoor(map) != null)
                    {
                        return;
                    }

                    GenSpawn.Spawn(mineableThing, intVec, map);
                    foreach (var intVec2 in GenAdj.AdjacentCellsAndInside)
                    {
                        if (Rand.Value < 0.5f)
                        {
                            var intVec3 = intVec + intVec2;
                            if (!intVec3.InBounds(map)) continue;

                            var thingList = intVec3.GetThingList(map);
                            if (thingList.All(thing => thing.def is
                                    { category: ThingCategory.Plant or ThingCategory.Item or ThingCategory.Pawn }))
                            {
                                FilthMaker.TryMakeFilth(intVec3, map, filth_RubbleRock);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void GeneratePostFog(Map map)
    {
        base.GeneratePostFog(map);
        foreach (var cell in tmpRidgeCells)
        {
            map.terrainGrid.SetTerrain(cell, TerrainDefOf.WaterOceanDeep);
        }
    }

    private class TileMutatorWorker_IslandCaves(TileMutatorDef def) : TileMutatorWorker_Caves(def)
    {
        protected override float BranchChance => 0f;
        protected override float WidthOffsetPerCell => Rand.Range(0.034f, 0.08f);
        protected override float MinTunnelWidth => Rand.Range(2.4f, 3.8f);
    }

    private class RoofThreshold
    {
        public RoofDef roofDef;

        public float minGridVal;
    }
}