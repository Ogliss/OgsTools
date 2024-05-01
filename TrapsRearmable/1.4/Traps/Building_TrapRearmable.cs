using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace TrapsRearmable
{
    // Token: 0x02000007 RID: 7
    public class Building_TrapRearmable : Building_Trap
    {
        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600001F RID: 31 RVA: 0x000027C3 File Offset: 0x000009C3
        public override bool Armed
        {
            get
            {
                return this.armedInt;
            }
        }

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000020 RID: 32 RVA: 0x000027CB File Offset: 0x000009CB
        public override Graphic Graphic
        {
            get
            {
                if (this.armedInt)
                {
                    return base.Graphic;
                }
                if (this.graphicUnarmedInt == null)
                {
                    this.graphicUnarmedInt = this.def.building.trapUnarmedGraphicData.GraphicColoredFor(this);
                }
                return this.graphicUnarmedInt;
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00002806 File Offset: 0x00000A06
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.armedInt, "armed", false, false);
            Scribe_Values.Look<bool>(ref this.autoRearm, "autoRearm", false, false);
        }

        // Token: 0x06000022 RID: 34 RVA: 0x00002834 File Offset: 0x00000A34
        protected override void SpringSub(Pawn p)
        {
            SoundStarter.PlayOneShot(SoundDefOf.TrapSpring, new TargetInfo(base.Position, base.Map, false));
            this.armedInt = false;
            if (p != null)
            {
                this.DamagePawn(p);
            }
            if (this.autoRearm && this.canBeDesignatedRearm())
            {
                base.Map.designationManager.AddDesignation(new Designation(this, TrapsDefOf.TR_RearmTrap));
            }
        }

        // Token: 0x06000023 RID: 35 RVA: 0x000028A4 File Offset: 0x00000AA4
        private bool canBeDesignatedRearm()
        {
            if (!this.armedInt)
            {
                return (from i in base.Map.designationManager.AllDesignationsOn(this)
                        where i.def == TrapsDefOf.TR_RearmTrap
                        select i).FirstOrDefault<Designation>() == null;
            }
            return false;
        }

        // Token: 0x06000024 RID: 36 RVA: 0x000028F8 File Offset: 0x00000AF8
        public void Rearm()
        {
            this.armedInt = true;
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00002904 File Offset: 0x00000B04
        private void DamagePawn(Pawn p)
        {
            Rand.PushState();
            float value = Rand.Value;
            Rand.PopState();
            float num = StatExtension.GetStatValue(this, StatDefOf.TrapMeleeDamage, true) * Building_TrapRearmable.TrapDamageFactor.RandomInRange / (float)Building_TrapRearmable.DamageCount.RandomInRange;
            float num2 = num * 0.015f;
            int num3 = 0;
            while ((float)num3 < (float)Building_TrapRearmable.DamageCount.RandomInRange)
            {
                DamageInfo damageInfo;
                damageInfo = new DamageInfo(DamageDefOf.Stab, num, num2, -1f, this, null, null, 0, null);
                DamageWorker.DamageResult damageResult = p.TakeDamage(damageInfo);
                if (num3 == 0)
                {
                    BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(p, RulePackDefOf.DamageEvent_TrapSpike, null);
                    Find.BattleLog.Add(battleLogEntry_DamageTaken);
                    damageResult.AssociateWithLog(battleLogEntry_DamageTaken);
                }
                num3++;
            }
        }

        // Token: 0x06000026 RID: 38 RVA: 0x000029B3 File Offset: 0x00000BB3
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return new Command_Toggle
            {
                defaultLabel = Translator.Translate("CommandAutoRearm"),
                defaultDesc = Translator.Translate("CommandAutoRearmDesc"),
                hotKey = KeyBindingDefOf.Misc3,
                icon = TexCommand.RearmTrap,
                isActive = (() => this.autoRearm),
                toggleAction = delegate ()
                {
                    this.autoRearm = !this.autoRearm;
                }
            };
            if (this.canBeDesignatedRearm())
            {
                yield return new Command_Action
                {
                    defaultLabel = Translator.Translate("AvP_CommandRearm"),
                    defaultDesc = Translator.Translate("AvP_CommandRearmDesc"),
                    hotKey = KeyBindingDefOf.Misc4,
                    icon = TexCommand.RearmTrap,
                    action = delegate ()
                    {
                        base.Map.designationManager.AddDesignation(new Designation(this, TrapsDefOf.TR_RearmTrap));
                    }
                };
            }
            yield break;
        }

        // Token: 0x0400000F RID: 15
        private bool autoRearm;

        // Token: 0x04000010 RID: 16
        private bool armedInt = true;

        // Token: 0x04000011 RID: 17
        private Graphic graphicUnarmedInt;

        // Token: 0x04000012 RID: 18
        private static readonly FloatRange TrapDamageFactor = new FloatRange(0.7f, 1.3f);

        // Token: 0x04000013 RID: 19
        private static readonly IntRange DamageCount = new IntRange(1, 2);
    }
}
