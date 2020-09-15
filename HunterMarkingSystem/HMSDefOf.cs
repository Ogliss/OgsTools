using RimWorld;
using System;
using Verse;

namespace HunterMarkingSystem
{
	// Token: 0x02000956 RID: 2390
	[DefOf]
	public static class HMSDefOf
    {
		// Token: 0x06003781 RID: 14209 RVA: 0x001A8393 File Offset: 0x001A6793
		static HMSDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(HMSDefOf));
		}

        // HunterMarkingSystem HefiffDefs
        public static HediffDef HMS_Hediff_Unblooded;
        public static HediffDef HMS_Hediff_BloodedUM;
        public static HediffDef HMS_Hediff_BloodedM;
        public static HediffDef HMS_Hediff_BloodedMHuman;
        public static HediffDef HMS_Hediff_BloodedMBeast;
        public static HediffDef HMS_Hediff_BloodedMWorthyHuman;
        public static HediffDef HMS_Hediff_BloodedMHumanlike;
        public static HediffDef HMS_Hediff_BloodedMWorthyHumanlike;
        public static HediffDef HMS_Hediff_BloodedMMechanoid;
        public static HediffDef HMS_Hediff_BloodedMXenomorph;
        public static HediffDef HMS_Hediff_BloodedMXenomorphThrumbo;
        public static HediffDef HMS_Hediff_BloodedMXenomorphQueen;
        public static HediffDef HMS_Hediff_BloodedMPredalien;
        public static HediffDef HMS_Hediff_BloodedMBadBlood;
        public static HediffDef HMS_Hediff_BloodedMHound;
        public static HediffDef HMS_Hediff_BloodedMThrumbo;
        public static HediffDef HMS_Hediff_BloodedMCrusher;
        public static HediffDef HMS_Hediff_BloodedMGroTye;

        // HunterMarkingSystem ThoughtDefs
        public static ThoughtDef HMS_Thought_BloodedM;

        // HunterMarkingSystem ThoughtDefs Thought_SituationalSocial
        public static ThoughtDef HMS_UnbloodedVs_ThoughtDef;
        public static ThoughtDef HMS_UnmarkedVs_ThoughtDef;
        public static ThoughtDef HMS_MarkedVs_ThoughtDef;

        // HunterMarkingSystem ThoughtDefs Memories
        public static ThoughtDef HMS_Thought_ThrillOfTheHunt;

        // HunterMarkingSystem JobDefs 
        public static JobDef HMS_Job_MarkSelf;
        public static JobDef HMS_Job_MarkOther;
        public static JobDef HMS_Job_TakeTrophy;

        // HunterMarkingSystem JobDefs 
        public static ConceptDef HMS_Concept_Unblooded;
        public static ConceptDef HMS_Concept_Blooding;
    }
}
