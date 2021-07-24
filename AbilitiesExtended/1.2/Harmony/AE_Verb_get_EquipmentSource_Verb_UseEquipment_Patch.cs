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
using UnityEngine;
using System.Reflection;

namespace AbilitesExtended.HarmonyInstance
{

    [HarmonyPatch(typeof(Verb), "get_EquipmentSource")]
    public static class AE_Verb_get_EquipmentSource_Verb_UseEquipment_Patch
    {
        [HarmonyPrefix, HarmonyPriority(200)]
        public static bool Prefix(ref Verb __instance, ref ThingWithComps __result)
        {
            if (__instance.GetType() == typeof(Verb_ShootEquipment))
            {
            //    Log.Message(__instance.DirectOwner.GetType().Name);
                Verb_ShootEquipment verb = (Verb_ShootEquipment)__instance;
                EquipmentAbility equipmentAbility = verb.ability as EquipmentAbility;
                if (equipmentAbility != null)
                {
                //    Log.Message(equipmentAbility.def.LabelCap);
                    if (equipmentAbility.sourceEquipment != null)
                    {
                    //    Log.Message(equipmentAbility.sourceEquipment.def.LabelCap);
                    }
                    __result = equipmentAbility.sourceEquipment;
                }
            }
            return true;
        }
    }

}
