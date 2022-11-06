using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using HunterMarkingSystem.Settings;
using HunterMarkingSystem.ExtensionMethods;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem
{
    [HarmonyPatch(typeof(ThingWithComps), "get_DescriptionFlavor")]
    public static class HMS_ThingWithComps_get_DescriptionFlavor_Patch
    {
        [HarmonyPostfix]
        public static void get_DescriptionFlavor_Postfix(ThingWithComps __instance, ref string __result)
        {
            if (__instance.def.race!=null)
            {
                __result = __result + "HMS_BloodStatus_MyScore".Translate(new MarkData((Pawn)__instance).MarkScore);
            }
        }
    }
}