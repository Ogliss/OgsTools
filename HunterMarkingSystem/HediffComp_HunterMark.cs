using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem
{
    public class HediffCompProperties_HunterMark : HediffCompProperties
    {
        // Token: 0x06004C0D RID: 19469 RVA: 0x00237094 File Offset: 0x00235494
        public HediffCompProperties_HunterMark()
        {
            this.compClass = typeof(HediffComp_HunterMark);
        }
        public MarkDrawerType markDrawerType;
        public string markGraphicPath;
        public Shader shader = ShaderDatabase.Cutout;
        public Vector2 drawSize = Vector2.one;
        public Color color = Color.white;
        public Color colorTwo = Color.white;
    }

    [StaticConstructorOnStartup]
    public class HediffComp_HunterMark : HediffComp
    {
        // Token: 0x17000BE6 RID: 3046
        // (get) Token: 0x06004C0F RID: 19471 RVA: 0x002370CE File Offset: 0x002354CE
        public HediffCompProperties_HunterMark Props
		{
			get
			{
				return (HediffCompProperties_HunterMark)this.props;
			}
        }
        public Comp_Markable Markable => Pawn.TryGetComp<Comp_Markable>();
        public MarkData markData
        {
            get
            {
                if (Def.defName.Contains(HunterMarkingSystem.Markedkey))
                {
                    return Markable.markDataKill;
                }
                else
                {
                    return Markable.markDataKillNew;
                }
            }
        }
        public BloodStatusMode BloodStatus => this.Markable != null ? this.Markable.BloodStatus : BloodStatusMode.NoComp;
        public Corpse corpse => this.Markable != null ? this.Markable.Markcorpse : null;
        public Pawn pawn => this.Markable != null ? this.Markable.Mark : null;
        public PawnKindDef pawnKindDef => Markable.markDataKill.kindDef ?? null;
        public string MarkHedifftype;
        public override bool CompShouldRemove => base.CompShouldRemove;
        /*
        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (pawnKindDef != null)
                {
                    return pawnKindDef.LabelCap;
                }
                return null;
            }
        }
        */
        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (markData != null)
                {
                    if (BloodStatus>=BloodStatusMode.Unmarked)
                    {
                        return markData.Humanlike ? markData.Name + " (" + markData.Label + ")" : markData.Label;
                    }
                }
                else
                {
                    return "Unblooded";
                }
                return null;
            }
        }

        public override string CompTipStringExtra => base.CompTipStringExtra;

        public override string CompDebugString()
        {
            return Markable.MarkScore.ToString();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
        }


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

        }

        public bool hasMat => !Props.markGraphicPath.NullOrEmpty();
        public Material ImplantMaterial(Pawn pawn, Rot4 bodyFacing)
        {
            string path;
            if (this.Props.markDrawerType != MarkDrawerType.Body)
            {
                path = Props.markGraphicPath;
            }
            else
            {
                path = Props.markGraphicPath + "_" + pawn.story.bodyType.ToString();
            }
            return GraphicDatabase.Get<Graphic_Multi>(path, Props.shader, Props.drawSize, Props.color, Props.colorTwo).MatAt(bodyFacing);
        }
        
    }



    public enum MarkDrawerType
    {
        Undefined,
        Body,
        Head
    }
}
