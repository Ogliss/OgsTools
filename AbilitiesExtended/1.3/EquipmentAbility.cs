using AbilitesExtended;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AbilitesExtended
{
    // Token: 0x02000AAD RID: 2733
    public class EquipmentAbility : Ability
    {
        EquipmentAbilityDef AbilityDef => (EquipmentAbilityDef)this.def;
        // Token: 0x06003FD1 RID: 16337 RVA: 0x0015268C File Offset: 0x0015088C
        public EquipmentAbility(Pawn pawn) : base(pawn)
        {
        }

        // Token: 0x06003FD2 RID: 16338 RVA: 0x00152695 File Offset: 0x00150895
        public EquipmentAbility(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public EquipmentAbility(Pawn pawn, AbilityDef def, Thing source) : base(pawn, def)
        {
            sourceEquipment = source as ThingWithComps;
        }

        public ThingWithComps sourceEquipment;
        public CompAbilityItem AbilityItem => sourceEquipment?.TryGetCompFast<CompAbilityItem>();

        public int MaxCastingTicks => (int)(AbilityDef.cooldown * GenTicks.TicksPerRealSecond);
        private int TicksUntilCasting = -1;
        public int CooldownTicksLeft
        {
            get => TicksUntilCasting;
            set => TicksUntilCasting = value;
        } //Log.Message(value.ToString()); } }

        public override void ExposeData()
        {
            Scribe_Defs.Look<AbilityDef>(ref this.def, "def");
            if (this.def == null)
            {
                return;
            }
            Scribe_Values.Look<int>(ref this.Id, "Id", -1, false);
            if (Scribe.mode == LoadSaveMode.LoadingVars && this.Id == -1)
            {
                this.Id = Find.UniqueIDsManager.GetNextAbilityID();
            }
            Scribe_References.Look(ref this.sourceEquipment, "sourceEquipment");
            Scribe_References.Look<Precept>(ref this.sourcePrecept, "sourcePrecept", false);
            Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[]
            {
                this
            });
            Scribe_Values.Look<int>(ref this.cooldownTicks, "cooldownTicks", 0, false);
            Scribe_Values.Look<int>(ref this.cooldownTicksDuration, "cooldownTicksDuration", 0, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.Initialize();
            }
            Scribe_Values.Look(ref TicksUntilCasting, "EquipmentAbilityTicksUntilcasting", -1);
        }
        // Token: 0x06003FD3 RID: 16339 RVA: 0x0015269F File Offset: 0x0015089F

        public override IEnumerable<Command> GetGizmos()
        {
            if (this.gizmo == null)
            {
                var command_CastPower = new Command_EquipmentAbility(this)
                {
                    defaultLabel = AbilityDef.LabelCap,

                };

                command_CastPower.curTicks = CooldownTicksLeft;

                //GetDesc
                var s = new StringBuilder();
                s.AppendLine(AbilityDef.GetDescription());
                command_CastPower.defaultDesc = s.ToString();
                s = null;
                command_CastPower.icon = this.def.uiIcon;
                /*
                command_CastPower.action = delegate (Thing target)
                {
                    var tInfo = GenUI.TargetsAt(UI.MouseMapPosition(), Verb.verbProps.targetParams, false)?.First() ??
                                target;
                    TryCastAbility(AbilityContext.Player, tInfo);
                };
                */
                if (!CanCastPowerCheck("Player", out string reason))
                    command_CastPower.Disable(reason);
                this.gizmo = command_CastPower;
            }
            yield return this.gizmo;
        }

        public virtual bool CanCastPowerCheck(string context, out string reason)
        {
            reason = "";

            if (context == "Player" && this.pawn.Faction != Faction.OfPlayer)
            {
                reason = "CannotOrderNonControlled".Translate();
                return false;
            }
            if (this.pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent) &&
                AbilityDef.verbProperties.isPrimary)
            {
                reason = "IsIncapableOfViolence".Translate(this.pawn.Name.ToStringShort);
                return false;
            }
            if (CooldownTicksLeft > 0)
            {
                reason = "AU_PawnAbilityRecharging".Translate(this.pawn.Name.ToStringShort);
                return false;
            }
            //else if (!Verb.CasterPawn.drafter.Drafted)
            //{
            //    reason = "IsNotDrafted".Translate(new object[]
            //    {
            //        Verb.CasterPawn.Name.ToStringShort
            //    });
            //}

            return true;
        }


        // Token: 0x06003FD7 RID: 16343 RVA: 0x001529C8 File Offset: 0x00150BC8
        public override void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination)
        {
            base.QueueCastingJob(target, destination);
            CooldownTicksLeft = MaxCastingTicks;
        }

        // Token: 0x06003FD8 RID: 16344 RVA: 0x00152A28 File Offset: 0x00150C28
        public override void AbilityTick()
        {
            base.AbilityTick();
            if (sourceEquipment != null)
            {
                if (sourceEquipment is Apparel apparel)
                {
                    if (apparel.Wearer != pawn)
                    {
                        pawn.abilities.TryRemoveEquipmentAbility(AbilityDef, sourceEquipment);
                    }
                }
            }
            else
            {
                Log.Warning($"{this} lost source equipment, removing ability");
                pawn.abilities.TryRemoveEquipmentAbility(AbilityDef, sourceEquipment);
            }
            if (CooldownTicksLeft > -1 && !Find.TickManager.Paused)
            {
                CooldownTicksLeft--;
                if (AbilityItem!=null)
                {
                //    abilityItem;
                }
                if (!this.gizmo.disabled)
                {
                    this.gizmo.Disable("On Cooldown");
                }
            }
            else
            {
                if (!Find.TickManager.Paused)
                {
                    if (this.gizmo != null)
                    {
                        if (this.gizmo.disabled)
                        {
                            this.gizmo.disabled = false;
                        }
                    }
                }
            }
        }

    }
}
