using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AdeptusMechanicus.HarmonyInstance
{
    [StaticConstructorOnStartup]
    public static class HarmonyInstance
    {

        static HarmonyInstance()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.OgsLasers");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}