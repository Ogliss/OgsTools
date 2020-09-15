using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TrapsRearmable
{
    // Token: 0x02000002 RID: 2
    public class JobDriver_RearmTrap : JobDriver
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return ReservationUtility.Reserve(this.pawn, this.job.targetA, this.job, 1, -1, null, true);
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002072 File Offset: 0x00000272
        protected override IEnumerable<Toil> MakeNewToils()
        {
            ToilFailConditions.FailOnDespawnedOrNull<JobDriver_RearmTrap>(this, TargetIndex.A);
            ToilFailConditions.FailOnThingMissingDesignation<JobDriver_RearmTrap>(this, TargetIndex.A, TrapsDefOf.TR_RearmTrap);
            Toil toil = new Toil();
            toil.initAction = delegate ()
            {
                this.pawn.pather.StartPath(base.TargetThingA, PathEndMode.Touch);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            ToilFailConditions.FailOnDespawnedNullOrForbidden<Toil>(toil, TargetIndex.A);
            yield return toil;
            yield return ToilEffects.WithProgressBarToilDelay(Toils_General.Wait(1125, 0), TargetIndex.A, false, -0.5f);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Thing thing = this.job.targetA.Thing;
                    Designation designation = base.Map.designationManager.DesignationOn(thing, TrapsDefOf.TR_RearmTrap);
                    if (designation != null)
                    {
                        designation.Delete();
                    }
                    (thing as Building_TrapRearmable).Rearm();
                    this.pawn.records.Increment(TrapsDefOf.TR_TrapsRearmed);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }

        // Token: 0x04000001 RID: 1
        private const int RearmTicks = 1125;
    }
}
