using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using Verse.Sound;

namespace OgsCompOversizedWeapon
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    public static class Pawn_EquipmentTracker_Notify_EquipmentAdded_ActivatableEffect_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {

            if (eq.TryGetCompFast<CompOversizedWeapon>() != null && eq.TryGetCompFast<CompOversizedWeapon>() is CompOversizedWeapon oversized)
            {
                if (oversized.Props != null && oversized.Props.groundGraphic != null)
                {
                    Graphic g = eq.DefaultGraphic;
                }
            }

        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentRemoved")]
    public static class Pawn_EquipmentTracker_Notify_EquipmentRemoved_ActivatableEffect_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {

            if (eq.TryGetCompFast<CompOversizedWeapon>() != null && eq.TryGetCompFast<CompOversizedWeapon>() is CompOversizedWeapon oversized)
            {
                if (oversized.Props != null && oversized.Props.groundGraphic != null)
                {
                    Graphic g = eq.DefaultGraphic;
                }
            }

        }
    }
}
