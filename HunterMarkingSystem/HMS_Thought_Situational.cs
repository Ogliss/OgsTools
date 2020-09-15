using HunterMarkingSystem;
using HunterMarkingSystem.ExtensionMethods;
using System;
using Verse;
namespace RimWorld
{
    // Token: 0x0200054E RID: 1358
    public class HMS_Thought_Situational : Thought_Situational
    {
        // Token: 0x170003A1 RID: 929
        // (get) Token: 0x0600197E RID: 6526 RVA: 0x0004E9ED File Offset: 0x0004CDED
        public override int CurStageIndex
        {
            get
            {
                int ind = 0;
                if (Markable != null)
                {
                    switch (Markable.BloodStatus)
                    {
                        case HMSUtility.BloodStatusMode.Unmarked:
                            ind = 1;
                            break;
                        case HMSUtility.BloodStatusMode.Marked:
                            ind = 2;
                            break;
                        default:
                            if (Markable.Inducted||Markable.MarkerRace)
                            {
                                ind = 0;
                            }
                            else
                            {
                                ind = -1;
                            }

                            break;
                    }
                //    Log.Message(string.Format("{0}, {1}, {2} = {3}", this, pawn.LabelShortCap, Markable.BloodStatus, ind));
                }
                else
                {

                //    Log.Message(string.Format("{0}, {1}, {2} = {3}", this, pawn.LabelShortCap, Markable.BloodStatus, ind));
                }
                return ind;
            }
        }

        public Comp_Markable Markable => pawn.TryGetComp<Comp_Markable>();
        // Token: 0x170003A2 RID: 930
        // (get) Token: 0x0600197F RID: 6527 RVA: 0x0004E9F5 File Offset: 0x0004CDF5

        public override string LabelCap
        {
            get
            {
                string labelstring = !this.reason.NullOrEmpty() ? base.CurStage.label : base.LabelCap;
                if (pawn != null)
                {
                    if (Markable != null)
                    {
                        if (Markable.markDataKill != null)
                        {
                            if (Markable.markDataKill.kindDef != null)
                            {
                                labelstring = labelstring.CapitalizeFirst() + " (" + Markable.markDataKill.kindDef.LabelCap + ")";
                            }
                        }
                    }
                }
                if (!this.reason.NullOrEmpty())
                {
                    return string.Format(labelstring, this.reason).CapitalizeFirst();
                }
                if (labelstring.NullOrEmpty())
                {
                    labelstring = this.reason;
                }
                return labelstring;
            }
        }

        public override float MoodOffset()
        {
            return base.MoodOffset();
        }
        
        /*
        public new string Description
        {
            get
            {
                string description = this.CurStage.description;
                if (pawn != null)
                { // if (selected) Log.Message("found corpse");
                  //    Log.Message("Description found pawn");
                    if (_Yautja != null)
                    {
                        //    Log.Message("Description found Comp_Yautja");
                        if (!_Yautja.MarkHedifflabel.NullOrEmpty())
                        {
                            description = desc1.CapitalizeFirst() + " (" + _Yautja.MarkHedifflabel.CapitalizeFirst() + ") "+ desc2;

                        }
                    }
                }
                if (description != null)
                {
                    return description;
                }
                return this.def.description;
            }
        }
        */
        protected override float BaseMoodOffset
        {
            get
            {
                if (pawn.isBloodable())
                {
                    switch (Markable.BloodStatus)
                    {
                        case HMSUtility.BloodStatusMode.Unblooded:
                            return base.BaseMoodOffset;
                        case HMSUtility.BloodStatusMode.Marked:
                            return Markable.MarkScore / 4;
                        default:
                            return 0f;
                    }
                }
                return 0f;
            }
        }
        // Token: 0x04000F33 RID: 3891
    }
}
