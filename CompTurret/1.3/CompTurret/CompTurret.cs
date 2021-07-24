using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CompTurret
{
	public class CompProperties_Turret : CompProperties
	{
		public CompProperties_Turret()
		{
			this.compClass = typeof(CompTurretGun);
		}

		public NamedArgument ChargeNounArgument
		{
			get
			{
				return this.chargeNoun.Named("CHARGENOUN");
			}
		}


		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string text in base.ConfigErrors(parentDef))
			{
				yield return text;
			}
			IEnumerator<string> enumerator = null;
			if (this.ammoDef != null && this.ammoCountToRefill == 0 && this.ammoCountPerCharge == 0)
			{
				yield return "TurretGun component has ammoDef but one of ammoCountToRefill or ammoCountPerCharge must be set";
			}
			if (this.ammoCountToRefill != 0 && this.ammoCountPerCharge != 0)
			{
				yield return "TurretGun component: specify only one of ammoCountToRefill and ammoCountPerCharge";
			}
			yield break;
			yield break;
		}

		// Token: 0x0600564E RID: 22094 RVA: 0x001CE077 File Offset: 0x001CC277
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			foreach (StatDrawEntry statDrawEntry in base.SpecialDisplayStats(req))
			{
				yield return statDrawEntry;
			}
			IEnumerator<StatDrawEntry> enumerator = null;
			if (!req.HasThing)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadMaxCharges_Name".Translate(this.ChargeNounArgument), this.maxCharges.ToString(), "Stat_Thing_ReloadMaxCharges_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
			}
			if (this.ammoDef != null)
			{
				if (this.ammoCountToRefill != 0)
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadRefill_Name".Translate(this.ChargeNounArgument), string.Format("{0} {1}", this.ammoCountToRefill, this.ammoDef.label), "Stat_Thing_ReloadRefill_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
				}
				else
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadPerCharge_Name".Translate(this.ChargeNounArgument), string.Format("{0} {1}", this.ammoCountPerCharge, this.ammoDef.label), "Stat_Thing_ReloadPerCharge_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
				}
			}
			if (this.destroyOnEmpty)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadDestroyOnEmpty_Name".Translate(this.ChargeNounArgument), "Yes".Translate(), "Stat_Thing_ReloadDestroyOnEmpty_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
			}
			yield break;
			yield break;
		}

		public float projectileOffset = 0f;
		public int maxCharges = 1;

		public ThingDef ammoDef;

		public int ammoCountToRefill;

		public float ammoCountPerCharge = 1f;

		public bool destroyOnEmpty;

		public bool drawTurret = true;
		public bool allowForcedTarget = true;
		public bool allowHoldFire = true;

		public int baseReloadTicks = 60;

		public bool displayGizmoWhileUndrafted = true;

		public bool displayGizmoWhileDrafted = true;

		public KeyBindingDef hotKey;

		public SoundDef soundReload;
		public SoundDef soundEmptyWarning;
		public string messageEmptyWarning;
		public SoundDef soundHalfRemaningWarning;
		public string messageHalfRemaningWarning;
		public SoundDef soundQuaterRemaningWarning;
		public string messageQuaterRemaningWarning;

		public string targetingLaserTexPath = string.Empty;
		public Color targetingLaserColor = Color.white;

		public bool OnByDefault = true;
		public bool DisableInMelee = true;
		public ThingDef TurretDef = null;
		public string iconPath;
		public string iconPathToggled;
		public int gizmoID = 0;

		public float barrellength = 0.75f;

		public bool showStunMote = true;
		public bool AffectedByEMP = true;
		public bool stunFromEMP = true;

		public Vector3 offsetNorth = new Vector3();
		public Vector3 offsetSouth = new Vector3();
		public Vector3 offsetEast = new Vector3();
		public Vector3 offsetWest = new Vector3();

		[MustTranslate]
		public string chargeNoun = "charge";
	}
	// Token: 0x02000CD3 RID: 3283
	public abstract class CompTurret : ThingComp, IAttackTargetSearcher, ITargetingSource
	{
		public CompProperties_Turret Props => this.props as CompProperties_Turret;
		public bool UseAmmo => Props.ammoDef != null;
		public bool HasAmmo => (UseAmmo && RemainingCharges > 0) || !UseAmmo;
		public ThingDef AmmoDef => Props.ammoDef;
		public int MaxCharges => Props.maxCharges;
		public float ChargesPerUnit => 1 / Props.ammoCountPerCharge;

		public Mote moteStun;

		public bool showStunMote => Props.showStunMote;
		public bool AffectedByEMP => Props.AffectedByEMP;
		public bool stunFromEMP;

		// Token: 0x04002E25 RID: 11813
		public int EMPAdaptedTicksLeft;

		// Token: 0x04002E26 RID: 11814
		public Effecter empEffecter;

		// Token: 0x04002E27 RID: 11815
		public string LabelRemaining
		{
			get
			{
				return string.Format("{0} / {1}", this.RemainingCharges, this.MaxCharges);
			}
		}

		public int RemainingCharges
		{
			get
			{
				return this.remainingCharges;
			}
		}


		// Token: 0x17000F31 RID: 3889
		// (get) Token: 0x06005655 RID: 22101 RVA: 0x001CE0BD File Offset: 0x001CC2BD
		public virtual bool CanBeUsed
		{
			get
			{
				return this.remainingCharges > 0;
			}
		}
		// Token: 0x17000F2F RID: 3887
		// (get) Token: 0x06005653 RID: 22099 RVA: 0x001CE0A3 File Offset: 0x001CC2A3

		public abstract LocalTargetInfo CurrentTarget { get; }

		public abstract Verb AttackVerb { get; }

		public LocalTargetInfo TargetCurrentlyAimingAt
		{
			get
			{
				return this.CurrentTarget;
			}
		}
		public Apparel Apparel => this.parent as Apparel;
		public Building Building => this.parent as Building;
		public Pawn Pawn => this.parent as Pawn;
		public Thing Operator
		{
			get
			{
                if (Building !=null) return Building;
				if (Pawn != null) return Pawn;
				if (Apparel != null)
				{
                    if (Apparel.Wearer != null) return Apparel.Wearer;
				}
				return null;
			}
		}
		public Pawn OperatorPawn
		{
			get
			{
				return Operator as Pawn;
			}
		}
		public bool IsOperated => OperatorPawn != null || Building != null;
		Thing IAttackTargetSearcher.Thing
		{
			get
			{
				return Operator;
			}
		}

		Verb IAttackTargetSearcher.CurrentEffectiveVerb
		{
			get
			{
				return this.AttackVerb;
			}
		}

		public LocalTargetInfo LastAttackedTarget
		{
			get
			{
				return this.lastAttackedTarget;
			}
		}

		public int LastAttackTargetTick
		{
			get
			{
				return this.lastAttackTargetTick;
			}
		}

		public int stunTicksLeft
		{
			get
			{
				return this.stunedticks;
			}
			set
			{
				this.stunedticks = value;
			}
		}
		public bool Stunned
		{
			get
			{
				return this.stunTicksLeft > 0;
			}
		}

		public float TargetPriorityFactor
		{
			get
			{
				return 1f;
			}
		}

		public bool CasterIsPawn => this.IsOperated && this.OperatorPawn != null;

		public bool IsMeleeAttack => false;

		public bool Targetable => true;

		public bool MultiSelect => false;

		public Thing Caster => this.Operator;

		public Pawn CasterPawn => this.OperatorPawn;

		public Verb GetVerb => this.AttackVerb;

		public Texture2D UIIcon => this.AttackVerb.UIIcon;

		public TargetingParameters targetParams => this.AttackVerb.targetParams;

		public ITargetingSource DestinationSelector => null;

        public bool HidePawnTooltips => throw new NotImplementedException();

        public override void CompTick()
		{
			base.CompTick();
			if (Operator == null)
			{
				return;
			}
			if (this.forcedTarget.HasThing && (!this.forcedTarget.Thing.Spawned || !Operator.Spawned || this.forcedTarget.Thing.Map != Operator.Map))
			{
				this.forcedTarget = LocalTargetInfo.Invalid;
			}
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			foreach (var item in base.CompFloatMenuOptions(selPawn))
			{
				yield return item;
			}
		}

		protected void OnAttackedTarget(LocalTargetInfo target)
		{
			this.lastAttackTargetTick = Find.TickManager.TicksGame;
			this.lastAttackedTarget = target;
		}

		// Token: 0x06005666 RID: 22118 RVA: 0x001CE3D0 File Offset: 0x001CC5D0
		public string DisabledReason(int minNeeded, int maxNeeded)
		{
			string result;
			if (this.AmmoDef == null)
			{
				result = "CommandReload_NoCharges".Translate(this.Props.ChargeNounArgument);
			}
			else
			{
				string arg;
				if (this.Props.ammoCountToRefill != 0)
				{
					arg = this.Props.ammoCountToRefill.ToString();
				}
				else
				{
					arg = ((minNeeded == maxNeeded) ? minNeeded.ToString() : string.Format("{0}-{1}", minNeeded, maxNeeded));
				}
				result = "CommandReload_NoAmmo".Translate(this.Props.ChargeNounArgument, this.AmmoDef.Named("AMMO"), arg.Named("COUNT"));
			}
			return result;
		}

		// Token: 0x06005667 RID: 22119 RVA: 0x001CE480 File Offset: 0x001CC680
		public bool NeedsReload(bool allowForcedReload)
		{
			if (this.AmmoDef == null)
			{
				return false;
			}
			if (this.Props.ammoCountToRefill == 0)
			{
				return this.RemainingCharges != this.MaxCharges;
			}
			if (!allowForcedReload)
			{
				return this.remainingCharges == 0;
			}
			return this.RemainingCharges != this.MaxCharges;
		}

		// Token: 0x06005668 RID: 22120 RVA: 0x001CE4D4 File Offset: 0x001CC6D4
		public void ReloadFrom(Thing ammo)
		{
			if (!this.NeedsReload(true))
			{
				return;
			}
			if (this.Props.ammoCountToRefill != 0)
			{
				if (ammo.stackCount < this.Props.ammoCountToRefill)
				{
					return;
				}
				ammo.SplitOff(this.Props.ammoCountToRefill).Destroy(DestroyMode.Vanish);
				this.remainingCharges = this.MaxCharges;
			}
			else
			{
				if (ammo.stackCount < this.Props.ammoCountPerCharge && this.Props.ammoCountPerCharge > 1f)
				{
					return;
				}
				int num = (int)(Mathf.Clamp(MaxAmmoNeeded(true), 1, ammo.stackCount));
				ammo.SplitOff(num).Destroy(DestroyMode.Vanish);
				this.remainingCharges += (int)(num * ChargesPerUnit);
			}
			if (this.Props.soundReload != null)
			{
				this.Props.soundReload.PlayOneShot(new TargetInfo(this.Operator.Position, this.Operator.Map, false));
			}
		}

		// Token: 0x06005669 RID: 22121 RVA: 0x001CE5D0 File Offset: 0x001CC7D0
		public int MinAmmoNeeded(bool allowForcedReload)
		{
			if (!this.NeedsReload(allowForcedReload))
			{
				return 0;
			}
			if (this.Props.ammoCountToRefill != 0)
			{
				return this.Props.ammoCountToRefill;
			}
			if (this.Props.ammoCountPerCharge<1f)
			{
				return 1;
			}
			return (int)this.Props.ammoCountPerCharge;
		}

		// Token: 0x0600566A RID: 22122 RVA: 0x001CE601 File Offset: 0x001CC801
		public int MaxAmmoNeeded(bool allowForcedReload)
		{
			if (!this.NeedsReload(allowForcedReload))
			{
				return 0;
			}
			if (this.Props.ammoCountToRefill != 0)
			{
				return this.Props.ammoCountToRefill;
			}
			return (int)(this.Props.ammoCountPerCharge * (this.MaxCharges - this.RemainingCharges));
		}

		// Token: 0x0600566B RID: 22123 RVA: 0x001CE640 File Offset: 0x001CC840
		public int MaxAmmoAmount()
		{
			if (this.AmmoDef == null)
			{
				return 0;
			}
			if (this.Props.ammoCountToRefill == 0)
			{
				return (int)(this.Props.ammoCountPerCharge * this.MaxCharges);
			}
			return this.Props.ammoCountToRefill;
		}

		// Token: 0x0600566C RID: 22124 RVA: 0x001CE677 File Offset: 0x001CC877
		public void UsedOnce()
		{
			if (this.remainingCharges > 0)
			{
				this.remainingCharges--;
			}
			if (this.Props.destroyOnEmpty && this.remainingCharges == 0)
			{
				this.parent.Destroy(DestroyMode.Vanish);
			}
		}

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

			if (Building != null || Pawn != null)
			{
				if (dinfo.Def == DamageDefOf.EMP)
				{
					if (this.AffectedByEMP)
					{
						this.stunTicksLeft += Mathf.RoundToInt(dinfo.Amount * 30f);
						this.stunFromEMP = true;

					}
				}
			}
		}

        // Token: 0x04003005 RID: 12293
        public int remainingCharges;

		// Token: 0x04003006 RID: 12294
		private VerbTracker verbTracker;
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_TargetInfo.Look(ref this.forcedTarget, "forcedTarget" + Props.gizmoID);
			Scribe_TargetInfo.Look(ref this.lastAttackedTarget, "lastAttackedTarget" + Props.gizmoID);
			Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer" + Props.gizmoID, new object[]
			{
				this
			});
			Scribe_Values.Look<int>(ref this.lastAttackTargetTick, "lastAttackTargetTick" + Props.gizmoID, 0, false);
			Scribe_Values.Look<int>(ref this.stunedticks, "stunedticks" + Props.gizmoID, 0, false);
			Scribe_Values.Look<int>(ref this.remainingCharges, "remainingCharges" + Props.gizmoID, -999, false);
			Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker" + Props.gizmoID, new object[]
			{
				this
			});
			if (Scribe.mode == LoadSaveMode.PostLoadInit && this.remainingCharges == -999)
			{
				this.remainingCharges = this.MaxCharges;
			}
		}

		public virtual bool CanHitTarget(LocalTargetInfo target)
		{
			return this.AttackVerb.CanHitTarget(target);
		}

		public void DrawHighlight(LocalTargetInfo target)
		{
			this.AttackVerb.DrawHighlight(target);
		}

		public virtual void OrderForceTarget(LocalTargetInfo target)
		{
			this.AttackVerb.OrderForceTarget(target);
		}

		public void OnGUI(LocalTargetInfo target)
		{
			this.AttackVerb.OnGUI(target);
		}

        public bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
		{
			return this.AttackVerb.ValidateTarget(target);
		}

        public ThingOwner innerContainer;
		protected LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;
		private LocalTargetInfo lastAttackedTarget;
		private int lastAttackTargetTick;
		private int stunedticks;
		private const float SightRadiusTurret = 13.4f;
	}
}
