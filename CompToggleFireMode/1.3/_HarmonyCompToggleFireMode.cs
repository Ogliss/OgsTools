using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CompToggleFireMode
{
    [StaticConstructorOnStartup]
    internal static class HarmonyCompToggleFireMode
    {
        static HarmonyCompToggleFireMode()
        {
            var harmony = new Harmony("rimworld.ogliss.comps.CompToggleFireMode");

            var type = typeof(HarmonyCompToggleFireMode);
            harmony.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)), null,
                new HarmonyMethod(type, nameof(GetGizmos_PostFix)));
            harmony.Patch(AccessTools.Method(typeof(CompEquippable), "get_PrimaryVerb"), null,
                new HarmonyMethod(type, nameof(PrimaryVerb_PostFix)));
        }

        public static void PrimaryVerb_PostFix(CompEquippable __instance, ref Verb __result)
        {
            if (__instance.parent.TryGetComp<CompToggleFireMode>() != null)
            {
                __result.verbProps = __instance.parent.TryGetComp<CompToggleFireMode>().Active;
            }
        }

        public static IEnumerable<Gizmo> GizmoGetter(CompToggleFireMode CompToggleFireMode)
        {
            if (CompToggleFireMode.GizmosOnEquip)
            {
                //Iterate EquippedGizmos
                var enumerator = CompToggleFireMode.EquippedGizmos().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    yield return current;
                }
            }
        }

        public static void GetGizmos_PostFix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            var pawn_EquipmentTracker = __instance.equipment;
            if (pawn_EquipmentTracker != null)
            {
                var thingWithComps =
                    pawn_EquipmentTracker
                        .Primary;

                if (thingWithComps != null)
                {
                    var CompToggleFireMode = thingWithComps.GetComp<CompToggleFireMode>();
                    if (CompToggleFireMode != null)
                        if (GizmoGetter(CompToggleFireMode).Count() > 0)
                            if (__instance != null)
                                if (__instance.Faction == Faction.OfPlayer)
                                    __result = __result.Concat(GizmoGetter(CompToggleFireMode));
                }
            }
        }
    }
}