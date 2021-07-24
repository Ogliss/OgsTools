using HarmonyLib;

namespace RimUniques.HarmonyInstance
{
    public static class HarmonyPatches
    {
        public static void Main()
        {
            Harmony harmony = new Harmony("RimWorld.RimUniques");
            harmony.PatchAll();
        }
    }
}