using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace CloakingDevice.HarmonyInstance
{
    [HarmonyPatch(typeof(PawnUtility), "IsInvisible")]
    public static class AvP_PawnUtility_IsInvisible_Patch
    {
        [HarmonyPostfix]
        public static void ThoughtsFromIngestingPostPrefix(Pawn pawn, ref bool __result)
        {
            if (pawn == null)
            {
                return;
            }
            if (pawn.CarriedBy != null)
            {
                __result = pawn.CarriedBy.IsInvisible();
                return;
            }
            if (pawn.RaceProps.Humanlike)
            {
                if (pawn.apparel.WornApparel.Any(x => x.TryGetComp<CompCloakGenerator>() != null))
                {
                    __result = __result || pawn.apparel.WornApparel.Any(x => x.TryGetComp<CompCloakGenerator>() != null && x.TryGetComp<CompCloakGenerator>().cloakMode == CompCloakGenerator.CloakMode.On);
                }
            }
        }
    }

}