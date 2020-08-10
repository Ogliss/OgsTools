using RimWorld;
using CloakingDevice.HarmonyInstance;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Linq;

namespace CloakingDevice
{
    public class CompProperties_CloakGenerator : CompProperties_Wearable
    {
        public CompProperties_CloakGenerator()
        {
            this.compClass = typeof(CompCloakGenerator);
        }
        public bool isShield = false;

        public bool damageDrains = true;
        public bool damageDisrupts = false;

        public bool EMPDrains = true;
        public bool EMPDisrupts = true;

        public bool rainDrains = true;
        public bool rainDisrupts = false;

        public bool waterDrains = true;
        public bool waterDisrupts = false;

        public bool qualityAffectsEnergy = true;

        public float energyDrainPerSecond = 0.001f;
        public float energyDrainPerSecondMoving = 0.05f;

        public int minsDisruptionTicks = 60;

    }

    public class CompCloakGenerator : CompWearable
    {
        public CompProperties_CloakGenerator Props => (CompProperties_CloakGenerator)props;

        private float EnergyMax
        {
            get
            {
                return Apparel.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
            }
        }

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x06000051 RID: 81 RVA: 0x00003B20 File Offset: 0x00001D20
        private float EnergyGainPerTick
        {
            get
            {
                return Apparel.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60f;
            }
        }
        private float EnergyUsedPerTick
        {
            get
            {
                return (IsWorn ? (Wearer.pather.MovingNow ? Props.energyDrainPerSecondMoving : Props.energyDrainPerSecond) : 0f) / 60f;
            }
        }

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x06000052 RID: 82 RVA: 0x00003B34 File Offset: 0x00001D34
        public float Energy
        {
            get
            {
                return this.energy;
            }
        }
        public Gizmo gizmo = null;
        public Gizmo Gizmo
        {
            get
            {
                if (IsWorn)
                {
                    if (Wearer.apparel.WornApparel.Any(x => x.TryGetComp<CompCloakGenerator>() != null && x.TryGetComp<CompCloakGenerator>().gizmo != null && x != Apparel))
                    {
                        gizmo = null;
                        return null;
                    }
                }
                gizmo = new Gizmo_CompCloakGenerator_Status
                {
                    Pawn = Wearer,
                    compCloak = this
                };
                return gizmo;
            }
        }

        public bool Cloaked => this.cloakMode == CloakMode.On;
        public override IEnumerable<Gizmo> CompGetGizmosWorn()
        {
            if (Wearer.Faction != Faction.OfPlayer)
            {
                yield break;
            }
            bool CloakReseached = (YautjaDefOf.AvP_Tech_Yautja_CloakGenerator.IsFinished);
            if (CloakReseached)
            {
                if (Find.Selector.SingleSelectedObject == (Wearer) && Gizmo != null)
                {
                    yield return Gizmo;
                }
                int num = 700000102;
                yield return new Command_Toggle
                {

                    icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_CloakMode", true),
                    defaultLabel = Cloaked ? "Cloak: on." : "Cloak: off.",
                    defaultDesc = "Switch mode.",
                    isActive = (() => Cloaked),
                    toggleAction = delegate ()
                    {
                        this.SwitchCloakMode();
                    },
                    activateSound = SoundDef.Named("Click"),
                    groupKey = num + 1,
                    /*
                    disabled = GetWearer.stances.curStance.StanceBusy,
                    disabledReason = "Busy"
                    */
                };
                /*
                if (this.cloakMode == CloakMode.On)
                {
                    yield return new Command_Action
                    {
                        icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_CloakMode", true),
                        defaultLabel = "Cloak: off.",
                        defaultDesc = "Switch mode.",
                        activateSound = SoundDef.Named("Click"),
                        action = new Action(this.SwitchCloakMode),
                        groupKey = num + 1,
                    };
                }
                if (this.cloakMode == CloakMode.Off)
                {
                    yield return new Command_Action
                    {
                        icon = ContentFinder<Texture2D>.Get("Ui/Commands/CommandButton_CloakMode", true),
                        defaultLabel = "Cloak: on.",
                        defaultDesc = "Switch mode.",
                        activateSound = SoundDef.Named("Click"),
                        action = new Action(this.SwitchCloakMode),
                        groupKey = num + 1,
                    };
                }
                */
            }
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000023A4 File Offset: 0x000005A4
        public void SwitchCloakMode()
        {
            switch (this.cloakMode)
            {
                case CompCloakGenerator.CloakMode.Off:
                    this.cloakMode = CompCloakGenerator.CloakMode.On;
                    break;
                case CompCloakGenerator.CloakMode.On:
                    this.cloakMode = CompCloakGenerator.CloakMode.Off;
                    break;
            }
            this.RefreshCloakState();
        }
        // Token: 0x06000004 RID: 4 RVA: 0x000020EE File Offset: 0x000002EE
        public void RefreshCloakState()
        {
            if (this.ComputeCloakState())
            {
                this.SwitchOnCloak();
                return;
            }
            this.SwitchOffCloak();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.cloakIsOn || Find.TickManager.TicksGame >= this.nextUpdateTick)
            {
                this.nextUpdateTick = Find.TickManager.TicksGame + 60;
                this.RefreshCloakState();
            }
            if (base.Wearer == null)
            {
                this.energy = 0f;
                return;
            }
            else if (Wearer.Faction == Faction.OfPlayer)
            {
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.AvP_Concept_Gauntlet) && Wearer.IsColonist)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.AvP_Concept_Gauntlet, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.AvP_Concept_Wistblade) && Wearer.IsColonist)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.AvP_Concept_Wistblade, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.AvP_Concept_SelfDestruct) && (Wearer.IsColonist || Wearer.IsPrisoner))
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.AvP_Concept_SelfDestruct, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.AvP_Concept_MediComp) && Wearer.IsColonist && YautjaDefOf.AvP_Tech_Yautja_MediComp.IsFinished)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.AvP_Concept_MediComp, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.AvP_Concept_ShardInjector) && Wearer.IsColonist && YautjaDefOf.AvP_Tech_Yautja_HealthShard.IsFinished)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.AvP_Concept_ShardInjector, OpportunityType.GoodToKnow);
                }
                if (!PlayerKnowledgeDatabase.IsComplete(YautjaConceptDefOf.AvP_Concept_Cloak) && Wearer.IsColonist && YautjaDefOf.AvP_Tech_Yautja_CloakGenerator.IsFinished)
                {
                    LessonAutoActivator.TeachOpportunity(YautjaConceptDefOf.AvP_Concept_Cloak, OpportunityType.GoodToKnow);
                }
            }
            if (this.cloakState == CloakState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                    return;
                }
            }
            else if (this.cloakState == CloakState.Active & this.cloakMode == CloakMode.On)
            {
                this.energy -= this.EnergyUsedPerTick;
                if (this.energy <= 0)
                {
                    this.energy = 0;
                    this.cloakMode = CloakMode.Off;
                    this.SwitchOffCloak();
                }
            }
            else if (this.cloakMode == CloakMode.Off)
            {
                this.energy += this.EnergyGainPerTick;
                if (this.energy > this.EnergyMax)
                {
                    this.energy = this.EnergyMax;
                }
            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002108 File Offset: 0x00000308
        public bool ComputeCloakState()
        {
            return IsWorn && !Wearer.Dead && !Wearer.Downed && Wearer.Awake() && this.cloakMode == CompCloakGenerator.CloakMode.On;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000021F0 File Offset: 0x000003F0
        public void SwitchOnCloak()
        {
            IntVec3 intVec = base.Wearer.DrawPos.ToIntVec3();
            if (Apparel.DestroyedOrNull() || !IsWorn)
            {
                this.cloakIsOn = false;
            }
            this.cloakIsOn = true;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x0000227D File Offset: 0x0000047D
        public void SwitchOffCloak()
        {
            if (Wearer != null)
            {
                if (Wearer.health.hediffSet.HasHediff(YautjaDefOf.AvP_Hediff_Cloaked))
                {
                    /*
                    Hediff hediff = Wearer.health.hediffSet.GetFirstHediffOfDef(YautjaDefOf.AvP_Hediff_Cloaked);
                    Wearer.health.RemoveHediff(hediff);
                    */
                    this.cloakMode = CloakMode.Off;
                }
            }
            this.cloakIsOn = false;
        }

        // Token: 0x06000059 RID: 89 RVA: 0x00003CB0 File Offset: 0x00001EB0
        public bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            if (dinfo.Instigator == Wearer)
            {
                return true;
            }
            if (this.cloakState == CompCloakGenerator.CloakState.Active && (dinfo.Instigator != null || dinfo.Def.isExplosive))
            {
                if (dinfo.Instigator != null && dinfo.Instigator.Position.AdjacentTo8WayOrInside(Wearer.Position))
                {
                    this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
                }
                this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
                if (this.energy < 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }
                return true;
            }
            if (this.cloakState != CompCloakGenerator.CloakState.Active || dinfo.Instigator != null)
            {
                return false;
            }
            this.energy -= (float)dinfo.Amount * this.EnergyLossPerDamage;
            if (this.energy < 0f)
            {
                this.Break();
                return false;
            }
            this.AbsorbedDamage(dinfo);
            return true;
        }

        // Token: 0x0600005A RID: 90 RVA: 0x00003DB9 File Offset: 0x00001FB9
        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        // Token: 0x0600005B RID: 91 RVA: 0x00003DCC File Offset: 0x00001FCC
        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            UnityEngine.Vector3 loc = Wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float num = Mathf.Min(10f, 2f + (float)dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, Wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                Rand.PushState();
                MoteMaker.ThrowDustPuff(loc, Wearer.Map, Rand.Range(0.8f, 1.2f));
                Rand.PopState();
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        // Token: 0x0600005C RID: 92 RVA: 0x00003EBC File Offset: 0x000020BC
        private void Break()
        {
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
            MoteMaker.MakeStaticMote(Wearer.TrueCenter(), Wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
            for (int i = 0; i < 6; i++)
            {
                Rand.PushState();
                MoteMaker.ThrowDustPuff(Wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), Wearer.Map, Rand.Range(0.8f, 1.2f));
                Rand.PopState();
            }
            this.energy = 0f;
            this.ticksToReset = this.StartingTicksToReset;
            cloakMode = CloakMode.Off;
        }

        // Token: 0x0600005D RID: 93 RVA: 0x00003F90 File Offset: 0x00002190
        private void Reset()
        {
            if (Wearer.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Wearer.Position, Wearer.Map, false));
                MoteMaker.ThrowLightningGlow(Wearer.TrueCenter(), Wearer.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;
        }

        public CloakState cloakState
        {
            get
            {
                if ((this.ticksToReset > 0))// || (this.cloakMode == Cloakgen.CloakMode.ForcedOff))
                {
                    return CompCloakGenerator.CloakState.Resetting;
                }
                else if ((this.cloakMode == CompCloakGenerator.CloakMode.On))
                {
                    return CompCloakGenerator.CloakState.Active;
                }
                return CompCloakGenerator.CloakState.InActive;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }

        public int nextUpdateTick;
        // Token: 0x04000019 RID: 25
        private float energy;

        // Token: 0x0400001A RID: 26
        private int ticksToReset = -1;

        // Token: 0x0400001B RID: 27
        private int lastKeepDisplayTick = -9999;

        // Token: 0x0400001C RID: 28
        private Vector3 impactAngleVect;

        // Token: 0x0400001D RID: 29
        private int lastAbsorbDamageTick = -9999;

        // Token: 0x04000022 RID: 34
        private int StartingTicksToReset = 360;

        // Token: 0x04000023 RID: 35
        private float EnergyOnReset = 0.0f;

        // Token: 0x04000024 RID: 36
        private float EnergyLossPerDamage = 0.01f;


        // Token: 0x04000004 RID: 4
        public bool cloakIsOn;

        // Token: 0x04000005 RID: 5
        public CompCloakGenerator.CloakMode cloakMode;

        // Token: 0x02000004 RID: 4
        public enum CloakMode
        {
            // Token: 0x04000008 RID: 8
            Off,
            // Token: 0x04000009 RID: 9
            On
        }

        // Token: 0x04000005 RID: 5
        // public Cloakgen.CloakState cloakState;
        public enum CloakState : byte
        {
            InActive,
            Active,
            Resetting
        }
    }
}
