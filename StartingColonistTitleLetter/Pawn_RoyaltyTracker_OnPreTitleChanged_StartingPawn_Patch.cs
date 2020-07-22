using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace StartingColonistTitleLetter
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.StartingColonistTitleLetter");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), "OnPreTitleChanged")]
    public static class Pawn_RoyaltyTracker_OnPreTitleChanged_StartingPawn_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_RoyaltyTracker __instance, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle, bool sendLetter)
        {
            Pawn pawn = __instance.pawn;
            List<Pawn> startingPawns = Find.GameInitData.startingAndOptionalPawns;
            if (startingPawns.NullOrEmpty())
            {
                Log.Message("startingPawns.NullOrEmpty()");
                return true;
            }
            if (pawn.Faction == Faction.OfPlayer)
            {
                if (startingPawns.Contains(pawn))
                {
                    OnPreTitleChanged(__instance, faction, currentTitle, newTitle, false);
                    return false;
                }
            }
            return true;
        }
        private static void OnPreTitleChanged(Pawn_RoyaltyTracker __instance, Faction faction, RoyalTitleDef currentTitle, RoyalTitleDef newTitle, bool sendLetter = true)
        {
            __instance.AssignHeirIfNone(newTitle, faction);
            if (currentTitle != null)
            {
                for (int i = 0; i < currentTitle.grantedAbilities.Count; i++)
                {
                    __instance.pawn.abilities.RemoveAbility(currentTitle.grantedAbilities[i]);
                }
            }
        }
    }

}
