using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using HunterMarkingSystem.Settings;
using HunterMarkingSystem.ExtensionMethods;

namespace HunterMarkingSystem
{
    [StaticConstructorOnStartup]
    class HarmonyMain
    {
        static HarmonyMain()
        {
            //    HarmonyInstance.DEBUG = true;
            var harmony = new Harmony("com.ogliss.rimworld.mod.HunterMarkingSystem");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}