using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace HunterMarkingSystem
{
    // Token: 0x02000D7E RID: 3454
    public class MarkHediff : HediffWithComps
    {
        public Comp_Markable markable => this.pawn.TryGetComp<Comp_Markable>() ?? null;
        public MarkData markData => markable.markDataKill ?? markable.markDataKillNew ?? null;
        public override string LabelBase
        {
            get
            {
                if (markable!=null)
                {
                }
                return this.def.label;
            }
        }
    //    public override string SeverityLabel => markData.raceDef.LabelCap +" "+ base.SeverityLabel;

    }
}
