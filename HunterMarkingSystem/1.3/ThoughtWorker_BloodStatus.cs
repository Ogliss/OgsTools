using HunterMarkingSystem.ExtensionMethods;
using RimWorld;
using Verse;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem
{
    public class ThoughtWorker_BloodStatus : ThoughtWorker
    {
        int stageIndex;
        public override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            if (p == null || other == null)
            {
                return false;
            }
            if (p.Dead || other.Dead)
            {
                return false;
            }
            if (!other.RaceProps.Humanlike) return false;
            BloodStatusMode pBloodStatus = BloodStatus(p);
            BloodStatusMode otherBloodStatus = BloodStatus(other);
            switch (pBloodStatus)
            {
                case BloodStatusMode.Unblooded:
                    switch (otherBloodStatus)
                    {
                        case BloodStatusMode.Unblooded:
                            stageIndex = 0;
                            break;
                        case BloodStatusMode.Unmarked:
                            stageIndex = 1;
                            break;
                        case BloodStatusMode.Marked:
                            stageIndex = 2;
                            break;
                        default:
                            stageIndex = 9;
                            break;
                    }
                    break;
                case BloodStatusMode.Unmarked:
                    switch (otherBloodStatus)
                    {
                        case BloodStatusMode.Unblooded:
                            stageIndex = 3;
                            break;
                        case BloodStatusMode.Unmarked:
                            stageIndex = 4;
                            break;
                        case BloodStatusMode.Marked:
                            stageIndex = 5;
                            break;
                        default:
                            stageIndex = 9;
                            break;
                    }
                    break;
                case BloodStatusMode.Marked:
                    switch (otherBloodStatus)
                    {
                        case BloodStatusMode.Unblooded:
                            stageIndex = 6;
                            break;
                        case BloodStatusMode.Unmarked:
                            stageIndex = 7;
                            break;
                        case BloodStatusMode.Marked:
                            stageIndex = 8;
                            break;
                        default:
                            stageIndex = 9;
                            break;
                    }
                    break;
                default:
                    return false;
            }
            return ThoughtState.ActiveAtStage(stageIndex);
        }
        private BloodStatusMode BloodStatus(Pawn p)
        {
            bool Pblooded = p.Markable(out Comp_Markable pMarkable);
            if (Pblooded && pMarkable != null)
            {
                return pMarkable.BloodStatus;
            }
            return BloodStatusMode.NoComp;
        }

    }

}
