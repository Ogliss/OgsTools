using System;
using UnityEngine;
using Verse;
using RimWorld;
using HunterMarkingSystem.ExtensionMethods;

namespace HunterMarkingSystem
{
    // Token: 0x0200021E RID: 542
    public class ThoughtWorker_MarkedMood : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
        //    Log.Message(string.Format("{0}, {1}", this, p.LabelShortCap));
            Comp_Markable Markable = p.TryGetComp<Comp_Markable>();
            ThoughtState state = ThoughtState.Inactive;
            if ((Markable != null))
            {
                if ((Markable.Inducted))
                {
                    switch (Markable.BloodStatus)
                    {
                        case HMSUtility.BloodStatusMode.Unblooded:
                            if (Markable.MarkerRace)
                            {
                                state = ThoughtState.ActiveAtStage(0);
                            }
                            else
                            {
                                state = ThoughtState.Inactive;
                            }
                            break;
                        case HMSUtility.BloodStatusMode.Unmarked:
                            state = ThoughtState.ActiveAtStage(1);
                            break;
                        case HMSUtility.BloodStatusMode.Marked:
                            state = ThoughtState.ActiveAtStage(2);
                            break;
                        default:
                            //    Log.Message(string.Format("{0}, {1}, {2} = {3} Bad BloodStatusMode", this, p.LabelShortCap, Markable.BloodStatus, state));
                            state = ThoughtState.Inactive;
                            break;
                    }
                }
                else
                {
                //    Log.Message(string.Format("{0}, {1}, {2} = {3} not inducted", this, p.LabelShortCap, Markable.BloodStatus, state));
                }
            //    Log.Message(string.Format("{0}, {1}, {2} = Active: {3}, Ind: {4}, Reason: {5}", this, p.LabelShortCap, Markable.BloodStatus, state.Active, state.StageIndex, state.Reason));
            }
            else
            {
                
            //    Log.Message(string.Format("{0}, {1} = {2}", this, p.LabelShortCap, state));
            }
            return state;
        }

    }
}
