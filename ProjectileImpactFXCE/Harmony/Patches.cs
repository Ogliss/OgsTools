using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using CombatExtended;

namespace ProjectileImpactFX.HarmonyInstance
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.ProjectileImpactFX");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}