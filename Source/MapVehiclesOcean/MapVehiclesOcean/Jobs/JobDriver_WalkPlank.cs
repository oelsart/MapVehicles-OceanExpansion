using RimWorld;
using UnityEngine;
using VehicleMapFramework;
using Verse;
using Verse.AI;

namespace MapVehiclesOcean;

public class JobDriver_WalkPlank : JobDriverBodyOffset
{
  private const int WalkDuration = 900;
  private const int WatchDuration = 120;

  protected Thing Gangplank => TargetA.Thing;
  
  protected bool IsWalking => CurToilIndex == 2;
  
  protected bool IsWatching => CurToilIndex == 4;

  public override bool TryMakePreToilReservations(bool errorOnFailed)
  {
    return pawn.Reserve(Gangplank.Map, TargetA, job, job.def.joyMaxParticipants, 0) &&
           pawn.Reserve(Gangplank.Map, TargetB, job);
  }

  protected override string ReportStringProcessed(string str)
  {
    if (IsWalking)
    {
      return "MVO_JobReport_WalkingPlank".Translate();
    }

    if (IsWatching)
    {
      return "MVO_JobReport_WatchingWalkingPlank".Translate();
    }

    return base.ReportStringProcessed(str);
  }

  protected override IEnumerable<Toil> MakeNewToils()
  {
    this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
    this.FailOnBurningImmobile(TargetIndex.A);
    yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
    
    var chooseRole = ToilMaker.MakeToil();
    var walkPlank = ToilMaker.MakeToil();
    var watchPlank = ToilMaker.MakeToil();
    
    chooseRole.initAction = () =>
    {
      if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
      {
        EndJobWith(JobCondition.Succeeded);
        return;
      }

      var participants = GetParticipants();
      try
      {
        var someoneWalking = false;

        foreach (var p in participants)
        {
          if (p.jobs?.curDriver is JobDriver_WalkPlank { IsWalking: true })
          {
            someoneWalking = true;
            break;
          }
        }

        if (!someoneWalking)
        {
          var selected = participants.RandomElementByWeightWithFallback(_ => 1f);
          if (selected == pawn)
          {
            return; // 自分に当選したため walkPlank へ進む
          }
        }

        JumpToToil(watchPlank);
      }
      finally
      {
        participants.Clear();
        SimplePool<List<Pawn>>.Return(participants);
      }
    };
    chooseRole.defaultCompleteMode = ToilCompleteMode.Instant;

    yield return chooseRole;

    // 板を歩くToil
    walkPlank.defaultCompleteMode = ToilCompleteMode.Delay;
    walkPlank.defaultDuration = WalkDuration;
    walkPlank.handlingFacing = true;
    walkPlank.socialMode = RandomSocialMode.SuperActive;

    walkPlank.initAction = () =>
    {
      job.locomotionUrgency = LocomotionUrgency.Walk;

      // 位置の入れ替え
      var plank = Gangplank;
      var actor = walkPlank.actor;
      var map = plank.Map;
      if (plank.Position != actor.Position)
      {
        map.reservationManager.Release(actor.Position, actor, job);
        
        var firstPawn = plank.Position.GetFirstPawn(map);
        if (firstPawn != null && firstPawn.CurJobDef == job.def)
        {
          map.reservationManager.Release(firstPawn.Position, firstPawn, firstPawn.CurJob);
          map.reservationManager.Reserve(firstPawn, firstPawn.CurJob, actor.Position);
          firstPawn.pather.StartPath(actor.Position, PathEndMode.OnCell);
        }
        
        map.reservationManager.Reserve(actor, job, plank.Position);
        actor.pather.StartPath(plank.Position, PathEndMode.OnCell);
      }
    };

    walkPlank.tickAction = () =>
    {
      var ticksPassed = WalkDuration - ticksLeftThisToil;
      var progress = (float)ticksPassed / WalkDuration;
      var length = Gangplank.TryGetComp<CompGangplank>()?.Props.length - 0.5f ?? 2f;

      // 先端で一時停止するタイムライン（0.38〜0.62で先端滞在）
      float walkProgress;
      var isAtTip = false;

      switch (progress)
      {
        case < 0.38f:
        {
          var p = progress / 0.38f;
          walkProgress = Mathf.Sin(p * (Mathf.PI / 2f));
          break;
        }
        case <= 0.62f:
          walkProgress = 1f;
          isAtTip = true;
          break;
        default:
        {
          var p = (1f - progress) / 0.38f;
          walkProgress = Mathf.Sin(p * (Mathf.PI / 2f));
          break;
        }
      }

      var baseDistance = walkProgress * length;

      // 先端では進退のランダム動作を抑え、移動中は歩みの緩急をつける
      var distNoiseFactor = isAtTip ? 0.1f : 0.8f;
      var ticks = GenTicks.TicksGame;
      var distNoise = (Mathf.PerlinNoise1D(ticks * 0.012f) - 0.5f) * distNoiseFactor;
      var targetDistance = Mathf.Clamp(baseDistance + distNoise, 0f, length);

      // 先端では左右の揺れ（バランス取り）をやや強めにする
      var wobbleAmp = isAtTip ? 0.12f : 0.10f;
      var wobbleFreq = isAtTip ? 0.04f : 0.025f;
      var wobbleNoise = (Mathf.PerlinNoise1D(ticks * wobbleFreq) - 0.5f) * wobbleAmp;

      var forward = Gangplank.Rotation.FacingCell.ToVector3();
      var side = Gangplank.Rotation.RighthandCell.ToVector3();
      var targetOffset = forward * targetDistance + side * wobbleNoise;
      drawOffset = Vector3.Lerp(drawOffset, targetOffset, 0.15f);
      if (walkPlank.actor.IsOnNonFocusedVehicleMapOf(out var vehicle))
        drawOffset = drawOffset.RotatedBy(vehicle.FullAngle);

      pawn.rotationTracker.FaceCell(Gangplank.Position + Gangplank.Rotation.FacingCell);
    };
    walkPlank.tickIntervalAction = delta =>
    {
      if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
      {
        EndJobWith(JobCondition.Succeeded);
      }
      else
      {
        JoyUtility.JoyTickCheckEnd(pawn, delta, joySource: Gangplank as Building);
      }
    };

    walkPlank.AddFinishAction(() =>
    {
      drawOffset = Vector3.zero;
      JoyUtility.TryGainRecRoomThought(pawn);
    });

    yield return walkPlank;
    yield return Toils_Jump.Jump(chooseRole);

    // 観覧するToil
    watchPlank.defaultCompleteMode = ToilCompleteMode.Delay;
    watchPlank.defaultDuration = WatchDuration;
    watchPlank.handlingFacing = true;
    watchPlank.socialMode = RandomSocialMode.SuperActive;

    watchPlank.tickIntervalAction = delta =>
    {
      drawOffset = Vector3.zero;
      pawn.rotationTracker.FaceCell(Gangplank.Position);

      if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
      {
        EndJobWith(JobCondition.Succeeded);
      }
      else
      {
        JoyUtility.JoyTickCheckEnd(pawn, delta, joySource: Gangplank as Building);
      }
    };

    watchPlank.AddFinishAction(() =>
    {
      JoyUtility.TryGainRecRoomThought(pawn);
    });

    yield return watchPlank;
    yield return Toils_Jump.Jump(chooseRole);
  }

  private List<Pawn> GetParticipants()
  {
    var plank = Gangplank;
    
    var list = SimplePool<List<Pawn>>.Get();
    if (pawn.Map == null || Gangplank == null)
      return list;

    foreach (var reservation in plank.Map.reservationManager.ReservationsReadOnly)
    {
      if (reservation.Job?.def == job.def && reservation.Target == Gangplank)
      {
        list.Add(reservation.Claimant);
      }
    }

    return list;
  }
}