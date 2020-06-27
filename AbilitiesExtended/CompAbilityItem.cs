using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AbilitesExtended
{
    public class CompProperties_AbilityItem : CompProperties
    {
        public List<AbilityDef> Abilities = new List<AbilityDef>();

        public CompProperties_AbilityItem()
        {
            compClass = typeof(CompAbilityItem);
        }
    }
    public class CompAbilityItem : ThingComp
    {
        public CompProperties_AbilityItem Props => (CompProperties_AbilityItem)this.props;

        public Verb PrimaryVerb
        {
            get
            {
                return this.verbTracker.PrimaryVerb;
            }
        }
        private Pawn Holder
        {
            get
            {
                return this.PrimaryVerb.CasterPawn;
            }
        }
        public List<Verb> AllVerbs
        {
            get
            {
                return this.verbTracker.AllVerbs;
            }
        }
        public VerbTracker VerbTracker
        {
            get
            {
                return this.verbTracker;
            }
        }

        public List<VerbProperties> VerbProperties
        {
            get
            {
                List<VerbProperties> VerbProperties = new List<VerbProperties>();
                foreach (var item in this.Props.Abilities)
                {
                    VerbProperties.Add(item.verbProperties);
                }
                return VerbProperties;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            if (this.Holder != null && this.Holder.equipment != null && this.Holder.equipment.Primary == this.parent)
            {
            //    this.Holder.equipment.Notify_PrimaryDestroyed();
            }
        }

        // Token: 0x06001754 RID: 5972 RVA: 0x0008577A File Offset: 0x0008397A
        public override void PostExposeData()
        {
            base.PostExposeData();
            /*
            Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[]
            {
                this
            });
            */
        }

        // Token: 0x06001755 RID: 5973 RVA: 0x0008579C File Offset: 0x0008399C
        public override void CompTick()
        {
            base.CompTick();
        }

        // Token: 0x06001756 RID: 5974 RVA: 0x000857B0 File Offset: 0x000839B0
        public void Notify_EquipmentLost()
        {
            List<Verb> allVerbs = this.AllVerbs;
            for (int i = 0; i < allVerbs.Count; i++)
            {
                allVerbs[i].Notify_EquipmentLost();
            }
        //    abilityTracker.
        }

        public VerbTracker verbTracker;
        public Pawn_AbilityTracker abilityTracker;
    }

    [StaticConstructorOnStartup]
    public class AbilityButtons
    {
        public static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        public static readonly Texture2D FullTex = SolidColorMaterials.NewSolidColorTexture(0.5f, 0.5f, 0.5f, 0.6f);
    }

    public static class StringsToTranslate
    {
        //
        public static readonly string AU_AoEProperties = "Area of Effect Properties";

        public static readonly string AU_TargetClass = "Targets: ";
        public static readonly string AU_AoECharacters = "Characters";
        public static readonly string AU_AoEFriendlyFire = "Friendly Fire: ";
        public static readonly string AU_AoEMaxTargets = "Max Targets: ";
        public static readonly string AU_AoEStartsFromCaster = "Starts from caster: ";
        public static readonly string AU_Cooldown = "Cooldown: ";
        public static readonly string AU_Type = "Type: ";
        public static readonly string AU_TargetAoE = "Area of Effect";
        public static readonly string AU_TargetSelf = "Targets Self";
        public static readonly string AU_TargetThing = "Targets Other";
        public static readonly string AU_TargetLocation = "Targets Location";
        public static readonly string AU_Extra = "Extra";
        public static readonly string AU_MentalStateChance = "Mental State Chance";
        public static readonly string AU_EffectChance = "Effect Chance";
        public static readonly string AU_BurstShotCount = "Burst Count:";
        public static readonly string AU_CastSuccess = "Cast Success";
        public static readonly string AU_CastFailure = "Cast Failed";
        public static readonly string AU_DISABLED = "DISABLED";
    }
}