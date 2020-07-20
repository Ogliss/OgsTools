using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PerfectlyGenericItem
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.PerfectlyGenericItem");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(BackCompatibility), "BackCompatibleDefName")]
    public static class PGI_BackCompatibility_BackCompatibleDefName_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void BackCompatibleDefName_Postfix(Type defType, string defName, ref string __result)
        {
            if (GenDefDatabase.GetDefSilentFail(defType, defName, false) == null)
            {
                if (defType == typeof(ThingDef))
                {
                    __result = "PerfectlyGenericItem";
                }
                /*
                if (defType == typeof(FactionDef) && PGISettings.Instance.replaceFactions)
                {
                    __result = "OutlanderRough";
                }
                */
            }
        }
    }

}
