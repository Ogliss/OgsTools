using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AbilitesExtended
{

    // AbilitesExtended.Verb_UseEquipment
    public class Verb_UseEquipment : Verb_CastAbility
    {
        public VerbProperties_EquipmentAbility verbProperties
        {
            get
            {
                return (VerbProperties_EquipmentAbility)this.verbProps;
            }
        }

        public new CompAbilityItem EquipmentCompSource
        {
            get
            {
                if (((EquipmentAbility)this.ability).sourceEquipment!=null)
                {
                    return ((EquipmentAbility)this.ability).sourceEquipment.TryGetComp<CompAbilityItem>();
                }
                return null;
            }
        }

        public new ThingWithComps EquipmentSource
        {
            get
            {
                EquipmentAbility equipmentAbility = this.ability as EquipmentAbility;
                if (equipmentAbility == null)
                {
                    return null;
                }
             //   Log.Message(this.EquipmentCompSource.parent.LabelCap);
                return equipmentAbility.sourceEquipment;
            }
        }

        // Token: 0x06002188 RID: 8584 RVA: 0x0000FFF1 File Offset: 0x0000E1F1
        public override bool ValidateTarget(LocalTargetInfo target)
        {
            
            return base.ValidateTarget(target);
        }

        private void ThrowDebugText(string text)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, text, -1f);
            }
        }

        // Token: 0x06002197 RID: 8599 RVA: 0x000CBF7D File Offset: 0x000CA17D
        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), this.caster.Map, text, -1f);
            }
        }

        // Token: 0x06002189 RID: 8585 RVA: 0x000CB488 File Offset: 0x000C9688
        public override void DrawHighlight(LocalTargetInfo target)
        {
            AbilityDef def = this.ability.def;
            this.DrawRadius();
            if (this.CanHitTarget(target) && this.IsApplicableTo(target, false))
            {
                if (def.HasAreaOfEffect)
                {
                    if (target.IsValid)
                    {
                        GenDraw.DrawTargetHighlight(target);
                        GenDraw.DrawRadiusRing(target.Cell, def.EffectRadius, Verb_CastAbility.RadiusHighlightColor, null);
                    }
                }
                else
                {
                    GenDraw.DrawTargetHighlight(target);
                }
            }
            if (target.IsValid)
            {
                this.ability.DrawEffectPreviews(target);
            }
            this.verbProps.DrawRadiusRing(this.caster.Position);
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                bool flag;
                float num = this.HighlightFieldRadiusAroundTarget(out flag);
                ShootLine shootLine;
                if (num > 0.2f && this.TryFindShootLineFromTo(this.caster.Position, target, out shootLine))
                {
                    if (flag)
                    {
                        GenExplosion.RenderPredictedAreaOfEffect(shootLine.Dest, num);
                        return;
                    }
                    GenDraw.DrawFieldEdges((from x in GenRadial.RadialCellsAround(shootLine.Dest, num, true)
                                            where x.InBounds(Find.CurrentMap)
                                            select x).ToList<IntVec3>());
                }
            }
        }

    }
}
