using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using Verse.Sound;

namespace HunterMarkingSystem
{
    [HarmonyPatch(typeof(BackCompatibility), "BackCompatibleDefName")]
    public static class HMS_BackCompatibility_BackCompatibleDefName_Patch
    {
        [HarmonyPostfix]
        public static void BackCompatibleDefName_Postfix(Type defType, string defName, bool forDefInjections, ref string __result)
        {
            if (GenDefDatabase.GetDefSilentFail(defType, defName, false) == null)
            {
                //    Log.Message(string.Format("Checking for replacement for {0} Type: {1}", defName, defType));
                if (defType == typeof(ThingDef))
                {

                }
                if (defType == typeof(FactionDef))
                {

                }
                if (defType == typeof(PawnKindDef))
                {

                }
                if (defType == typeof(ResearchProjectDef))
                {

                }
                if (defType == typeof(HediffDef))
                {
                    if (defName.Contains("AvP_Hediff_Unblooded"))
                    {
                        __result = "HMS_Hediff_Unblooded";
                    }
                    if (defName.Contains("AvP_Hediff_BloodedUM"))
                    {
                        __result = "HMS_Hediff_BloodedUM";
                    }
                    if (defName == ("AvP_Hediff_BloodedM"))
                    {
                        __result = "HMS_Hediff_BloodedM";
                    }
                    else if (defName.Contains("AvP_Hediff_BloodedM"))
                    {
                        // Taking a string 
                        String str = defName;

                        String[] spearator = { "_" };
                        Int32 count = 3;

                        // using the method 
                        String[] strlist = str.Split(spearator, count,
                               StringSplitOptions.RemoveEmptyEntries);
                        __result = DefDatabase<HediffDef>.AllDefs.Where(x=> x.defName.Contains(strlist[2])).First().defName;
                    }
                }
            }
        }
    }

}
