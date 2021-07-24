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

// Settlement
namespace ExtraHives.HarmonyInstance
{
    [HarmonyPatch(typeof(Settlement), "get_Label")]
    public static class Settlement_Label_HiveFactionPhase_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Settlement __instance, ref string __result)
        {
            if (__instance.Faction.def.HasModExtension<HiveFactionExtension>())
            {
                HiveFactionExtension ext = __instance.Faction.def.GetModExtension<HiveFactionExtension>();
                if (ext.HasStages && ext.showStageInName)
                {
                    __result += " "+ext.stageKey.Translate(ext.ActiveStage);
                }
            }
        }
    }
}