using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ApparelRestrictor.HarmonyInstance
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {

            var harmony = new Harmony("com.ogliss.rimworld.mod.ApparelRestrictor");
            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new Type[]
            {
                typeof(Thing),
                typeof(Pawn),
                typeof(string).MakeByRefType()
            }, null), null, new HarmonyMethod(typeof(AR_EquipmentUtility_CanEquip_Restricted_Patch).GetMethod("Postfix")));
        }

    }

}