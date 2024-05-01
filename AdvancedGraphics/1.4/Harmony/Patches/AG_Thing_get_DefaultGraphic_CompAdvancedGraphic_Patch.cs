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

namespace AdvancedGraphics.HarmonyInstance
{
    [HarmonyPatch(typeof(Thing), "get_DefaultGraphic")]
    public static class Thing_get_DefaultGraphic_AdvancedGraphic_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Thing __instance, Graphic __result, ref Graphic ___graphicInt)
        {
            if (__instance != null)
            {
                if (___graphicInt == null)
                {
                    Graphic Graphic = __instance.def.graphicData?.Graphic;
                    if (Graphic != null && Graphic is Graphic_RandomRotated R && R != null)
                    {
                        //    Graphic_SingleQuality quality = R.ExtractInnerGraphicFor(__instance) as Graphic_SingleQuality;
                        if (R.subGraphic is Graphic_SingleQuality quality)
                        {
                            ___graphicInt = new Graphic_RandomRotated(quality.QualityGraphicFor(__instance), 35f);
                        }
                        else if (R.subGraphic is Graphic_SingleRandomized randomized)
                        {
                            ___graphicInt = new Graphic_RandomRotated(randomized.RandomGraphicFor(__instance), 35f);
                        }
                    }
                }
            }
        }
    }
}