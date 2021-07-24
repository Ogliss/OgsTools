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

namespace AbilitesExtended.HarmonyInstance
{

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    public static class AE_Pawn_EquipmentTracker_Notify_EquipmentAdded_CompAbilityItem_Patch 
    {
        [HarmonyPostfix]
        public static void Notify_EquipmentAddedPostfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            if (eq == null || __instance == null)
            {
                return;
            }
            if (eq.TryGetCompFast<AbilitesExtended.CompAbilityItem>() != null && eq.TryGetCompFast<AbilitesExtended.CompAbilityItem>() is AbilitesExtended.CompAbilityItem abilityItem)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.RaceProps.Humanlike)
                {
                    return;
                }
                if (!abilityItem.Props.Abilities.NullOrEmpty())
                {
                    foreach (EquipmentAbilityDef def in abilityItem.Props.Abilities)
                    {
                        __instance.pawn.abilities.TryGainEquipmentAbility(def, eq);
                    }
                }
            }
        }


    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentRemoved")]
    public static class AE_Pawn_EquipmentTracker_Notify_EquipmentRemoved_CompAbilityItem_Patch
    {
        [HarmonyPostfix]
        public static void Notify_EquipmentRemovedPostfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            if (eq == null || __instance == null)
            {
                return;
            }
            if (eq.TryGetCompFast<AbilitesExtended.CompAbilityItem>() != null && eq.TryGetCompFast<AbilitesExtended.CompAbilityItem>() is AbilitesExtended.CompAbilityItem abilityItem)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.RaceProps.Humanlike)
                {
                    return;
                }
                if (!abilityItem.Props.Abilities.NullOrEmpty())
                {
                    foreach (AbilityDef abilityDef in abilityItem.Props.Abilities)
                    {
                        __instance.pawn.abilities.TryRemoveEquipmentAbility(abilityDef, eq);
                    }
                }
            }
        }
    }

}
