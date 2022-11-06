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

namespace CompApparelVerbGiver.HarmonyInstance
{

    [HarmonyPatch(typeof(Verb), "get_EquipmentSource")]
    public static class Verb_EquipmentSource_CompApparelVerbGiver_Patch
    {
        public static void Postfix(Verb __instance, ref ThingWithComps __result)
        {
            if (__instance.EquipmentCompSource == null)
            {
                if (__instance.CasterIsPawn)
                {
                    if (__instance.CasterPawn.apparel != null)
                    {
                        CompApparelVerbGiver apparelVerbGiver = __instance.DirectOwner as CompApparelVerbGiver;
                        if (apparelVerbGiver != null)
                        {
                            __result = apparelVerbGiver.parent;
                        }
                    }
                }
            }
        }
    }

}