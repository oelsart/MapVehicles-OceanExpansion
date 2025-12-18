using RimWorld;
using UnityEngine;
using VehicleMapFramework;
using Verse;
using Verse.AI;

namespace MapVehicles;

[HotSwap]
public class JobDriver_Lookout : JobDriver_OperateScanner
{
    public override Vector3 ForcedBodyOffset
    {
        get
        {
            var comp = job.targetA.Thing.TryGetComp<CompAdditionalGraphicsChildByParent>();
            var rot = comp.parentThing.BaseRotationVehicleDraw();
            var offset = comp.Props.graphicsByParent[comp.parentThing.def][0].DrawOffsetForRot(rot);
            if (!rot.IsHorizontal) offset.z += 0.15f;
            if (comp.parentThing.IsOnNonFocusedVehicleMapOf(out var vehicle))
            {
                offset = offset.RotatedBy(-vehicle.FullAngle - vehicle.Angle);
            }
            return offset + new Vector3(0f, 0f, 1.35f);
        }
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var scannerComp = job.targetA.Thing.TryGetComp<CompScanner>();
        _ = base.MakeNewToils();
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        var work = ToilMaker.MakeToil();
        work.tickAction = () =>
        {
            var actor = work.actor;
            scannerComp.Used(actor);
        };
        work.AddFailCondition(() => !scannerComp.CanUseNow);
        work.defaultCompleteMode = ToilCompleteMode.Never;
        work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        yield return work;
    }
}