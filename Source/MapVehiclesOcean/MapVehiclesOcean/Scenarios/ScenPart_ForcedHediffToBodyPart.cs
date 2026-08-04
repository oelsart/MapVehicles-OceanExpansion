using System.Diagnostics.CodeAnalysis;
using RimWorld;
using UnityEngine;
using Verse;

namespace MapVehiclesOcean;

public class ScenPart_ForcedHediffToBodyPart : ScenPart_PawnModifier
{
  private HediffDef hediffDef;
  private BodyPartDef bodyPartDef;
  private FloatRange severityRange;
  
  private float MaxSeverity => hediffDef.lethalSeverity <= 0f ? 1f : hediffDef.lethalSeverity * 0.99f;

  public override void DoEditInterface(Listing_ScenEdit listing)
  {
    var scenPartRect = listing.GetScenPartRect(this, RowHeight * 3f + 31f);
    if (Widgets.ButtonText(scenPartRect.TopPartPixels(RowHeight), hediffDef.LabelCap))
    {
      FloatMenuUtility.MakeMenu(PossibleHediffs(), hd => hd.LabelCap, hd =>
        delegate
        {
          hediffDef = hd;
          if (severityRange.max > MaxSeverity)
          {
            severityRange.max = MaxSeverity;
          }

          if (severityRange.min > MaxSeverity)
          {
            severityRange.min = MaxSeverity;
          }
        });
    }

    Widgets.FloatRange(new Rect(scenPartRect.x, scenPartRect.y + RowHeight, scenPartRect.width, 31f),
      listing.CurHeight.GetHashCode(), ref severityRange, 0f, MaxSeverity, "ConfigurableSeverity");
    DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(RowHeight * 2f));
  }

  private static IEnumerable<HediffDef> PossibleHediffs()
  {
    return DefDatabase<HediffDef>.AllDefsListForReading.Where(x => x.scenarioCanAdd);
  }

  public override void ExposeData()
  {
    base.ExposeData();
    Scribe_Defs.Look(ref hediffDef, nameof(hediffDef));
    Scribe_Defs.Look(ref bodyPartDef, nameof(bodyPartDef));
    Scribe_Values.Look(ref severityRange, nameof(severityRange));
  }

  public override string Summary(Scenario scen)
  {
    return "ScenPart_PawnsHaveHediff"
      .Translate(context.ToStringHuman(), chance.ToStringPercent(), hediffDef.label).CapitalizeFirst();
  }

  public override void Randomize()
  {
    base.Randomize();
    hediffDef = PossibleHediffs().RandomElement();
    severityRange.max = Rand.Range(MaxSeverity * 0.2f, MaxSeverity * 0.95f);
    severityRange.min = severityRange.max * Rand.Range(0f, 0.95f);
  }

  public override bool TryMerge(ScenPart other)
  {
    if (other is ScenPart_ForcedHediffToBodyPart scenPart_ForcedHediff && hediffDef == scenPart_ForcedHediff.hediffDef)
    {
      chance = GenMath.ChanceEitherHappens(chance, scenPart_ForcedHediff.chance);
      return true;
    }

    return false;
  }

  public override bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
  {
    if (!base.AllowPlayerStartingPawn(pawn, tryingToRedress, req))
    {
      return false;
    }

    if (hideOffMap)
    {
      switch (req)
      {
        case { AllowDead: false, ForceDead: false } when
          pawn.health.WouldDieAfterAddingHediff(hediffDef, null, severityRange.max):
        case { AllowDowned: false, ForceDead: false } when
          pawn.health.WouldBeDownedAfterAddingHediff(hediffDef, null, severityRange.max):
          return false;
      }
    }

    return true;
  }

  protected override void ModifyNewPawn(Pawn p)
  {
    AddHediff(p);
  }

  protected override void ModifyHideOffMapStartingPawnPostMapGenerate(Pawn p)
  {
    AddHediff(p);
  }

  private void AddHediff(Pawn p)
  {
    if (Find.GameInitData != null && Find.GameInitData.QuickStarted && Prefs.DisableQuickStartCryptoSickness &&
        hediffDef == HediffDefOf.CryptosleepSickness)
    {
      return;
    }

    var hediff = HediffMaker.MakeHediff(hediffDef, p);
    hediff.Severity = severityRange.RandomInRange;
    p.health.AddHediff(hediff, p.health.hediffSet.GetBodyPartRecord(bodyPartDef));
  }

  public override bool HasNullDefs()
  {
    return base.HasNullDefs() || hediffDef == null;
  }

  [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
  public override int GetHashCode()
  {
    return base.GetHashCode() ^ (hediffDef != null ? hediffDef.GetHashCode() : 0) ^
           severityRange.GetHashCode();
  }
}