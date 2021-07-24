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
    
    [HarmonyPatch(typeof(ThingDefGenerator_Neurotrainer), "ImpliedThingDefs")]
    public static class AE_ThingDefGenerator_Neurotrainer_ImpliedThingDefs_Filter_Patch
    {
        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> list)
        {
            foreach (ThingDef item in list)
            {
                CompProperties_Neurotrainer compProperties = item.GetCompProperties<CompProperties_Neurotrainer>();
                if (compProperties.ability != null)
                {
                    if (compProperties.ability.GetType() != typeof(AbilitesExtended.EquipmentAbilityDef))
                    {
                        yield return item;
                    }
                }
                else yield return item;
            }
            yield break;
        }
    }


}
