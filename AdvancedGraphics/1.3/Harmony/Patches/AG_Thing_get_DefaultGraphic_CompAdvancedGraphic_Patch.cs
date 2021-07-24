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
    public static class AG_Thing_get_DefaultGraphic_CompAdvancedGraphic_Patch
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
                        Graphic innerGraphic = AG_Thing_get_DefaultGraphic_CompAdvancedGraphic_Patch.subgraphic.GetValue(Graphic) as Graphic;
                        Graphic_SingleQuality quality = innerGraphic as Graphic_SingleQuality;
                        //    Graphic_SingleQuality quality = R.ExtractInnerGraphicFor(__instance) as Graphic_SingleQuality;
                        if (quality != null)
                        {
                            ___graphicInt = new Graphic_RandomRotated(quality.QualityGraphicFor(__instance), 35f);
                        }
                        else
                        {
                            Graphic_SingleRandomized randomized = innerGraphic as Graphic_SingleRandomized;
                            //    Graphic_SingleRandomized randomized = R.ExtractInnerGraphicFor(__instance) as Graphic_SingleRandomized;
                            if (randomized != null)
                            {
                                ___graphicInt = new Graphic_RandomRotated(randomized.RandomGraphicFor(__instance), 35f);
                            }
                        }
                    }
                }
            }
        }
        public static FieldInfo subgraphic = typeof(Graphic_RandomRotated).GetField("subGraphic", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
    }
}