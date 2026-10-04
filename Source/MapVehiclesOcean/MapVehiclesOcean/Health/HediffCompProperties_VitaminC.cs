using System.Xml;
using JetBrains.Annotations;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class HediffCompProperties_VitaminC : HediffCompProperties
{
	public List<VitaminSource> vitaminSources;

	public HediffCompProperties_VitaminC()
	{
		compClass = typeof(HediffComp_VitaminC);
	}
	
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class VitaminSource
	{
		public ThingDef source;
		public float amount;
		
		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			XmlHelper.ParseElements(this, xmlRoot, "source", "amount");
		}
	}
}