using Vehicles;

namespace MapVehicles;

public class VehicleTurret_RotateVehicle : VehicleTurret
{
    /// <summary>
    /// Init from CompProperties
    /// </summary>
    public VehicleTurret_RotateVehicle()
    {
    }

    /// <summary>
    /// Init from save file
    /// </summary>
    public VehicleTurret_RotateVehicle(VehiclePawn vehicle) : base(vehicle)
    {
    }

    /// <summary>
    /// Newly Spawned
    /// </summary>
    /// <param name="vehicle"></param>
    /// <param name="reference">VehicleTurret as defined in xml</param>
    public VehicleTurret_RotateVehicle(VehiclePawn vehicle, VehicleTurret reference) : base(vehicle, reference)
    {
    }
    
    protected override bool TurretRotationTick()
    {
        if (base.TurretRotationTick())
        {
            if (TurretTargetValid || vehicle.CompVehicleTurrets.Deploying)
            {
                RotateVehicle();
            }
            return true;
        }
        return false;
    }

    protected override bool TurretTargeterTick()
    {
        if (base.TurretTargeterTick())
        {
            RotateVehicle();
            return true;
        }
        return false;
    }

    private void RotateVehicle() => vehicle.Transform.rotation = TurretRotation - vehicle.FullRotation.AsAngle;
}