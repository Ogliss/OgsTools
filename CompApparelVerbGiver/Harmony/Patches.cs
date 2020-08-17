using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace CompApparelVerbGiver
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.CompApparelVerbGiver");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
        }

    }
}