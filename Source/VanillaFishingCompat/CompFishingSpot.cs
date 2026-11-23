using Verse;
using CompFishingSpotBase = MapVehiclesOcean.CompFishingSpot;

namespace MapVehiclesOcean.VanillaFishingCompat;

public class CompFishingSpot : CompFishingSpotBase
{
	protected override Type Zone_FishingOnVehicle => typeof(Zone_FishingOnVehicle);

	protected override bool RequiredModActive => ModsConfig.IsActive("VanillaExpanded.VCEF");

	public override void CompTickRare()
  {
    base.CompTickRare();
    
    if (FishingCell.GetZone(parent.Map) is Zone_FishingOnVehicle zone)
	    zone.InitialSetZoneFishList();
  }
}