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
    public static class HarmonyCompActivatableEffect
    {
        public static bool enabled_AlienRaces;
        public static bool enabled_rooloDualWield;
        static HarmonyCompActivatableEffect()
        {
            enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
            enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "Roolo.DualWield");

            var harmony = new Harmony("rimworld.Ogliss.comps.activator");
            
            harmony.Patch(typeof(Pawn).GetMethod("GetGizmos"), null,
                new HarmonyMethod(typeof(HarmonyCompActivatableEffect).GetMethod("GetGizmosPrefix")));

            MethodInfo target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("OgsCompOversizedWeapon.Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler", "OgsCompOversizedWeapon"), "draw", null, null);
            if (target == null)
            {
                Log.Warning("Target: Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.draw Not found");
            }
            MethodInfo patch = typeof(HarmonyCompActivatableEffect).GetMethod("DrawMeshModified");
            if (patch == null)
            {
                Log.Warning("Patch is null HarmonyCompActivatableEffect.DrawMeshModified");
            }
            if (target != null && patch != null)
            {
                if (harmony.Patch(target, null, new HarmonyMethod(patch)) == null)
                {
                    Log.Warning("OgsCompActivatableEffect: OgsCompOversizedWeapon Patch Failed to apply");
                }
            }
            else
            {
                MethodInfo target2 = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null);
                if (target2 == null && enabled_rooloDualWield)
                {
                    Log.Warning("Target: DualWield.Harmony.PawnRenderer_DrawEquipmentAiming.DrawEquipmentAimingOverride Not found");
                }
                MethodInfo patch2 = typeof(Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler).GetMethod("Transpiler");
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
                    MethodInfo patch3 = typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler).GetMethod("Transpiler");
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
                new HarmonyMethod(typeof(HarmonyCompActivatableEffect), nameof(TryStartCastOnPrefix)), null);
            harmony.Patch(typeof(Pawn).GetMethod("ExitMap"),
                new HarmonyMethod(typeof(HarmonyCompActivatableEffect).GetMethod("ExitMap_PreFix")), null);
            harmony.Patch(typeof(Pawn_EquipmentTracker).GetMethod("TryDropEquipment"),
                new HarmonyMethod(typeof(HarmonyCompActivatableEffect).GetMethod("TryDropEquipment_PreFix")), null);
            harmony.Patch(typeof(Pawn_DraftController).GetMethod("set_Drafted"), null,
                new HarmonyMethod(typeof(HarmonyCompActivatableEffect).GetMethod("set_DraftedPostFix")));
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
        public static void DrawMeshModified(Mesh mesh, Matrix4x4 matrix, Material mat, int layer, Thing eq, Pawn pawn, Vector3 position, Quaternion rotation)
        {
            //    Log.Message("DrawMeshModified");
            ThingWithComps thingWithComps = eq as ThingWithComps;
            CompEquippable equippable = eq.TryGetComp<CompEquippable>();
            OgsCompOversizedWeapon.CompOversizedWeapon compOversized = thingWithComps.TryGetComp<OgsCompOversizedWeapon.CompOversizedWeapon>();
            var compActivatableEffect = thingWithComps?.GetComp<CompActivatableEffect>();
            if (compActivatableEffect?.Graphic == null) return;
            if (!compActivatableEffect.IsActiveNow) return;
            var matSingle = compActivatableEffect.Graphic.MatSingle;
            Vector3 s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            if (compOversized != null)
            {
                if (pawn.RaceProps.Humanlike)
                {
                    if (HarmonyCompActivatableEffect.enabled_AlienRaces)
                    {
                        Vector2 v = AlienRaceUtility.AlienRacesPatch(pawn, eq);
                        float f = Mathf.Max(v.x, v.y);
                        s = new Vector3(eq.def.graphicData.drawSize.x * f, 1f, eq.def.graphicData.drawSize.y * f);
                    }
                    else
                    {
                        s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                    }
                }
                else
                {
                    Vector2 v = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                    s = new Vector3(eq.def.graphicData.drawSize.x + v.x / 10, 1f, eq.def.graphicData.drawSize.y + v.y / 10);
                }
                //    Log.Message("DrawEquipmentAimingPostFix compOversized offset: "+ offset + " Rotation: "+rotation + " size: " + s);
            }
            Vector3 vector3 = position;
            vector3.y -= 0.0005f;
            matrix.SetTRS(vector3, rotation, s);
            Graphics.DrawMesh(mesh, matrix, matSingle, 0);
        }
        public static IEnumerable<Gizmo> GizmoGetter(OgsCompActivatableEffect.CompActivatableEffect compActivatableEffect)
        {
            //Log.Message("5");
            if (compActivatableEffect.GizmosOnEquip)
            {
                //Log.Message("6");
                //Iterate EquippedGizmos
                var enumerator = compActivatableEffect.EquippedGizmos().GetEnumerator();
                while (enumerator.MoveNext())
                {
                    //Log.Message("7");
                    var current = enumerator.Current;
                    yield return current;
                }
            }
        }

        public static void GetGizmosPrefix(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            //Log.Message("1");
            var pawn_EquipmentTracker = __instance.equipment;
            if (pawn_EquipmentTracker != null)
            {
                //Log.Message("2");
                //ThingWithComps thingWithComps = (ThingWithComps)AccessTools.Field(typeof(Pawn_EquipmentTracker), "primaryInt").GetValue(pawn_EquipmentTracker);
                var thingWithComps = pawn_EquipmentTracker.Primary;

                if (thingWithComps != null)
                {
                    //Log.Message("3");
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