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
using AdvancedGraphics;

namespace AdvancedGraphics.HarmonyInstance
{
    
    [HarmonyPatch(typeof(Verb), "get_UIIcon")]
    public static class Verb_get_UIIcon_ColouredWeapons_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Verb __instance, ref Texture2D __result)
        {
            if (__instance.EquipmentSource != null)
            {
                if (__instance.EquipmentSource.Graphic is Graphic_SingleQuality)
                {

                //    Log.Message("UIIcon_ColouredWeapon Graphic_SingleQuality start: " + __result.name);
                    __result = __instance.EquipmentSource.Graphic.MatSingleFor(__instance.EquipmentSource).mainTexture as Texture2D;
                }
                if (__instance.EquipmentSource.Graphic is Graphic_SingleRandomized)
                {

                //    Log.Message("UIIcon_ColouredWeapon Graphic_SingleRandomized start: " + __result.name);
                    __result = __instance.EquipmentSource.Graphic.MatSingleFor(__instance.EquipmentSource).mainTexture as Texture2D;
                }

                if (__instance.EquipmentSource.Graphic is Graphic_RandomRotated r)
                {
                    __result = __instance.EquipmentSource.Graphic.MatSingleFor(__instance.EquipmentSource).mainTexture as Texture2D;
                }
            }

        }
    }
}
