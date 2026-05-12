using System.Text;
using RimWorld;
using RimWorld.Planet;
using Vehicles.World;
using Verse;

namespace MapVehiclesOcean;

public class CompTradeGoods : ThingComp
{
    private PlanetTile? tile;
    private string traderName;

    protected CompProperties_TradeGoods Props => (CompProperties_TradeGoods)props;
    
    public override string TransformLabel(string label)
    {
        if (traderName is null) return label;
        return "MVO_TradeGoods".Translate(traderName, label);
    }

    public override void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
    {
        if (EvaluateDistance(stat, out var distance, out var factor))
        {
            // ReSharper disable once SimplifyStringInterpolation Boxingを避けるため
            sb.AppendLine($"{"StatsReport_MultiplierFor".Translate("distance".Translate()).ToString()} {distance.ToString("F0")}: x{factor.ToStringPercent()}");
        }
    }

    public override void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
    {
        if (action == TradeAction.PlayerBuys && tile is null)
        {
            tile = trader switch
            {
                WorldObject worldObject => worldObject.Tile,
                Pawn pawn => pawn.Tile,
                TradeShip => playerNegotiator.Tile,
                _ => null
            };
            traderName = trader switch
            {
                WorldObject worldObject => worldObject.LabelShort,
                _ => trader.TraderName
            };
            parent.overrideGraphicIndex ??= parent.thingIDNumber; // Splitされた後もグラフィックを維持するため
        }
    }

    public override float GetStatFactor(StatDef stat)
    {
        EvaluateDistance(stat, out _, out var factor);
        return factor;
    }

    private bool EvaluateDistance(StatDef stat, out float distance, out float factor)
    {
        if (stat == StatDefOf.MarketValue && tile is { Valid: true } && parent.Tile.Valid)
        {
            distance = WorldHelper.GetTileDistance(tile.Value, parent.Tile);
            factor = Props.distanceFactorCurve.Evaluate(distance);
            return true;
        }
        distance = 0f;
        factor = 1f;
        return false;
    }

    public override bool AllowStackWith(Thing other)
    {
        return other.TryGetComp<CompTradeGoods>(out var comp) &&
               tile == comp.tile &&
               traderName == comp.traderName &&
               parent.overrideGraphicIndex == other.overrideGraphicIndex;
    }

    public override void PostSplitOff(Thing piece)
    {
        if (piece.TryGetComp<CompTradeGoods>(out var comp))
        {
            comp.tile = tile;
            comp.traderName = traderName;
            piece.overrideGraphicIndex = parent.overrideGraphicIndex ?? parent.thingIDNumber;
        }
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref tile, nameof(tile));
        Scribe_Values.Look(ref traderName, nameof(traderName));
    }
}