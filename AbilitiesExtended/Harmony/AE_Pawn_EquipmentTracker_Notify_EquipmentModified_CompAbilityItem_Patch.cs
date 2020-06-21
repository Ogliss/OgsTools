﻿using System;
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
            if (eq.TryGetComp<AbilitesExtended.CompAbilityItem>() != null && eq.TryGetComp<AbilitesExtended.CompAbilityItem>() is AbilitesExtended.CompAbilityItem abilityItem)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.RaceProps.Humanlike)
                {
                    return;
                }
                if (!abilityItem.Props.Abilities.NullOrEmpty())
                {
                    foreach (AbilityDef def in abilityItem.Props.Abilities)
                    {
                        if (!__instance.pawn.abilities.abilities.Any(x => x.def == def))
                        {
                            __instance.pawn.abilities.GainAbility(def, eq);
                        }
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
            if (eq.TryGetComp<AbilitesExtended.CompAbilityItem>() != null && eq.TryGetComp<AbilitesExtended.CompAbilityItem>() is AbilitesExtended.CompAbilityItem abilityItem)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.RaceProps.Humanlike)
                {
                    return;
                }
                if (!abilityItem.Props.Abilities.NullOrEmpty())
                {
                    foreach (AbilityDef def in abilityItem.Props.Abilities)
                    {
                        if (__instance.pawn.abilities.abilities.Any(x => x.def == def))
                        {
                            Ability ability = __instance.pawn.abilities.abilities.Find(x => x.def == def);
                            __instance.pawn.abilities.abilities.Remove(ability);
                        }
                    }
                }
            }
        }
    }

}
