using System.Xml;
using JetBrains.Annotations;
using RimWorld;
using XmlHelper = Verse.XmlHelper;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_CrowsNest : CompProperties_Scanner
{
	public int range;
  public List<QuestWeight> questWeights;

  public CompProperties_CrowsNest()
  {
    compClass = typeof(CompCrowsNest);
  }

  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public class QuestWeight
  {
    public QuestScriptDef quest;
    public float weight;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
      XmlHelper.ParseElements(this, xmlRoot, "quest", "weight");
    }
  }
}