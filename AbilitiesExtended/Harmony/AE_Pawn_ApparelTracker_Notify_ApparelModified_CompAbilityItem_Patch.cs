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

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    public static class AE_Pawn_ApparelTracker_Notify_ApparelRemoved_CompAbilityItem_Patch
    {
        [HarmonyPostfix] // Apparel apparel
        public static void Notify_ApparelRemovedPostfix(Pawn_EquipmentTracker __instance, Apparel apparel)
        {
            bool abilityitem = apparel.TryGetComp<AbilitesExtended.CompAbilityItem>() != null;
            if (abilityitem)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.RaceProps.Humanlike)
                {
                    return;
                }
                foreach (AbilitesExtended.CompAbilityItem compAbilityItem in apparel.GetComps<AbilitesExtended.CompAbilityItem>())
                {
                    if (__instance.pawn.abilities.abilities.Any(x => compAbilityItem.Props.Abilities.Contains(x.def)))
                    {
                        foreach (AbilityDef abilityDef in compAbilityItem.Props.Abilities)
                        {
                            if (__instance.pawn.abilities.abilities.Any(x => compAbilityItem.Props.Abilities.Contains(abilityDef)))
                            {
                                Ability ability = __instance.pawn.abilities.abilities.Find(x => x.def == abilityDef);
                                __instance.pawn.abilities.abilities.Remove(ability);
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    public static class AE_Pawn_ApparelTracker_Notify_ApparelAdded_CompAbilityItem_Patch
    {
        [HarmonyPostfix] // Apparel apparel
        public static void Notify_Notify_ApparelAddedPostfix(Pawn_EquipmentTracker __instance, Apparel apparel)
        {
            if (apparel.TryGetComp<AbilitesExtended.CompAbilityItem>() != null && apparel.TryGetComp<AbilitesExtended.CompAbilityItem>() is AbilitesExtended.CompAbilityItem abilityItem)
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
                            __instance.pawn.abilities.GainAbility(def, apparel);
                            Ability ability = __instance.pawn.abilities.abilities.Find(x => x.def == def);
                        }
                    }
                }
            }
        }
    }

}
