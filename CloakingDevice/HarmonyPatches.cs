using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CloakingDevice.HarmonyInstance
{
    public class HarmonyPatches
    {
        public static Harmony harmony = null;
        public HarmonyPatches()
        {
            harmony = new Harmony("com.ogliss.rimworld.mod.CloakingDevice");
            if (AccessTools.GetMethodNames(typeof(PawnGraphicSet)).Contains("HeadMatAt_NewTemp"))
            {
                HarmonyPatches.HeadMatAt_NewTemp();
            }
            else
            {
                HarmonyPatches.HeadMatAt();
            }

            if (AccessTools.GetMethodNames(typeof(PawnGraphicSet)).Contains("HairMatAt_NewTemp"))
            {
                HarmonyPatches.HairMatAt_NewTemp();
            }
            else
            {
                HarmonyPatches.HairMatAt();
            }

            if (AccessTools.GetMethodNames(typeof(PawnRenderer)).Contains("OverrideMaterialIfNeeded_NewTemp"))
            {
                HarmonyPatches.OverrideMaterialIfNeeded_NewTemp();
            }
            else
            {
                HarmonyPatches.OverrideMaterialIfNeeded();
            }
        }
        public static void OverrideMaterialIfNeeded()
        {
            HarmonyPatches.harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded", null, null), null, new HarmonyMethod(typeof(AvP_PawnRenderer_OverrideMaterialIfNeeded_Xenomorph_Patch), "Postfix", null), null, null);
        }

        public static void OverrideMaterialIfNeeded_NewTemp()
        {
            HarmonyPatches.harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded_NewTemp", null, null), null, new HarmonyMethod(typeof(AvP_PawnRenderer_OverrideMaterialIfNeeded_NewTemp_Xenomorph_Patch), "Postfix", null), null, null);
        }

        public static void HairMatAt()
        {
            HarmonyPatches.harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "HairMatAt", null, null), null, new HarmonyMethod(typeof(AvP_PawnGraphicSet_HairMatAt_Invis_Patch), "Postfix", null), null, null);
        }

        public static void HairMatAt_NewTemp()
        {
            HarmonyPatches.harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "HairMatAt_NewTemp", null, null), null, new HarmonyMethod(typeof(AvP_PawnGraphicSet_HairMatAt_NewTemp_Invis_Patch), "Postfix", null), null, null);
        }
        public static void HeadMatAt()
        {
            HarmonyPatches.harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "HeadMatAt", null, null), null, new HarmonyMethod(typeof(AvP_PawnGraphicSet_HeadMatAt_Invis_Patch), "Postfix", null), null, null);
        }

        public static void HeadMatAt_NewTemp()
        {
            HarmonyPatches.harmony.Patch(AccessTools.Method(typeof(PawnGraphicSet), "HeadMatAt_NewTemp", null, null), null, new HarmonyMethod(typeof(AvP_PawnGraphicSet_HeadMatAt_NewTemp_Invis_Patch), "Postfix", null), null, null);
        }

    }
}
