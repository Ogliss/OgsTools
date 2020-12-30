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
            Type type = AccessTools.TypeByName("CombatExtended.ProjectileCE");
            if (type != null)
            {

                string s = "CE Patching: ";

                if (CombatExtendedPatch(harmony))
                {
                    s += "Complete";
                }
                else
                {
                    s += "Failed";
                }
                Log.Message(s);
            }
            else
            {
                if (!VanillaPatch(harmony))
                {
                    Log.Warning("Vanilla Patch Failed");
                }
            }
         //   harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool CombatExtendedPatch(Harmony harmony)
        {
            Type type = AccessTools.TypeByName("CombatExtended.ProjectileCE");
            MethodInfo target = type.GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (target == null)
            {
                Log.Warning("Target: CombatExtended.ProjectileCE.Impact Not found");
                return false;
            }
            MethodInfo patch = typeof(Projectile_Impact_EffectProjectileExtension_Patch_CE).GetMethod("Prefix");
            if (patch == null)
            {
                Log.Warning("Patch is null Projectile_Impact_EffectProjectileExtension_Patch_CE.Prefix");
                return false;
            }
            return harmony.Patch(target, new HarmonyMethod(patch)) != null;

        }
        public static bool VanillaPatch(Harmony harmony)
        {
            MethodInfo target = typeof(Projectile).GetMethod("Impact", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (target == null)
            {
                Log.Warning("Target is null Projectile.Impact");
                return false;
            }
            MethodInfo patch = typeof(Projectile_Impact_EffectProjectileExtension_Patch).GetMethod("Prefix");
            if (patch == null)
            {
                Log.Warning("Patch is null Projectile_Impact_EffectProjectileExtension_Patch.Prefix");
                return false;
            }
            return harmony.Patch(target, new HarmonyMethod(patch)) != null;
        }
    }
}