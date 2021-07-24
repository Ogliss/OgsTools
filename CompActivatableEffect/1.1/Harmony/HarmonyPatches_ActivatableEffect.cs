using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace OgsCompActivatableEffect
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches_ActivatableEffect
    {
        public static bool enabled_AlienRaces;
        public static bool enabled_rooloDualWield;
        public static bool enabled_YayosCombat;
        static HarmonyPatches_ActivatableEffect()
        {
            enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
            enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "Roolo.DualWield");
            enabled_YayosCombat = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "com.yayo.combat3");


            var harmony = new Harmony("rimworld.Ogliss.comps.activator");
            
            harmony.Patch(typeof(Pawn).GetMethod("GetGizmos"), null,
                new HarmonyMethod(typeof(HarmonyPatches_ActivatableEffect).GetMethod("GetGizmosPrefix")));

            MethodInfo target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("OgsCompOversizedWeapon.OversizedUtil", "OgsCompOversizedWeapon"), "Draw", null, null);
            if (target == null)
            {
                Log.Warning("Target: OversizedUtil.Draw Not found");
            }
            MethodInfo patch = typeof(ActivatableEffectUtil).GetMethod("DrawMeshExtra");
            if (patch == null)
            {
                Log.Warning("Patch is null ActivatableEffectUtil.DrawMeshExtra");
            }
            if (target != null && patch != null)
            {
                if (harmony.Patch(target, null, new HarmonyMethod(patch)) == null)
                {
                    Log.Warning("OgsCompActivatableEffect: OgsCompOversizedWeapon Patch Failed to apply");
                }
                else
                {
                    if (Prefs.DevMode) Log.Message("ActivatableEffect: OversizedUtil.Draw Patched");
                }
            }
            else
            {
                MethodInfo target2 = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null);
                if (target2 == null && enabled_rooloDualWield)
                {
                    Log.Warning("Target: DualWield.Harmony.PawnRenderer_DrawEquipmentAiming.DrawEquipmentAimingOverride Not found");
                }
                MethodInfo patch2 = typeof(PawnRenderer_DrawEquipmentAiming_DualWield_Transpiler).GetMethod("Transpiler");
                if (patch2 == null && enabled_rooloDualWield)
                {
                    Log.Warning("Patch is null Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler.Transpiler");
                }
                if (target2 != null && patch2 != null && enabled_rooloDualWield)
                {
                    if (harmony.Patch(target2, null, null, new HarmonyMethod(patch2)) == null)
                    {
                        Log.Warning("OgsCompOversizedWeapon: DualWield Patch Failed to apply");
                    }
                }
                else
                {
                    MethodInfo target3 = AccessTools.Method(typeof(PawnRenderer), "DrawEquipmentAiming", null, null);
                    if (target3 == null)
                    {
                        Log.Warning("Target: PawnRenderer.DrawEquipmentAiming Not found");
                    }
                    MethodInfo patch3 = typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler).GetMethod("Transpiler");
                    if (patch3 == null)
                    {
                        Log.Warning("Patch is null Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.Transpiler");
                    }
                    if (target3 != null && patch3 != null)
                    {
                        if (harmony.Patch(target3, null, new HarmonyMethod(patch3)) == null)
                        {
                            Log.Warning("OgsCompActivatableEffect: Patch Failed to apply");
                        }
                    }
                }
            }
            harmony.Patch(typeof(Verb).GetMethod("TryStartCastOn", new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(bool), typeof(bool) }),
                new HarmonyMethod(typeof(HarmonyPatches_ActivatableEffect), nameof(TryStartCastOnPrefix)), null);
            harmony.Patch(typeof(Pawn).GetMethod("ExitMap"),
                new HarmonyMethod(typeof(HarmonyPatches_ActivatableEffect).GetMethod("ExitMap_PreFix")), null);
            harmony.Patch(typeof(Pawn_EquipmentTracker).GetMethod("TryDropEquipment"),
                new HarmonyMethod(typeof(HarmonyPatches_ActivatableEffect), nameof(TryDropEquipment_PreFix)), null);
            harmony.Patch(typeof(Pawn_DraftController).GetMethod("set_Drafted"), null,
                new HarmonyMethod(typeof(HarmonyPatches_ActivatableEffect).GetMethod("set_DraftedPostFix")));
        }


        //=================================== COMPACTIVATABLE

        // Verse.Pawn_EquipmentTracker
        public static void TryDropEquipment_PreFix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            if (__instance is Pawn_EquipmentTracker eqq &&
                eqq.Primary is ThingWithComps t &&
                t.GetComp<OgsCompActivatableEffect.CompActivatableEffect>() is OgsCompActivatableEffect.CompActivatableEffect compActivatableEffect &&
                compActivatableEffect.CurrentState == OgsCompActivatableEffect.CompActivatableEffect.State.Activated)
                compActivatableEffect.TryDeactivate();
        }

        public static void ExitMap_PreFix(Pawn __instance, bool allowedToJoinOrCreateCaravan)
        {
            if (__instance is Pawn p && p.equipment is Pawn_EquipmentTracker eq &&
                eq.Primary is ThingWithComps t &&
                t.GetComp<OgsCompActivatableEffect.CompActivatableEffect>() is OgsCompActivatableEffect.CompActivatableEffect compActivatableEffect &&
                compActivatableEffect.CurrentState == OgsCompActivatableEffect.CompActivatableEffect.State.Activated)
                compActivatableEffect.TryDeactivate();
        }
        
        public static void set_DraftedPostFix(Pawn_DraftController __instance, bool value)
        {
            if (__instance == null)
            {
                return;
            }
            if (__instance.pawn is Pawn p && p.equipment is Pawn_EquipmentTracker eq &&
                eq.Primary is ThingWithComps t &&
                t.GetComp<OgsCompActivatableEffect.CompActivatableEffect>() is OgsCompActivatableEffect.CompActivatableEffect compActivatableEffect)
                if (value == false)
                {
                    if (compActivatableEffect.CurrentState == OgsCompActivatableEffect.CompActivatableEffect.State.Activated)
                        compActivatableEffect.TryDeactivate();
                }
                else
                {
                    if (compActivatableEffect.CurrentState == OgsCompActivatableEffect.CompActivatableEffect.State.Deactivated)
                        compActivatableEffect.TryActivate();
                }
        }

        public static bool TryStartCastOnPrefix(ref bool __result, Verb __instance)
        {
            if (__instance.caster is Pawn pawn)
            {
                var pawn_EquipmentTracker = pawn?.equipment;
                if (pawn_EquipmentTracker == null) return true;

                var thingWithComps =
                    pawn_EquipmentTracker?.Primary; //(ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);

                var compActivatableEffect = thingWithComps?.GetComp<OgsCompActivatableEffect.CompActivatableEffect>();
                if (compActivatableEffect == null) return true;

                //Equipment source throws errors when checked while casting abilities with a weapon equipped.
                // to avoid this error preventing our code from executing, we do a try/catch.
                try
                {
                    if (__instance?.EquipmentSource != thingWithComps)
                        return true;
                }
                catch (Exception e)
                {
                }

                if (compActivatableEffect.CurrentState == OgsCompActivatableEffect.CompActivatableEffect.State.Activated) return true;
                
                if (Find.TickManager.TicksGame % 250 == 0)
                    Messages.Message("DeactivatedWarning".Translate(pawn.Label),
                        MessageTypeDefOf.RejectInput);
                if (!compActivatableEffect.Props.allowUnactivedUse)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }

        ///// <summary>
        ///// Prevents the user from having damage with the verb.
        ///// </summary>
        ///// <param name="__instance"></param>
        ///// <param name="__result"></param>
        ///// <param name="pawn"></param>
        //public static void GetDamageFactorForPostFix(Verb __instance, ref float __result, Pawn pawn)
        //{
        //    Pawn_EquipmentTracker pawn_EquipmentTracker = pawn.equipment;
        //    if (pawn_EquipmentTracker != null)
        //    {
        //        //Log.Message("2");
        //        ThingWithComps thingWithComps = (ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);

        //        if (thingWithComps != null)
        //        {
        //            //Log.Message("3");
        //            CompActivatableEffect compActivatableEffect = thingWithComps.GetComp<CompActivatableEffect>();
        //            if (compActivatableEffect != null)
        //            {
        //                if (compActivatableEffect.CurrentState != CompActivatableEffect.State.Activated)
        //                {
        //                    //Messages.Message("DeactivatedWarning".Translate(), MessageSound.RejectInput);
        //                    __result = 0f;
        //                }
        //            }
        //        }
        //    }
        //}


        /// <summary>
        ///     Adds another "layer" to the equipment aiming if they have a
        ///     weapon with a CompActivatableEffect.
        /// </summary>
        public static IEnumerable<Gizmo> GizmoGetter(OgsCompActivatableEffect.CompActivatableEffect compActivatableEffect)
        {
            if (compActivatableEffect.GizmosOnEquip)
            {
                var enumerator = compActivatableEffect.EquippedGizmos().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    yield return current;
                }
            }
        }

        public static void GetGizmosPrefix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            var pawn_EquipmentTracker = __instance.equipment;
            if (pawn_EquipmentTracker != null)
            {
                var thingWithComps = pawn_EquipmentTracker.Primary;

                if (thingWithComps != null)
                {
                    var compActivatableEffect = thingWithComps.GetComp<OgsCompActivatableEffect.CompActivatableEffect>();
                    if (compActivatableEffect != null)
                        if (__instance != null)
                            if (__instance.Faction == Faction.OfPlayer)
                            {
                                __result = __result.Concat(GizmoGetter(compActivatableEffect));
                            }
                            else
                            {
                                if (compActivatableEffect.CurrentState == OgsCompActivatableEffect.CompActivatableEffect.State.Deactivated)
                                    compActivatableEffect.Activate();
                            }
                }
            }
        }
    }
}