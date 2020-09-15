using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CompTurret
{
	[StaticConstructorOnStartup]
	public class CompTurretGun : CompTurret
	{
		public bool Active
		{
			get
			{
				return (this.Wearer != null) && active;
			}
		}

		public CompEquippable GunCompEq
		{
			get
			{
				if (this.gun==null)
				{
					this.MakeGun();
				}
				return this.gun.TryGetComp<CompEquippable>();
			}
		}

		public override LocalTargetInfo CurrentTarget
		{
			get
			{
				return this.currentTargetInt;
			}
		}

		private bool WarmingUp
		{
			get
			{
				return this.burstWarmupTicksLeft > 0;
			}
		}
		public Vector3 TurretPos
		{
			get
			{
				return this.top?.DrawPos ?? this.Wearer.DrawPos;
			}
		}
		public override Verb AttackVerb
		{
			get
			{
				return this.GunCompEq.PrimaryVerb;
			}
		}

		private bool PlayerControlled
		{
			get
			{
				return (Wearer.Faction == Faction.OfPlayer || this.MannedByColonist);
			}
		}

		private bool CanSetForcedTarget
		{
			get
			{
				return this.PlayerControlled;
			}
		}

		private bool CanToggleHoldFire
		{
			get
			{
				return this.PlayerControlled;
			}
		}

		private bool IsMortar
		{
			get
			{
				return this.Props.TurretDef.building.IsMortar;
			}
		}

		private bool IsMortarOrProjectileFliesOverhead
		{
			get
			{
				return this.AttackVerb.ProjectileFliesOverhead() || this.IsMortar;
			}
		}

		private bool CanExtractShell
		{
			get
			{
				if (!this.PlayerControlled)
				{
					return false;
				}
				CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
				return compChangeableProjectile != null && compChangeableProjectile.Loaded;
			}
		}

		private bool MannedByColonist
		{
			get
			{
				return this.Wearer != null && this.Wearer.Faction == Faction.OfPlayer;
			}
		}

		private bool MannedByNonColonist
		{
			get
			{
				return this.Wearer != null  && Wearer.Faction != Faction.OfPlayer;
			}
		}

		public CompTurretGun()
		{
			this.top = new CompTurretTop(this);
		}
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				if (this.top !=null && Wearer != null)
				{
					this.top.SetRotationFromOrientation();
				}

				this.burstCooldownTicksLeft = Props.TurretDef.building.turretInitialCooldownTime.SecondsToTicks();
			}
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			this.MakeGun();
		}
		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			this.ResetCurrentTarget();
			Effecter effecter = this.progressBarEffecter;
			if (effecter == null)
			{
				return;
			}
			effecter.Cleanup();
		}

		public override void OrderForceTarget(LocalTargetInfo targ)
		{
			Log.Message("OrderForceTarget 0");
			if (!targ.IsValid)
			{
				if (this.forcedTarget.IsValid)
				{
					this.ResetForcedTarget();
				}
				return;
			}
			if ((targ.Cell - Wearer.Position).LengthHorizontal < this.AttackVerb.verbProps.EffectiveMinRange(targ, Wearer))
			{
				Messages.Message("MessageTargetBelowMinimumRange".Translate(), apparel, MessageTypeDefOf.RejectInput, false);
				return;
			}
			if ((targ.Cell - Wearer.Position).LengthHorizontal > this.AttackVerb.verbProps.range)
			{
				Messages.Message("MessageTargetBeyondMaximumRange".Translate(), apparel, MessageTypeDefOf.RejectInput, false);
				return;
			}
			if (this.forcedTarget != targ)
			{
				this.forcedTarget = targ;
				this.currentTargetInt = targ;
				if (this.burstCooldownTicksLeft <= 0)
				{
					this.TryStartShootSomething(false);
				}
			}
			if (this.holdFire)
			{
				Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(this.Props.TurretDef.label), this.apparel, MessageTypeDefOf.RejectInput, false);
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (Wearer == null)
			{
				this.AttackVerb.caster = parent;
				return;
			}
			else
			{
				if (this.AttackVerb.caster == null)
				{
					this.AttackVerb.caster = Wearer;
				}
			}
			if (this.Stunned)
			{
				this.stunTicksLeft--;
				if (this.top != null && Wearer != null)
				{
					this.top.TurretTopTick();
				}
				return;
			}
			if (Wearer.Downed || !Wearer.Awake())
			{
				return;
			}
			if (this.CanExtractShell && this.MannedByColonist)
			{
				CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
				if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
				{
					this.ExtractShell();
				}
			}

			if (this.forcedTarget.IsValid && !this.CanSetForcedTarget)
			{
				this.ResetForcedTarget();
			}
			if (this.forcedTarget.IsValid && !HasAmmo)
			{
				this.ResetForcedTarget();
			}
			if (!this.CanToggleHoldFire)
			{
				this.holdFire = false;
			}
			if (this.forcedTarget.ThingDestroyed)
			{
				this.ResetForcedTarget();
			}
		//	Log.Message("tick Active: " + this.Active + ", Worn: " + (this.Wearer != null) + ", Wearer Spawned: " + Wearer.Spawned);
			if (this.Active && (this.Wearer != null) && Wearer.Spawned)
			{
				this.GunCompEq.verbTracker.VerbsTick();
				bool stunflag = !this.Stunned;
				if (stunflag && this.AttackVerb.state != VerbState.Bursting)
				{
					if (this.WarmingUp)
					{
						this.burstWarmupTicksLeft--;
						if (this.burstWarmupTicksLeft == 0)
						{
						//	Log.Message("WarmingUp complete, BeginBurst");
							this.BeginBurst();
						}
					}
					else
					{
					//	Log.Message("tick 2 1 b1: "+ this.burstCooldownTicksLeft);
						if (this.burstCooldownTicksLeft > 0)
						{
							this.burstCooldownTicksLeft--;
							if (this.IsMortar)
							{
								if (this.progressBarEffecter == null)
								{
									this.progressBarEffecter = EffecterDefOf.ProgressBar.Spawn();
								}
								this.progressBarEffecter.EffectTick(Wearer, TargetInfo.Invalid);
								MoteProgressBar mote = ((SubEffecter_ProgressBar)this.progressBarEffecter.children[0]).mote;
								mote.progress = 1f - (float)Math.Max(this.burstCooldownTicksLeft, 0) / (float)this.BurstCooldownTime().SecondsToTicks();
								mote.offsetZ = -0.8f;
							}
						}
					//	if (Wearer.IsHashIntervalTick(10)) Log.Message("tick 2 1 b2 interval: "+ Wearer.IsHashIntervalTick(10));
						if (this.burstCooldownTicksLeft <= 0 && Wearer.IsHashIntervalTick(10) && HasAmmo)
						{
							this.TryStartShootSomething(true);
						}
					}
					if (this.top != null && Wearer != null)
					{
						this.top.TurretTopTick();
					}
					return;
				}
			}
			else
			{
				this.ResetCurrentTarget();
			}
		}

		protected void TryStartShootSomething(bool canBeginBurstImmediately)
		{
			if (this.progressBarEffecter != null)
			{
				this.progressBarEffecter.Cleanup();
				this.progressBarEffecter = null;
			}
			if (!Wearer.Spawned || (this.holdFire && this.CanToggleHoldFire) || (this.AttackVerb.ProjectileFliesOverhead() && Wearer.Map.roofGrid.Roofed(Wearer.Position)) || !this.AttackVerb.Available())
			{
				this.ResetCurrentTarget();
				return;
			}
			bool isValid = this.currentTargetInt.IsValid;
			if (this.forcedTarget.IsValid)
			{
				this.currentTargetInt = this.forcedTarget;
			}
			else
			{
				this.currentTargetInt = this.TryFindNewTarget();
			}
			if (!isValid && this.currentTargetInt.IsValid)
			{
				SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
			}
			if (!this.currentTargetInt.IsValid)
			{
				this.ResetCurrentTarget();
				return;
			}
			if (this.Props.TurretDef.building.turretBurstWarmupTime > 0f)
			{
				this.burstWarmupTicksLeft = this.Props.TurretDef.building.turretBurstWarmupTime.SecondsToTicks();
				return;
			}
			if (canBeginBurstImmediately)
			{
				this.BeginBurst();
				return;
			}
			this.burstWarmupTicksLeft = 1;
		}

		protected LocalTargetInfo TryFindNewTarget()
		{
			IAttackTargetSearcher attackTargetSearcher = this.TargSearcher();
			Faction faction = attackTargetSearcher.Thing.Faction;
			float range = this.AttackVerb.verbProps.range;
			Building t;
			if (Rand.Value < 0.5f && this.AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && Wearer.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
			{
				float num = this.AttackVerb.verbProps.EffectiveMinRange(x, Wearer);
				float num2 = (float)x.Position.DistanceToSquared(Wearer.Position);
				return num2 > num * num && num2 < range * range;
			}).TryRandomElement(out t))
			{
				return t;
			}
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
			if (!this.AttackVerb.ProjectileFliesOverhead())
			{
				targetScanFlags |= TargetScanFlags.NeedLOSToAll;
				targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
			}
			if (this.AttackVerb.IsIncendiary())
			{
				targetScanFlags |= TargetScanFlags.NeedNonBurning;
			}
			if (this.IsMortar)
			{
				targetScanFlags |= TargetScanFlags.NeedNotUnderThickRoof;
			}
			Thing tgt = (Thing)BestShootTargetFromCurrentPosition(attackTargetSearcher, AttackVerb, targetScanFlags, new Predicate<Thing>(this.IsValidTarget), 0f, 9999f);
			if (tgt == null && Wearer.TargetCurrentlyAimingAt!=null)
			{
				tgt = Wearer.TargetCurrentlyAimingAt.Thing as Thing;
			}
			return tgt;
		}
		public static IAttackTarget BestShootTargetFromCurrentPosition(IAttackTargetSearcher searcher, Verb currentEffectiveVerb, TargetScanFlags flags, Predicate<Thing> validator = null, float minDistance = 0f, float maxDistance = 9999f)
		{
			if (currentEffectiveVerb == null)
			{
				Log.Error("BestShootTargetFromCurrentPosition with " + searcher.ToStringSafe<IAttackTargetSearcher>() + " who has no attack verb.", false);
				return null;
			}
			return AttackTargetFinder.BestAttackTarget(searcher, flags, validator, Mathf.Max(minDistance, currentEffectiveVerb.verbProps.minRange), Mathf.Min(maxDistance, currentEffectiveVerb.verbProps.range), default(IntVec3), float.MaxValue, false, false);
		}

		private IAttackTargetSearcher TargSearcher()
		{
			if (this.Wearer != null)
			{
				return this.Wearer;
			}
			return this;
		}

		private bool IsValidTarget(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				if (this.AttackVerb.ProjectileFliesOverhead())
				{
					RoofDef roofDef = Wearer.Map.roofGrid.RoofAt(t.Position);
					if (roofDef != null && roofDef.isThickRoof)
					{
						return false;
					}
				}
				if (this.Wearer != null)
				{
					return !GenAI.MachinesLike(Wearer.Faction, pawn);
				}
				if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
				{
					return false;
				}
			}
		//	Log.Message(t + " IsValidTarget");
			return true;
		}

		protected void BeginBurst()
		{
		//	Log.Message(AttackVerb+" BeginBurst " + CurrentTarget);
			this.AttackVerb.TryStartCastOn(this.CurrentTarget, false, true);
			base.OnAttackedTarget(this.CurrentTarget);
		}

		protected void BurstComplete()
		{
			this.burstCooldownTicksLeft = this.BurstCooldownTime().SecondsToTicks();
		}

		protected float BurstCooldownTime()
		{
			if (this.Props.TurretDef.building.turretBurstCooldownTime >= 0f)
			{
				return this.Props.TurretDef.building.turretBurstCooldownTime;
			}
			return this.AttackVerb.verbProps.defaultCooldownTime;
		}


		public override string CompInspectStringExtra()
		{
            if (this.UseAmmo)
			{
				return "TSF_AmmunitionRemaining".Translate(this.Props.ChargeNounArgument) + ": " + this.LabelRemaining;
			}
			return base.CompInspectStringExtra();
		}

		/*
		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string inspectString = base.CompInspectStringExtra();
			if (!inspectString.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString);
			}
			if (this.AttackVerb.verbProps.minRange > 0f)
			{
				stringBuilder.AppendLine("MinimumRange".Translate() + ": " + this.AttackVerb.verbProps.minRange.ToString("F0"));
			}
			if (Wearer.Spawned && this.IsMortarOrProjectileFliesOverhead && Wearer.Position.Roofed(Wearer.Map))
			{
				stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
			}
			else if (Wearer.Spawned && this.burstCooldownTicksLeft > 0 && this.BurstCooldownTime() > 5f)
			{
				stringBuilder.AppendLine("CanFireIn".Translate() + ": " + this.burstCooldownTicksLeft.ToStringSecondsFromTicks());
			}
			CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile != null)
			{
				if (compChangeableProjectile.Loaded)
				{
					stringBuilder.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell));
				}
				else
				{
					stringBuilder.AppendLine("ShellNotLoaded".Translate());
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
		*/
		public override void PostDraw()
		{
			if (Wearer != null)
			{
                if (this.Props.drawTurret)
				{
					if (this.top == null)
					{
						top = new CompTurretTop(this);
					}
					if (this.top != null)
					{
						this.top.DrawTurret();
					}
				}
			}
			base.PostDraw();
		}

		public static readonly Material LineMatRed = MaterialPool.MatFrom("Other/TSFTargetingLaser", ShaderDatabase.Transparent, Color.red);
		public override void PostDrawExtraSelectionOverlays()
		{
			if (!this.isWorn)
			{
				return;
			}
			/*
			float range = this.AttackVerb.verbProps.range;
			if (range < 90f)
			{
				GenDraw.DrawRadiusRing(Wearer.Position, range);
			}
			float num = this.AttackVerb.verbProps.EffectiveMinRange(true);
			if (num < 90f && num > 0.1f)
			{
				GenDraw.DrawRadiusRing(Wearer.Position, num);
			}
			*/
			if (this.WarmingUp)
			{
				int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
				DrawAimPie(Wearer, this.CurrentTarget, degreesWide, (float)Props.TurretDef.size.x * 0.5f);
			}
			if (this.burstCooldownTicksLeft>0)
			{
				GenDraw.DrawCooldownCircle(TurretPos, Mathf.Min(0.5f, (float)this.burstCooldownTicksLeft * 0.002f));
			}
			if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned))
			{
				Vector3 vector;
				if (this.forcedTarget.HasThing)
				{
					vector = this.forcedTarget.Thing.TrueCenter();
				}
				else
				{
					vector = this.forcedTarget.Cell.ToVector3Shifted();
				}
				Vector3 a = TurretPos;
				vector.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				a.y = vector.y;
				GenDraw.DrawLineBetween(a, vector, CompTurretGun.ForcedTargetLineMat);
			}
		}

		// Token: 0x06002133 RID: 8499 RVA: 0x000CB158 File Offset: 0x000C9358
		public void DrawAimPie(Thing shooter, LocalTargetInfo target, int degreesWide, float offsetDist)
		{
			float facing = 0f;
			if (target.Cell != shooter.Position)
			{
				if (target.Thing != null)
				{
					facing = (target.Thing.DrawPos - TurretPos).AngleFlat();
				}
				else
				{
					facing = (target.Cell.ToVector3() - TurretPos).AngleFlat();
				}
			}
			GenDraw.DrawAimPieRaw(TurretPos + new Vector3(0f, offsetDist, 0f), facing, degreesWide);
		}

		public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
			{
				yield return gizmo;
			}
			IEnumerator<Gizmo> enumerator = null;
			/*
			if (this.CanExtractShell)
			{
				CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
				yield return new Command_Action
				{
					defaultLabel = "CommandExtractShell".Translate(),
					defaultDesc = "CommandExtractShellDesc".Translate(),
					icon = compChangeableProjectile.LoadedShell.uiIcon,
					iconAngle = compChangeableProjectile.LoadedShell.uiIconAngle,
					iconOffset = compChangeableProjectile.LoadedShell.uiIconOffset,
					iconDrawScale = GenUI.IconDrawScale(compChangeableProjectile.LoadedShell),
					action = delegate ()
					{
						this.ExtractShell();
					}
				};
			}
			CompChangeableProjectile compChangeableProjectile2 = this.gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile2 != null)
			{
				StorageSettings storeSettings = compChangeableProjectile2.GetStoreSettings();
				foreach (Gizmo gizmo2 in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
				{
					yield return gizmo2;
				}
				enumerator = null;
			}
			*/
			int num = 600000000;
			bool flag = Find.Selector.SelectedObjects.Contains(Wearer);
			if (flag && Wearer.IsColonist)
			{
				Texture2D CommandTex;
				CommandTex = ContentFinder<Texture2D>.Get(Props.iconPath, true);
				yield return new Command_ToggleCompTurret(this)
				{

					icon = CommandTex,
					defaultLabel = Props.TurretDef.building.turretGunDef.LabelCap + (active ? " turret: on." : " turret: off."),
					defaultDesc = "Switch mode.",
					isActive = (() => active),
					toggleAction = delegate ()
					{
						active = !active;
					},
					activateSound = SoundDef.Named("Click"),
					groupKey = num + Props.gizmoID,
					/*
                    disabled = GetWearer.stances.curStance.StanceBusy,
                    disabledReason = "Busy"
                    */
				};
			}

			if (this.CanSetForcedTarget && active)
			{
				num+=100;
				Command_CompTurretVerbTarget command_VerbTarget = new Command_CompTurretVerbTarget();
				command_VerbTarget.defaultLabel = Props.TurretDef.building.turretGunDef.LabelCap + " " +"CommandSetForceAttackTarget".Translate();
				command_VerbTarget.defaultDesc = "CommandSetForceAttackTargetDesc".Translate();
				command_VerbTarget.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
				command_VerbTarget.verb = this.AttackVerb;
				command_VerbTarget.gunTurret = this;
				command_VerbTarget.hotKey = Props.hotKey;
				command_VerbTarget.drawRadius = true;
				command_VerbTarget.groupKey = num + Props.gizmoID;
				if (Wearer.Spawned && this.IsMortarOrProjectileFliesOverhead && Wearer.Position.Roofed(Wearer.Map))
				{
					command_VerbTarget.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
				}
				if (Wearer.Spawned && Stunned)
				{
					command_VerbTarget.Disable("CannotFire".Translate() + ": " + "EMPDisabled".Translate().CapitalizeFirst());
				}
				/*
				command_VerbTarget.action = delegate (LocalTargetInfo target)
				{
					this.OrderForceTarget(target);
				};
				*/
				yield return command_VerbTarget;
			}
			if (this.forcedTarget.IsValid)
			{
				num += 100;
				Command_Action command_Action = new Command_Action();
				command_Action.defaultLabel = Props.TurretDef.building.turretGunDef.LabelCap + " " + "CommandStopForceAttack".Translate();
				command_Action.defaultDesc = "CommandStopForceAttackDesc".Translate();
				command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
				command_Action.groupKey = num + Props.gizmoID;
				command_Action.action = delegate ()
				{
					this.ResetForcedTarget();
					SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
				};
				if (!this.forcedTarget.IsValid)
				{
					command_Action.Disable("CommandStopAttackFailNotForceAttacking".Translate());
				}
				command_Action.hotKey = KeyBindingDefOf.Misc5;
				yield return command_Action;
			}
			if (this.CanToggleHoldFire)
			{
				num += 100;
				yield return new Command_Toggle
				{
					defaultLabel = "CommandHoldFire".Translate(),
					defaultDesc = "CommandHoldFireDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
					hotKey = KeyBindingDefOf.Misc6,
					groupKey = num + Props.gizmoID,
					toggleAction = delegate ()
					{
						this.holdFire = !this.holdFire;
						if (this.holdFire)
						{
							this.ResetForcedTarget();
						}
					},
					isActive = (() => this.holdFire)
				};
			}
			if (this.UseAmmo)
			{
				bool drafted = this.Wearer.Drafted;
				if ((drafted && !this.Props.displayGizmoWhileDrafted) || (!drafted && !this.Props.displayGizmoWhileUndrafted))
				{
					yield break;
				}
				/*
				ThingWithComps gear = this.parent;
				foreach (Verb verb in this.GunCompEq.VerbTracker.AllVerbs)
				{
					if (verb.verbProps.hasStandardCommand)
					{
						yield return this.CreateVerbTargetCommand(gear, verb);
					}
				}
				*/
				if (Prefs.DevMode)
				{
					yield return new Command_Action
					{
						defaultLabel = Props.TurretDef.building.turretGunDef.LabelCap + " " + "Debug: Reload to full",
						
						action = delegate ()
						{
							this.remainingCharges = this.MaxCharges;
						}
						
					};
				}
			}
		//	yield break;
		}

		private void ExtractShell()
		{
			GenPlace.TryPlaceThing(this.gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), Wearer.Position, Wearer.Map, ThingPlaceMode.Near, null, null, default(Rot4));
		}

		private void ResetForcedTarget()
		{
			this.forcedTarget = LocalTargetInfo.Invalid;
			this.burstWarmupTicksLeft = 0;
			if (this.burstCooldownTicksLeft <= 0)
			{
				this.TryStartShootSomething(false);
			}
		}

		private void ResetCurrentTarget()
		{
			this.currentTargetInt = LocalTargetInfo.Invalid;
			this.burstWarmupTicksLeft = 0;
		}

		public void MakeGun()
		{
			this.gun = ThingMaker.MakeThing(this.Props.TurretDef.building.turretGunDef, null);
			UpdateGunVerbs();
		}

		private void UpdateGunVerbs()
		{
			if (this.gun==null)
			{
				this.MakeGun();
				if (this.gun == null)
				{
					return;
				}
			}
			List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
			if (allVerbs.NullOrEmpty())
			{
				return;
			}
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = (Thing)this.apparel.Wearer ?? this.apparel;
				verb.castCompleteCallback = new Action(this.BurstComplete);
				Verb_ShootCompMounted verb_ = verb as Verb_ShootCompMounted;
				if (verb_!=null)
				{
					verb_.turret = this;
				}
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<bool>(ref this.active, "active" + Props.gizmoID, false, false);
			Scribe_Values.Look<int>(ref this.burstCooldownTicksLeft, "burstCooldownTicksLeft" + Props.gizmoID, 0, false);
			Scribe_Values.Look<int>(ref this.burstWarmupTicksLeft, "burstWarmupTicksLeft" + Props.gizmoID, 0, false);
			Scribe_TargetInfo.Look(ref this.currentTargetInt, "currentTarget" + Props.gizmoID);
			Scribe_Values.Look<bool>(ref this.holdFire, "holdFire" + Props.gizmoID, false, false);
			Scribe_Deep.Look<Thing>(ref this.gun, "gun" + Props.gizmoID, Array.Empty<object>());
			BackCompatibility.PostExposeData(this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.UpdateGunVerbs();
			}
		}
		private bool active;
		protected int burstCooldownTicksLeft;
		protected int burstWarmupTicksLeft;
		protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;
		private bool holdFire;
		public Thing gun;
		protected CompTurretTop top;
		protected Effecter progressBarEffecter;
		private const int TryStartShootSomethingIntervalTicks = 10;
		public static Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));
	}
}
