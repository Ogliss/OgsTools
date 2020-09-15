using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CompTurret
{
	// CompTurret.JobDriver_Reload

	public class JobDriver_ReloadCompTurret : JobDriver
	{
		private const TargetIndex GearInd = TargetIndex.A;

		private const TargetIndex AmmoInd = TargetIndex.B;

		private ThingWithComps Gear => job.GetTarget(TargetIndex.A).Thing as ThingWithComps;
		private ThingWithComps Gun => job.GetTarget(TargetIndex.C).Thing as ThingWithComps;
		private Thing Ammo => job.GetTargetQueue(TargetIndex.B)[0].Thing;
		private ThingDef ammoDef = null;
		private ThingDef AmmoDef
		{
			get
			{
				if (ammoDef == null)
				{
					ammoDef = Ammo.def;
				}
				return ammoDef;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => AmmoDef == null);
			CompTurretGun comp = null;
			if (true)
			{

			}
			foreach (var item in Gear?.AllComps)
			{
				if (item is CompTurretGun c)
				{
					if (c.Props.ammoDef == AmmoDef && c.gun == Gun)
					{
						comp = c;
						break;
					}
				}
			}
		//	CompApparel_Turret comp = Gear?.AllComps.Find(x=> x is CompApparel_Turret && ((CompApparel_Turret)(x)).Props.ammoDef == Ammo.def) as CompApparel_Turret;
			this.FailOn(() => comp == null);
			this.FailOn(() => comp.Wearer != pawn);
			this.FailOn(() => !comp.NeedsReload(allowForcedReload: true));
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			Toil getNextIngredient = Toils_General.Label();
			yield return getNextIngredient;
			foreach (Toil item in ReloadAsMuchAsPossible(comp))
			{
				yield return item;
			}
			yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
			foreach (Toil item2 in ReloadAsMuchAsPossible(comp))
			{
				yield return item2;
			}
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				if (carriedThing != null && !carriedThing.Destroyed)
				{
					pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out Thing _);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
		}

		private IEnumerable<Toil> ReloadAsMuchAsPossible(CompTurret comp)
		{
			Toil done = Toils_General.Label();
			yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null || pawn.carryTracker.CarriedThing.stackCount < comp.MinAmmoNeeded(allowForcedReload: true));
			yield return Toils_General.Wait(comp.Props.baseReloadTicks).WithProgressBarToilDelay(TargetIndex.A);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Thing carriedThing = pawn.carryTracker.CarriedThing;
				comp.ReloadFrom(carriedThing);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield return done;
		}
	}

}
