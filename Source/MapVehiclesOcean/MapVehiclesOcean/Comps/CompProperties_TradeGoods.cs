using JetBrains.Annotations;
using Verse;

namespace MapVehiclesOcean;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CompProperties_TradeGoods : CompProperties
{
  public CompProperties_TradeGoods()
  {
    compClass = typeof(CompTradeGoods);
  }

  public SimpleCurve distanceFactorCurve;
}