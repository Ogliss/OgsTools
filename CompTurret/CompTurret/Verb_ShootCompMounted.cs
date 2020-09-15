using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CompTurret
{
	// 1168 CompTurret.Verb_ShootCompMounted
	public class Verb_ShootCompMounted : Verb_LaunchProjectile
	{
		protected override int ShotsPerBurst
		{
			get
			{
				return this.verbProps.burstShotCount;
			}
		}
		public CompTurret turret;
		public CompTurretGun turretGun => turret as CompTurretGun ?? ReloadableCompSource as CompTurretGun;

		public float barrellength => turretGun.Props.barrellength;
		public float offset => turretGun.Props.projectileOffset;
		public override Thing Caster
		{
			get
			{
				Apparel a = this.caster as Apparel;
				if (a != null)
				{
					if (a.Wearer != null)
					{
						if (this.caster != a.Wearer)
						{
						//	Log.Message("New Wearer "+ a.Wearer);
							this.caster = a.Wearer;
						}
					}
					else
					{
					//	Log.Message("caster is Apparel Not worn");
					}
				}
				else
				{
				//	Log.Message("caster is not Apparel "+caster);
				}
				return this.caster;
			}
		}

		public new CompTurret ReloadableCompSource
		{
			get
			{
				return this.DirectOwner as CompTurret;
			}
		}
		public override bool CasterIsPawn
		{
			get
			{
			//	Log.Message("Caster is Pawn ? " + (Caster is Pawn));
				return Caster is Pawn;
			}
		}
		public override Pawn CasterPawn => Caster as Pawn;

		public override void DrawHighlight(LocalTargetInfo target)
		{
			this.verbProps.DrawRadiusRing(this.Caster.Position);
			if (target.IsValid)
			{
				GenDraw.DrawTargetHighlight(target);
				this.DrawHighlightFieldRadiusAroundTarget(target);
			}
		}
		/*
		public override void WarmupComplete()
		{
		//	Log.Message("WarmupComplete");
			base.WarmupComplete();
			Pawn pawn = this.currentTarget.Thing as Pawn;
			if (pawn != null && !pawn.Downed && this.CasterIsPawn && this.CasterPawn.skills != null)
			{
				float num = pawn.HostileTo(this.Caster) ? 170f : 20f;
				float num2 = this.verbProps.AdjustedFullCycleTime(this, this.CasterPawn);
				this.CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2, false);
			}
		}
		*/
		public int warningticks = 0;

		protected override bool TryCastShot()
		{
		//	Log.Message("TryCastShot ");
			if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.Caster.Map)
			{
			//	Log.Message("TGT Wrong Map");
				return false;
			}
			ThingDef projectile = this.Projectile;
			if (projectile == null)
			{
			//	Log.Message("projectile == null");
				return false;
			}
			ShootLine shootLine;
			bool flag = base.TryFindShootLineFromTo(this.Caster.Position, this.currentTarget, out shootLine);
			if (this.verbProps.stopBurstWithoutLos && !flag)
			{
			//	Log.Message("stopBurstWithoutLos");
				return false;
			}
			Vector3 muzzlePos;
		//	Log.Message("TryCastShot 1");
		/*
			if (base.EquipmentSource != null)
			{
				CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
				if (comp != null)
				{
					comp.Notify_ProjectileLaunched();
				}
				CompReloadable comp2 = base.EquipmentSource.GetComp<CompReloadable>();
				if (comp2 != null)
				{
					comp2.UsedOnce();
				}
			}
			*/
		//	Log.Message("TryCastShot 2");
			if (turretGun != null)
			{
				if (turretGun.UseAmmo)
				{
					bool playerpawn = this.CasterIsPawn && this.Caster.Faction == Faction.OfPlayer;
					if (turretGun.HasAmmo)
					{
						turretGun.UsedOnce();
					}
					else
					{
						return false;
					}
					if (turretGun.RemainingCharges == 0)
					{
						if (turretGun.Props.soundEmptyWarning != null && playerpawn)
						{
							turretGun.Props.soundEmptyWarning.PlayOneShot(new TargetInfo(this.Caster.Position, this.Caster.Map, false));
						}
						if (!turretGun.Props.messageEmptyWarning.NullOrEmpty() && playerpawn)
						{
							MoteMaker.ThrowText(Caster.Position.ToVector3(), Caster.Map, turretGun.Props.messageEmptyWarning.Translate(EquipmentSource.LabelCap, Caster.LabelShortCap), 3f);
						}
					}
					float a = turretGun.RemainingCharges;
					float b = turretGun.MaxCharges;
					int remaining = (int)(a / b * 100f);
					if (remaining == 50 && warningticks == 0)
					{
						warningticks = this.verbProps.ticksBetweenBurstShots+1;
						if (turretGun.Props.soundHalfRemaningWarning != null && playerpawn)
						{
							turretGun.Props.soundHalfRemaningWarning.PlayOneShot(new TargetInfo(this.Caster.Position, this.Caster.Map, false));
						}
						if (!turretGun.Props.messageHalfRemaningWarning.NullOrEmpty() && playerpawn)
						{
							MoteMaker.ThrowText(Caster.Position.ToVector3(), Caster.Map, turretGun.Props.messageHalfRemaningWarning.Translate(EquipmentSource.LabelCap, Caster.LabelShortCap, remaining), 3f);
						}
					}
					if (remaining == 25 && warningticks == 0)
					{
						warningticks = this.verbProps.ticksBetweenBurstShots + 1;
						if (turretGun.Props.soundQuaterRemaningWarning != null && playerpawn)
						{
							turretGun.Props.soundQuaterRemaningWarning.PlayOneShot(new TargetInfo(this.Caster.Position, this.Caster.Map, false));
						}
						if (!turretGun.Props.messageQuaterRemaningWarning.NullOrEmpty() && playerpawn)
						{
							MoteMaker.ThrowText(Caster.Position.ToVector3(), Caster.Map, turretGun.Props.messageQuaterRemaningWarning.Translate(EquipmentSource.LabelCap, Caster.LabelShortCap, remaining), 3f);
						}
					}
					muzzlePos = MuzzlePosition(this.Caster, this.currentTarget, offset);
				}
			}
			else
			{
				Log.Error(Caster+"'s "+this +" has no Comp_Turret");
				return false;
			}
		//	Log.Message("TryCastShot 3");
			Thing launcher = this.Caster;
			Thing equipment = base.EquipmentSource;
		//	Log.Message("TryCastShot 4");
			Vector3 drawPos = this.Caster.DrawPos;
		//	Log.Message("TryCastShot 4b");
			Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.Caster.Map, WipeMode.Vanish);
		//	Log.Message("TryCastShot 4c");
			if (this.verbProps.forcedMissRadius > 0.5f)
			{
			//	Log.Message("TryCastShot 4 1");
				float num = VerbUtility.CalculateAdjustedForcedMiss(this.verbProps.forcedMissRadius, this.currentTarget.Cell - this.Caster.Position);
			//	Log.Message("TryCastShot 4 2");
				if (num > 0.5f)
				{
				//	Log.Message("TryCastShot 4 2 1");
					int max = GenRadial.NumCellsInRadius(num);
					int num2 = Rand.Range(0, max);
				//	Log.Message("TryCastShot 4 2 2");
					if (num2 > 0)
					{
					//	Log.Message("TryCastShot 4 2 2 1");
						IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num2];
						this.ThrowDebugText("ToRadius");
						this.ThrowDebugText("Rad\nDest", c);
						ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
					//	Log.Message("TryCastShot 4 2 2 2");
						if (Rand.Chance(0.5f))
						{
							projectileHitFlags = ProjectileHitFlags.All;
						}
					//	Log.Message("TryCastShot 4 2 2 3");
						if (!this.canHitNonTargetPawnsNow)
						{
							projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
						}
					//	Log.Message("TryCastShot 4 2 2 4 Caster" + this.Caster);
					//	Log.Message("TryCastShot 4 2 2 4 currentTarget " + this.currentTarget);
					//	Log.Message("TryCastShot 4 2 2 4 offset " + offset);
						muzzlePos = MuzzlePosition(this.Caster, this.currentTarget, offset);
					//	Log.Message("TryCastShot 4 2 2 5");
						projectile2.Launch(launcher, muzzlePos, c, this.currentTarget, projectileHitFlags, equipment, null);
					//	Log.Message("TryCastShot 4 2 2 6");
						if (this.CasterIsPawn)
						{
							this.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
						}
					//	Log.Message("TryCastShot 4 2 2 7");
						//	Log.Message("TryCastShot 1");
						return true;
					}
				//	Log.Message("TryCastShot 4 2 3");
				}
			}
		//	Log.Message("TryCastShot 5");
			ShotReport shotReport = ShotReport.HitReportFor(this.Caster, this, this.currentTarget);
		//	Log.Message("TryCastShot 5 1");
			Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
		//	Log.Message("TryCastShot 5 2");
			ThingDef targetCoverDef = (randomCoverToMissInto != null) ? randomCoverToMissInto.def : null;
		//	Log.Message("TryCastShot 5 3");
			if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
			{
				shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
				this.ThrowDebugText("ToWild" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
				this.ThrowDebugText("Wild\nDest", shootLine.Dest);
				ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
				if (Rand.Chance(0.5f) && this.canHitNonTargetPawnsNow)
				{
					projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
				}
			//	Log.Message("TryCastShot 5 3 6 launcher " + launcher);
			//	Log.Message("TryCastShot 5 3 6 Caster " + this.Caster);
			//	Log.Message("TryCastShot 5 3 6 currentTarget " + this.currentTarget);
			//	Log.Message("TryCastShot 5 3 6 shootLine.Dest " + shootLine.Dest);
			//	Log.Message("TryCastShot 5 3 6 offset " + offset);
				muzzlePos = MuzzlePosition(this.Caster, this.currentTarget, offset);
			//	Log.Message("TryCastShot 5 3 7");
				projectile2.Launch(launcher, muzzlePos, shootLine.Dest, this.currentTarget, projectileHitFlags2, equipment, targetCoverDef);

			//	Log.Message("TryCastShot 5 3 7b");
				if (this.CasterIsPawn)
				{
					this.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
				}
			//	Log.Message("TryCastShot 5 3 8");
				//	Log.Message("TryCastShot 2");
				return true;
			}
		//	Log.Message("TryCastShot 6");
			if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
			{
				this.ThrowDebugText("ToCover" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
				this.ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
				ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
				if (this.canHitNonTargetPawnsNow)
				{
					projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
				}
				muzzlePos = MuzzlePosition(this.Caster, this.currentTarget, offset);
				projectile2.Launch(launcher, muzzlePos, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, equipment, targetCoverDef);
				if (this.CasterIsPawn)
				{
					this.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
				}
				return true;
			}
		//	Log.Message("TryCastShot 7");
			ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
			if (this.canHitNonTargetPawnsNow)
			{
				projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
			}
		//	Log.Message("TryCastShot 8");
			if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
			{
				projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
			}
		//	Log.Message("TryCastShot 9");
			this.ThrowDebugText("ToHit" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
		//	Log.Message("TryCastShot 10");
			muzzlePos = MuzzlePosition(this.Caster, this.currentTarget, offset);
			if (this.currentTarget.Thing != null)
			{
				projectile2.Launch(launcher, muzzlePos, this.currentTarget, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
				this.ThrowDebugText("Hit\nDest", this.currentTarget.Cell);
			}
			else
			{
				projectile2.Launch(launcher, muzzlePos, shootLine.Dest, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
				this.ThrowDebugText("Hit\nDest", shootLine.Dest);
			}

		//	Log.Message("TryCastShot 11");
			if (this.CasterIsPawn)
			{
				this.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
			}
		//	Log.Message("TryCastShot 12");
			return true;
		}

		public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true)
		{
		//	Log.Message("TryStartCastOn 0");
			if (this.Caster == null)
			{
				Log.Error("Verb " + this.GetUniqueLoadID() + " needs Caster to work (possibly lost during saving/loading).", false);
				return false;
			}
		//	Log.Message("TryStartCastOn Bursting: "+ (this.state == VerbState.Bursting) + " Can Hit tgt: "+ (this.CanHitTarget(castTarg)));
			if (this.state == VerbState.Bursting || !this.CanHitTarget(castTarg))
			{
				return false;
			}
			if (turretGun != null)
			{
				if (turretGun.UseAmmo)
				{
					if (turretGun.RemainingCharges<=0)
					{
						return false;
					}
				}
			}
		//	Log.Message("TryStartCastOn 3");
			if (this.CausesTimeSlowdown(castTarg))
			{
				Find.TickManager.slower.SignalForceNormalSpeed();
			}
		//	Log.Message("TryStartCastOn 4");
			this.surpriseAttack = surpriseAttack;
			this.canHitNonTargetPawnsNow = canHitNonTargetPawns;
			this.currentTarget = castTarg;
			this.currentDestination = destTarg;
		//	Log.Message("TryStartCastOn 5");
			/*
			if (this.CasterIsPawn && this.verbProps.warmupTime > 0f)
			{
				Log.Message("TryStartCastOn DoWarmup");
				ShootLine newShootLine;
				if (!this.TryFindShootLineFromTo(this.caster.Position, castTarg, out newShootLine))
				{
					Log.Message("TryStartCastOn No LOS");
					return false;
				}
				this.CasterPawn.Drawer.Notify_WarmingCastAlongLine(newShootLine, this.caster.Position);
				float statValue = this.CasterPawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
				int ticks = (this.verbProps.warmupTime * statValue).SecondsToTicks();
				this.CasterPawn.stances.SetStance(new Stance_Warmup(ticks, castTarg, this));
			}
			else
			{
				Log.Message("TryStartCastOn WarmupComplete");
				this.WarmupComplete();
			}
			*/

		//	Log.Message("TryStartCastOn WarmupComplete");
			this.WarmupComplete();
		//	Log.Message("TryStartCastOn 6");
			return true;
		}

		protected new void TryCastNextBurstShot()
		{
			LocalTargetInfo localTargetInfo = this.currentTarget;
			if (this.Available() && this.TryCastShot())
			{
				if (this.verbProps.muzzleFlashScale > 0.01f)
				{
					MoteMaker.MakeStaticMote(MuzzlePosition(this.Caster, this.currentTarget, this.turretGun.Props.projectileOffset), this.caster.Map, ThingDefOf.Mote_ShotFlash, this.verbProps.muzzleFlashScale);
				}
				if (this.verbProps.soundCast != null)
				{
					this.verbProps.soundCast.PlayOneShot(new TargetInfo(this.caster.Position, this.caster.Map, false));
				}
				if (this.verbProps.soundCastTail != null)
				{
					this.verbProps.soundCastTail.PlayOneShotOnCamera(this.caster.Map);
				}
				if (this.CasterIsPawn)
				{
					if (this.CasterPawn.thinker != null)
					{
						Notify_EngagedTarget();
					}
					if (this.CasterPawn.mindState != null)
					{
						Notify_AttackedTarget(localTargetInfo);
					}
					if (this.CasterPawn.MentalState != null)
					{
						this.CasterPawn.MentalState.Notify_AttackedTarget(localTargetInfo);
					}
					if (this.TerrainDefSource != null)
					{
						this.CasterPawn.meleeVerbs.Notify_UsedTerrainBasedVerb();
					}
					if (this.CasterPawn.health != null)
					{
						this.CasterPawn.health.Notify_UsedVerb(this, localTargetInfo);
					}
					if (this.EquipmentSource != null)
					{
						this.EquipmentSource.Notify_UsedWeapon(this.CasterPawn);
					}
					if (!this.CasterPawn.Spawned)
					{
						this.Reset();
						return;
					}
				}
				if (this.verbProps.consumeFuelPerShot > 0f)
				{
					CompRefuelable compRefuelable = this.caster.TryGetComp<CompRefuelable>();
					if (compRefuelable != null)
					{
						compRefuelable.ConsumeFuel(this.verbProps.consumeFuelPerShot);
					}
				}
				this.burstShotsLeft--;
			}
			else
			{
				this.burstShotsLeft = 0;
			}
			if (this.burstShotsLeft > 0)
			{
				this.ticksToNextBurstShot = this.verbProps.ticksBetweenBurstShots;
				if (this.CasterIsPawn && !this.verbProps.nonInterruptingSelfCast)
				{
					this.CasterPawn.stances.SetStance(new Stance_Cooldown(this.verbProps.ticksBetweenBurstShots + 1, this.currentTarget, this));
					return;
				}
			}
			else
			{
				this.state = VerbState.Idle;
				if (this.CasterIsPawn && !this.verbProps.nonInterruptingSelfCast)
				{
					this.CasterPawn.stances.SetStance(new Stance_Cooldown(this.verbProps.AdjustedCooldownTicks(this, this.CasterPawn), this.currentTarget, this));
				}
				if (this.castCompleteCallback != null)
				{
					this.castCompleteCallback();
				}
			}
		}

		// Token: 0x060028BD RID: 10429 RVA: 0x000F09F4 File Offset: 0x000EEBF4
		internal void Notify_EngagedTarget()
		{
			this.CasterPawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
		}

		// Token: 0x060028BE RID: 10430 RVA: 0x000F0A06 File Offset: 0x000EEC06
		internal void Notify_AttackedTarget(LocalTargetInfo target)
		{
			this.CasterPawn.mindState.lastAttackTargetTick = Find.TickManager.TicksGame;
			this.CasterPawn.mindState.lastAttackedTarget = target;
		}
		// Token: 0x06002133 RID: 8499 RVA: 0x000CB158 File Offset: 0x000C9358
		public Vector3 MuzzlePosition(Thing shooter, LocalTargetInfo target, float offsetDist)
		{
			float facing = 0f;
			if (target.Cell != shooter.Position)
			{
				if (target.Thing != null)
				{
					facing = (target.Thing.DrawPos - this.turretGun.TurretPos).AngleFlat();
				}
				else
				{
					facing = (target.Cell.ToVector3() - this.turretGun.TurretPos).AngleFlat();
				}
			}
			return MuzzlePositionRaw(this.turretGun.TurretPos + new Vector3(0f, offsetDist, 0f), facing);
		}

		// Token: 0x06002134 RID: 8500 RVA: 0x000CB1EC File Offset: 0x000C93EC
		public static Vector3 MuzzlePositionRaw(Vector3 center, float facing)
		{
			center += Quaternion.AngleAxis(facing, Vector3.up) * Vector3.forward * 0.8f;
			return center;
		}
		public new void VerbTick()
		{
			if (this.state == VerbState.Bursting)
			{
				if (!this.Caster.Spawned)
				{
					this.Reset();
					return;
				}
				this.ticksToNextBurstShot--;
				if (this.ticksToNextBurstShot <= 0)
				{
					this.TryCastNextBurstShot();
				}
			}
			if (warningticks > 0)
			{
				warningticks--;
			}
		}

		public override bool ValidateTarget(LocalTargetInfo target)
		{
			Pawn p;
			return !this.CasterIsPawn || (p = (target.Thing as Pawn)) == null || (!p.InSameExtraFaction(this.Caster as Pawn, ExtraFactionType.HomeFaction, null) && !p.InSameExtraFaction(this.Caster as Pawn, ExtraFactionType.MiniFaction, null));
		}

		public override bool CanHitTarget(LocalTargetInfo targ)
		{
		//	Log.Message("CanHitTarget ");
			if (this.Caster is Apparel app)
			{
			//	Log.Message("CanHitTarget app");
				if (app.Wearer != null)
				{
				//	Log.Message("CanHitTarget Wearer spawned: "+ app.Wearer.Spawned+" Selftarget: " + (targ == app.Wearer)+ " CanHitFrom: "+(this.CanHitTargetFrom(app.Wearer.Position, targ)));
					return app.Wearer.Spawned && (targ == app.Wearer || this.CanHitTargetFrom(app.Wearer.Position, targ));
				}

			}
			return this.Caster != null && this.Caster.Spawned && (targ == this.Caster || this.CanHitTargetFrom(this.Caster.Position, targ));
		}

		public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
		{
			ShootLine shootLine;
			if (this.Caster is Apparel app)
			{
			//	Log.Message("CanHitTargetFrom app");
				if (app.Wearer != null)
				{
				}

			}
			if (targ.Thing != null && targ.Thing == this.Caster)
			{
				return this.targetParams.canTargetSelf;
			}
			return !this.ApparelPreventsShooting(root, targ) && this.TryFindShootLineFromTo(root, targ, out shootLine);
		}

		private bool CausesTimeSlowdown(LocalTargetInfo castTarg)
		{
			if (!this.verbProps.CausesTimeSlowdown)
			{
				return false;
			}
			if (!castTarg.HasThing)
			{
				return false;
			}
			Thing thing = castTarg.Thing;
			if (thing.def.category != ThingCategory.Pawn && (thing.def.building == null || !thing.def.building.IsTurret))
			{
				return false;
			}
			Pawn pawn = thing as Pawn;
			bool flag = pawn != null && pawn.Downed;
			return (thing.Faction == Faction.OfPlayer && this.Caster.HostileTo(Faction.OfPlayer)) || (this.Caster.Faction == Faction.OfPlayer && thing.HostileTo(Faction.OfPlayer) && !flag);
		}

		// Verse.Verb
		// Token: 0x060022E2 RID: 8930 RVA: 0x000D4A4C File Offset: 0x000D2C4C
		public new bool TryFindShootLineFromTo(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine)
		{
			if (targ.HasThing && targ.Thing.Map != this.Caster.Map)
			{
				resultingLine = default(ShootLine);
				return false;
			}
			if (this.verbProps.IsMeleeAttack || this.EffectiveRange <= 1.42f)
			{
				resultingLine = new ShootLine(root, targ.Cell);
				return ReachabilityImmediate.CanReachImmediate(root, targ, this.Caster.Map, PathEndMode.Touch, null);
			}
			CellRect cellRect = targ.HasThing ? targ.Thing.OccupiedRect() : CellRect.SingleCell(targ.Cell);
			float num = this.verbProps.EffectiveMinRange(targ, this.Caster);
			float num2 = cellRect.ClosestDistSquaredTo(root);
			if (num2 > this.EffectiveRange * this.EffectiveRange || num2 < num * num)
			{
				resultingLine = new ShootLine(root, targ.Cell);
				return false;
			}
			if (!this.verbProps.requireLineOfSight)
			{
				resultingLine = new ShootLine(root, targ.Cell);
				return true;
			}
			if (this.CasterIsPawn)
			{
				IntVec3 dest;
				if (this.CanHitFromCellIgnoringRange(root, targ, out dest))
				{
					resultingLine = new ShootLine(root, dest);
					return true;
				}
				ShootLeanUtility.LeanShootingSourcesFromTo(root, cellRect.ClosestCellTo(root), this.Caster.Map, Verb_ShootCompMounted.tempLeanShootSources);
				for (int i = 0; i < Verb_ShootCompMounted.tempLeanShootSources.Count; i++)
				{
					IntVec3 intVec = Verb_ShootCompMounted.tempLeanShootSources[i];
					if (this.CanHitFromCellIgnoringRange(intVec, targ, out dest))
					{
						resultingLine = new ShootLine(intVec, dest);
						return true;
					}
				}
			}
			else
			{
				foreach (IntVec3 intVec2 in this.Caster.OccupiedRect())
				{
					IntVec3 dest;
					if (this.CanHitFromCellIgnoringRange(intVec2, targ, out dest))
					{
						resultingLine = new ShootLine(intVec2, dest);
						return true;
					}
				}
			}
			resultingLine = new ShootLine(root, targ.Cell);
			return false;
		}


		// Token: 0x060022E3 RID: 8931 RVA: 0x000D4C5C File Offset: 0x000D2E5C
		private bool CanHitFromCellIgnoringRange(IntVec3 sourceCell, LocalTargetInfo targ, out IntVec3 goodDest)
		{
			if (targ.Thing != null)
			{
				if (targ.Thing.Map != this.Caster.Map)
				{
					goodDest = IntVec3.Invalid;
					return false;
				}
				ShootLeanUtility.CalcShootableCellsOf(Verb_ShootCompMounted.tempDestList, targ.Thing);
				for (int i = 0; i < Verb_ShootCompMounted.tempDestList.Count; i++)
				{
					if (this.CanHitCellFromCellIgnoringRange(sourceCell, Verb_ShootCompMounted.tempDestList[i], targ.Thing.def.Fillage == FillCategory.Full))
					{
						goodDest = Verb_ShootCompMounted.tempDestList[i];
						return true;
					}
				}
			}
			else if (this.CanHitCellFromCellIgnoringRange(sourceCell, targ.Cell, false))
			{
				goodDest = targ.Cell;
				return true;
			}
			goodDest = IntVec3.Invalid;
			return false;
		}

		// Token: 0x060022E4 RID: 8932 RVA: 0x000D4D2C File Offset: 0x000D2F2C
		private bool CanHitCellFromCellIgnoringRange(IntVec3 sourceSq, IntVec3 targetLoc, bool includeCorners = false)
		{
			if (this.verbProps.mustCastOnOpenGround && (!targetLoc.Standable(this.Caster.Map) || this.Caster.Map.thingGrid.CellContains(targetLoc, ThingCategory.Pawn)))
			{
				return false;
			}
			if (this.verbProps.requireLineOfSight)
			{
				if (!includeCorners)
				{
					if (!GenSight.LineOfSight(sourceSq, targetLoc, this.Caster.Map, true, null, 0, 0))
					{
						return false;
					}
				}
				else if (!GenSight.LineOfSightToEdges(sourceSq, targetLoc, this.Caster.Map, true, null))
				{
					return false;
				}
			}
			return true;
		}
		private void ThrowDebugText(string text)
		{
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(this.Caster.DrawPos, this.Caster.Map, text, -1f);
			}
		}

		private void ThrowDebugText(string text, IntVec3 c)
		{
			if (DebugViewSettings.drawShooting)
			{
				MoteMaker.ThrowText(c.ToVector3Shifted(), this.Caster.Map, text, -1f);
			}
		}

		// Token: 0x04001569 RID: 5481
		private Texture2D commandIconCached;

		// Token: 0x0400156A RID: 5482
		private static List<IntVec3> tempLeanShootSources = new List<IntVec3>();

		// Token: 0x0400156B RID: 5483
		private static List<IntVec3> tempDestList = new List<IntVec3>();
	}
}
