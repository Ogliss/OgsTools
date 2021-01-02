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

namespace CompTurret.HarmonyInstance 
{
    [HarmonyPatch(typeof(Pawn), "Tick")]
    public static class Apparel_Tick_CompTurret_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance)
        {
            if (__instance == null)
            {
                return;
            }
            if (__instance.apparel == null)
            {
                return;
            }
            if (__instance.apparel.WornApparel.NullOrEmpty())
            {
                return;
            }
            for (int i = 0; i < __instance.apparel.WornApparel.Count; i++)
            {
                Apparel apparel = __instance.apparel.WornApparel[i];
                for (int ii = 0; ii < apparel.AllComps.Count; ii++)
                {
                    CompTurretGun turretGun = apparel.AllComps[ii] as CompTurretGun;
                    if (turretGun != null)
                    {
                        turretGun.CompTick();
                    }
                }
            }
        }
    }
    
}