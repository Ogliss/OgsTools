using System.Linq;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace ExtraApparelLayers
{
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveApparelGraphics")]
    public static class PawnGraphicSet_ResolveApparelGraphics_ApparelLayerDrawOrder_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(ref PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            if (!pawn.RaceProps.Humanlike)
            {
                return;
            }
            if (__instance.apparelGraphics.NullOrEmpty())
            {
                return;
            }
            __instance.apparelGraphics.OrderBy(x => x.sourceApparel.def.apparel.LastLayer.drawOrder);
        }
    }

}
