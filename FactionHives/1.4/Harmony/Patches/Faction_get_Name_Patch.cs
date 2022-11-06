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

namespace ExtraHives.HarmonyInstance
{
//    [HarmonyPatch(typeof(Faction), "get_Name")]
    public static class Faction_get_Name_Patch
    { // LabelCap
        [HarmonyPostfix]
        public static void Postfix(Faction __instance, ref string __result)
        {
            if (__instance.def.HasModExtension<HiveFactionExtension>())
            {
                HiveFactionExtension ext = __instance.def.GetModExtension<HiveFactionExtension>();
                if (ext.HasStages)
                {
                //    Log.Message("Faction_get_Name_Patch " + __instance);
                    __result += " " + ext.ActiveStage;
                }
            }
        }
    }
}