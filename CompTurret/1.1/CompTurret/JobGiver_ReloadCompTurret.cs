using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace CompTurret
{
	// CompTurret.JobGiver_ReloadCompTurret
	public class JobGiver_ReloadCompTurret : ThinkNode_JobGiver
	{
		// Token: 0x0600313A RID: 12602 RVA: 0x00114108 File Offset: 0x00112308
		public override float GetPriority(Pawn pawn)
		{
			return 5.9f;
		}

		// Token: 0x0600313B RID: 12603 RVA: 0x00114110 File Offset: 0x00112310
		public override Job TryGiveJob(Pawn pawn)
		{

			foreach (CompTurretGun compReloadable in CompTurretReloadableUtility.FindSomeReloadableComponents(pawn, false))
			{
				if (compReloadable == null)
				{
					continue;
				}
				List<Thing> list = CompTurretReloadableUtility.FindEnoughAmmo(pawn, pawn.Position, compReloadable, false);
				if (list == null)
				{
					continue;
				}
				return JobGiver_ReloadCompTurret.MakeReloadJob(compReloadable, list);
			}
			return null;
		}

		// Token: 0x0600313C RID: 12604 RVA: 0x00114148 File Offset: 0x00112348
		public static Job MakeReloadJob(CompTurretGun comp, List<Thing> chosenAmmo)
		{
			Job job = JobMaker.MakeJob(JobDefOf.CompTurretReload, comp.parent, null, targetC: comp.gun);
			job.targetQueueB = (from t in chosenAmmo
								select new LocalTargetInfo(t)).ToList<LocalTargetInfo>();
			job.count = chosenAmmo.Sum((Thing t) => t.stackCount);
			
			job.count = Math.Min(job.count, comp.MaxAmmoNeeded(true));
			return job;
		}

		// Token: 0x04001B62 RID: 7010
		private const bool forceReloadWhenLookingForWork = false;
	}
}
