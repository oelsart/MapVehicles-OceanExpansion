using System.Xml;
using JetBrains.Annotations;
using RimWorld;
using VehicleMapFramework;
using Vehicles;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class GenStep_Drifter : GenStep
{
    public override int SeedPart => 97878422;

    public List<VehicleWeight> vehicleDefs;

    public override void Generate(Map map, GenStepParams parms)
    {
        var pawns = parms.sitePart.things.OfType<Pawn>().ToList();
        if (pawns.Empty()) return;

        if (!vehicleDefs.TryRandomElementByWeight(v => v.weight, out var option)) return;
        var vehicle = VehicleSpawner.GenerateVehicle(option.vehicleDef, pawns[0].Faction);

        var rot = Rot4.Random;
        if (!CellFinderExtended.TryFindRandomCenterCell(map, c => GenSpawn.CanSpawnAt(option.vehicleDef, c, map, rot),
                out var cell))
            return;
        GenSpawn.Spawn(vehicle, cell, map, rot);

        if (vehicle is not VehiclePawnWithMap { VehicleMap: { } vehicleMap } vehiclePawnWithMap) return;
        if (vehiclePawnWithMap.GetComp<CompNpcVehicleMap>() is { } compNpcVehicleMap)
        {
            compNpcVehicleMap.SetParams(pawns.Count);
            var prefab = compNpcVehicleMap.Params.prefabDef;
            PrefabUtility.SpawnPrefab(prefab, vehicleMap, vehicleMap.Center, Rot4.North, vehicle.Faction);
            vehiclePawnWithMap.Resize();
        }
        
        foreach (var pawn in pawns)
        {
            pawn.mindState.WillJoinColonyIfRescued = true;
            pawn.mindState.duty = new PawnDuty(MVO_DefOf.MVO_WanderAnywhere, LocalTargetInfo.Invalid);
            if (CellFinder.TryFindRandomSpawnCellForPawnNear(vehicleMap.Center, vehicleMap, out var cell2))
            {
                GenSpawn.Spawn(pawn, cell2, vehicleMap);
            }
        }
    }

    public class VehicleWeight
    {
        public VehicleDef vehicleDef;
        public float weight;
        
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "vehicleDef", xmlRoot.Name);
            weight = ParseHelper.FromString<float>(xmlRoot.InnerText);
        }
    }
}