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
    [HarmonyPatch(typeof(Def), "get_LabelCap")]
    public static class Def_get_LabelCap_HiveFactionPhase_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Def __instance, ref TaggedString __result)
        {
            if (__instance.HasModExtension<HiveFactionExtension>())
            {
                HiveFactionExtension ext = __instance.GetModExtension<HiveFactionExtension>();
                if (ext.HasStages && ext.showStageInName)
                {
                    __result += ext.stageKey.Translate(ext.ActiveStage);
                }
            }
        }
    }
}