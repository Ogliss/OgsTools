using HunterMarkingSystem.ExtensionMethods;
using RimWorld;
using Verse;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem
{
    // Token: 0x02000207 RID: 519
    public class ThoughtWorker_Unblooded : ThoughtWorker
    {
        int stageIndex;
        // Token: 0x06000A02 RID: 2562 RVA: 0x0004F2B0 File Offset: 0x0004D6B0
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (p == null || other == null)
            {
                return false;
            }
            if (p.Dead || other.Dead)
            {
                return false;
            }
            bool Pblooded = p.Markable(out Comp_Markable pMarkable);
            bool Oblooded = other.Markable(out Comp_Markable oMarkable);
            
            if (pMarkable == null)
            {
                //    Log.Message(string.Format("{0} {1} Vs {2} pmarkable is null", this, p.LabelShortCap, other.LabelShortCap));
                return false;
            }
            if (!pMarkable.MarkerRace && pMarkable.UseMarkerRace)
            {
                //    Log.Message(string.Format("{0} {1} Vs {2}, {1} is not a MarkerRace", this, p.LabelShortCap, other.LabelShortCap));
                return false;
            }
            if (!p.RaceProps.Humanlike)
            {
                //    Log.Message(string.Format("{0} {1} Vs {2}, {1} is not a Humanlike", this, p.LabelShortCap, other.LabelShortCap));
                return false;
            }
            if (!other.RaceProps.Humanlike)
            {
                //    Log.Message(string.Format("{0} {1} Vs {2}, {2} is not a Humanlike", this, p.LabelShortCap, other.LabelShortCap));
                return false;
            }
            /*
            if (!RelationsUtility.PawnsKnowEachOther(p, other))
            {
                return ThoughtState.Inactive;
            }
            */

            if (Pblooded && pMarkable.BloodStatus > (BloodStatusMode.None))
            {
                switch (oMarkable.BloodStatus)
                {
                    case BloodStatusMode.Unblooded:
                        if (pMarkable.MarkerRace)
                        {
                            stageIndex = 0;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case BloodStatusMode.Unmarked:
                        stageIndex = 1;
                        break;
                    case BloodStatusMode.Marked:
                        stageIndex = 2;
                        break;
                    default:
                        //    Log.Message(string.Format("{0} {1} Vs {2}, Bad BloodStatusMode ActiveAtStage {3}", this, p.LabelShortCap, other.LabelShortCap, stageIndex));
                        return false;
                }
                if (pMarkable.BloodStatus == BloodStatusMode.Unblooded)
                {
                    //    Log.Message(string.Format("{0} {1} Vs {2}, is ActiveAtStage {3}", this, p.LabelShortCap, other.LabelShortCap, stageIndex));
                    return ThoughtState.ActiveAtStage(stageIndex);
                }
                else
                {
                    //    Log.Message(string.Format("{0} {1} Vs {2}, {1} is not Marked", this, p.LabelShortCap, other.LabelShortCap));
                }
            }
            else
            {
                //    Log.Message(string.Format("{0} {1} Vs {2}, {1} is not Blooded", this, p.LabelShortCap, other.LabelShortCap));
            }
            return false;
        }
    }

}
