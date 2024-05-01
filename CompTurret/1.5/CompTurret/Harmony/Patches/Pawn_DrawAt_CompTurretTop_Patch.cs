using CompTurret.ExtensionMethods;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CompTurret.HarmonyInstance 
{
    [HarmonyPatch(typeof(Pawn), "DrawAt")]
    public static class Pawn_DrawAt_CompTurretTop_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Pawn __instance)
        {
            if (__instance.apparel!=null)
            {
                if (__instance.apparel.WornApparel.Any())
                {

                    foreach (var item in __instance.apparel.WornApparel)
                    {
                        CompTurret turret = item.TryGetCompFast<CompTurret>();
                        if (turret!=null)
                        {
                            foreach (CompTurret comp in item.GetComps<CompTurret>())
                            {
                                if (comp.Props.drawTurret)
                                {
                                    comp.PostDraw();
                                }
                            }
                        }
                    }

                }
            }
        }
    }
}
