using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Dessicator
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.Dessicator");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    // Desicator.DesicatorExt
    public class DessicatorExt : DefModExtension
    {
        public HediffDef HediffDef;
        public ThingDef desicatedDef;
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Dessicator_Pawn_Kill_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (dinfo.HasValue)
            {
                if (dinfo.Value.Instigator != null)
                {
                    Thing inst = dinfo.Value.Instigator;
                    DessicatorExt desicator = inst.def.GetModExtension<DessicatorExt>();
                    if (desicator != null)
                    {
                        if (desicator.desicatedDef != null)
                        {
                            FieldInfo corpse = typeof(Pawn).GetField("Corpse", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
                            Traverse.Create(__instance);
                            corpse.SetValue(__instance, ThingMaker.MakeThing(desicator.desicatedDef));
                        }
                        else
                        {
                            CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
                            compRottable.RotImmediately();
                        }
                    }
                }
            }
            HediffDef def = DefDatabase<HediffDef>.GetNamed("RT_HuskTouch");
            if (__instance.health.hediffSet.HasHediff(def))
            {
            //    Log.Message("husktouch present");
                CompRottable compRottable = __instance.Corpse.TryGetComp<CompRottable>();
                compRottable.RotImmediately();
            }
        }
    }
}