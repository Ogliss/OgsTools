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

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    public static class AE_Pawn_ApparelTracker_Notify_ApparelRemoved_CompAbilityItem_Patch
    {
        [HarmonyPostfix] // Apparel apparel
        public static void Notify_ApparelRemovedPostfix(Pawn_EquipmentTracker __instance, Apparel apparel)
        {
            bool abilityitem = apparel.TryGetCompFast<AbilitesExtended.CompAbilityItem>() != null;
            if (abilityitem)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.RaceProps.Humanlike)
                {
                    return;
                }
                foreach (AbilitesExtended.CompAbilityItem compAbilityItem in apparel.GetComps<AbilitesExtended.CompAbilityItem>())
                {
                    foreach (AbilityDef abilityDef in compAbilityItem.Props.Abilities)
                    {
                        __instance.pawn.abilities.TryRemoveEquipmentAbility(abilityDef, apparel);
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
            if (apparel.TryGetCompFast<AbilitesExtended.CompAbilityItem>() != null && apparel.TryGetCompFast<AbilitesExtended.CompAbilityItem>() is AbilitesExtended.CompAbilityItem abilityItem)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.RaceProps.Humanlike)
                {
                    return;
                }
                if (!abilityItem.Props.Abilities.NullOrEmpty())
                {
                    bool dirty = false;
                    foreach (EquipmentAbilityDef def in abilityItem.Props.Abilities)
                    {
                        if (!__instance.pawn.abilities.abilities.Any(x => x.def == def) && (!def.requirePsyker || isPsyker(pawn)))
                        {
                            __instance.pawn.abilities.TryGainEquipmentAbility(def, apparel);
                        }
                    }
                }
            }
        }

        public static bool isPsyker(Pawn pawn)
        {
            return isPsyker(pawn, out int Level);
        }

        public static bool isPsyker(Pawn pawn, out int Level)
        {
            return isPsyker(pawn, out Level, out float Mult);
        }

        public static bool isPsyker(Pawn pawn, out int Level, out float Mult)
        {
            bool result = false;
            Mult = 0f;
            Level = 0;

            if (pawn.RaceProps.Humanlike)
            {
                if (pawn.health.hediffSet.hediffs.Any(x => x.GetType() == typeof(Hediff_Level)))
                {
                    Level = (pawn.health.hediffSet.hediffs.First(x => x.GetType() == typeof(Hediff_Level)) as Hediff_Level).level;
                    result = true;
                }
                else
                if (pawn.story.traits.HasTrait(TraitDefOf.PsychicSensitivity))
                {
                    result = pawn.story.traits.DegreeOfTrait(TraitDefOf.PsychicSensitivity) > 0;
                    Level = pawn.story.traits.DegreeOfTrait(TraitDefOf.PsychicSensitivity);
                }
                else
                {
                    TraitDef Corruptionpsyker = DefDatabase<TraitDef>.GetNamedSilentFail("Psyker");
                    if (Corruptionpsyker != null)
                    {
                        result = true;
                        pawn.story.traits.HasTrait(Corruptionpsyker);
                        Level = pawn.story.traits.DegreeOfTrait(Corruptionpsyker);
                    }
                }
                Mult = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * (pawn.needs.mood.CurInstantLevelPercentage - pawn.health.hediffSet.PainTotal);
            }
            /*
            else
            {
                ToolUserPskyerDefExtension extension = null;
                if (pawn.def.HasModExtension<ToolUserPskyerDefExtension>())
                {
                    extension = pawn.def.GetModExtension<ToolUserPskyerDefExtension>();
                }
                else
                if (pawn.kindDef.HasModExtension<ToolUserPskyerDefExtension>())
                {
                    extension = pawn.kindDef.GetModExtension<ToolUserPskyerDefExtension>();
                }
                if (extension != null)
                {
                    result = true;
                    Level = extension.Level;
                }
                if (pawn.needs != null && pawn.needs.mood != null)
                {
                    Mult = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * (pawn.needs.mood.CurInstantLevelPercentage - pawn.health.hediffSet.PainTotal);
                }
                else
                {
                    Mult = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * (1 - pawn.health.hediffSet.PainTotal);
                }
            }
            */
            return result;
        }

    }

}
