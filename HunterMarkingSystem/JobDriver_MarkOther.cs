using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace HunterMarkingSystem
{
    // Token: 0x02000089 RID: 137
    public class JobDriver_MarkOther : JobDriver
    {
        // Token: 0x06000390 RID: 912 RVA: 0x00024538 File Offset: 0x00022938
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.useDuration, "useDuration", 0, false);
        }

        // Token: 0x06000391 RID: 913 RVA: 0x00024554 File Offset: 0x00022954
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.useDuration = 150;
        }

        // Token: 0x06000392 RID: 914 RVA: 0x00024590 File Offset: 0x00022990
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo targetA = this.job.targetA;
            LocalTargetInfo targetB = this.job.targetB;
            Job job = this.job;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed);// && pawn.Reserve(targetB, job, 1, -1, null, errorOnFailed);
        }

        // Token: 0x06000393 RID: 915 RVA: 0x000245C8 File Offset: 0x000229C8
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = Toils_General.Wait(this.useDuration, TargetIndex.A);
            prepare.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            prepare.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return prepare;
            Toil use = new Toil();
            use.initAction = delegate ()
            {
                Pawn actor = use.actor;
                Pawn inductee = (Pawn)TargetB.Thing;
                Comp_Markable _Yautja = inductee.TryGetComp<Comp_Markable>();
            //    _Yautja.inducted = true;
            };
            use.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return use;
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            Toil prepare2 = Toils_General.Wait(this.useDuration, TargetIndex.B);
            prepare2.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            prepare2.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            prepare2.FailOnCannotTouch(TargetIndex.B, PathEndMode.Touch);
            yield return prepare2;
            Toil use2 = new Toil();
            use2.initAction = delegate ()
            {
                Pawn actor = use2.actor;
                Pawn inductee = (Pawn)TargetB.Thing;
                CompUseEffect_MarkSelf compUsable = actor.CurJob.targetA.Thing.TryGetComp<CompUseEffect_MarkSelf>();
                compUsable.DoEffect(inductee);
            };
            use2.defaultCompleteMode = ToilCompleteMode.Delay;
            yield return use2;
            yield break;
        }

        // Token: 0x0400024D RID: 589
        private int useDuration = -1;
    }
    
}
