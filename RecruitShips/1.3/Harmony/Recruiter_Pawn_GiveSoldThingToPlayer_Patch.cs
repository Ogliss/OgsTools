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
using UnityEngine;
using System.Reflection;

namespace Recruiters
{
    [HarmonyPatch(typeof(Pawn), "GiveSoldThingToPlayer")]
    public static class Recruiter_Pawn_GiveSoldThingToPlayer_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            if (__instance.TraderKind.HasModExtension<RecruiterExt>())
            {
                Pawn pawn = toGive as Pawn;
                if (pawn != null)
                {
                    pawn.SetFaction(playerNegotiator.Faction, playerNegotiator);
                }
            }
        }
    }
}
