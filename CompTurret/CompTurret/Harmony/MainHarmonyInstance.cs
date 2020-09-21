using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace CompTurret.HarmonyInstance
{
    [StaticConstructorOnStartup]
    public class MainHarmonyInstance : Mod
    {
        public MainHarmonyInstance(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.CompTurret");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

}