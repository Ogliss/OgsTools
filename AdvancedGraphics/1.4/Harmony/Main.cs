using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace AdvancedGraphics.HarmonyInstance
{
    [StaticConstructorOnStartup]
    class Main
    {
        public static bool enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.AdvancedGraphics");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}