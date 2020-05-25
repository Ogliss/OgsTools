﻿using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AdeptusMechanicus
{
    [StaticConstructorOnStartup]
    public static class HarmonyCompOversizedWeapon
    {
        public static bool enabled_AlienRaces;
        public static bool enabled_rooloDualWield;
        static HarmonyCompOversizedWeapon()
        {
            enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces"); 
            enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "Roolo.DualWield");
            var harmony = new Harmony("rimworld.Ogliss.comps.oversized");

            if (enabled_rooloDualWield)
            {
                /*
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("CompOversizedWeapon.HarmonyCompOversizedWeapon", "CompOversizedWeapon"), "DrawEquipmentAimingPreFix", null, null), new HarmonyMethod(Main.patchType, "`", null), new HarmonyMethod(Main.patchType, "DrawEquipmentAiming_DualWield_OverSized_PostFix", null));
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("CompActivatableEffect.HarmonyCompActivatableEffect", "CompActivatableEffect"), "DrawEquipmentAimingPostFix", null, null), new HarmonyMethod(Main.patchType, "DrawEquipmentAiming_DualWield_Activatable_PreFix", null));
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null), new HarmonyMethod(Main.patchType, "DrawEquipmentAimingOverride_DualWield_compActivatableEffect_PreFix", null));
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Ext_Pawn_EquipmentTracker", "DualWield"), "AddOffHandEquipment", null, null),null , new HarmonyMethod(Main.patchType, "AddOffHandEquipment_PostFix", null));
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnWeaponGenerator_TryGenerateWeaponFor", "DualWield.Harmony"), "Postfix", null, null), new HarmonyMethod(Main.patchType, "PawnWeaponGenerator_TryGenerateWeaponFor_PostFix", null));
                */

                /*
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingPreFix_DualWield")), null);
                harmony.Patch(typeof(PawnRenderer).GetMethod("DrawEquipmentAiming"),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingPreFix")), null);
                */
                /*
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingOverride_DualWieldPreFix")), null);
                */
                /*
                harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingOverride")), null);
                */

                harmony.Patch(typeof(DualWield.Harmony.PawnRenderer_DrawEquipmentAiming).GetMethod("DrawEquipmentAimingOverride"),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingOverride")), null);

                harmony.Patch(typeof(PawnRenderer).GetMethod("DrawEquipmentAiming"),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingDualWieldPreFix")), null);
                /*
                harmony.Patch(typeof(PawnRenderer).GetMethod("DrawEquipmentAiming"),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingDualWieldPreFix")), null);
                */
            }
            else
            {
                harmony.Patch(typeof(PawnRenderer).GetMethod("DrawEquipmentAiming"),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon).GetMethod("DrawEquipmentAimingPreFix")), null);

            }

            harmony.Patch(AccessTools.Method(typeof(Thing), "get_DefaultGraphic"), null,
                new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(get_DefaultGraphic_PostFix)));
            /*
            harmony.Patch(AccessTools.Method(typeof(Thing), "get_DefaultGraphic"), null,
                new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(get_Graphic_PostFix)));
            */
        }


        /// <summary>
        ///     Adds another "layer" to the equipment aiming if they have a
        ///     weapon with a CompActivatableEffect.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="eq"></param>
        /// <param name="drawLoc"></param>
        /// <param name="aimAngle"></param>
        public static bool DrawEquipmentAimingPreFix(Pawn ___pawn, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            ThingWithComps thingWithComps = eq as ThingWithComps;
            bool flag = thingWithComps != null;
            if (flag)
            {
                ThingComp thingComp = thingWithComps.AllComps.FirstOrDefault((ThingComp y) => y.GetType().ToString() == "CompDeflector.CompDeflector" || y.GetType().BaseType.ToString() == "CompDeflector.CompDeflector");
                bool flag2 = thingComp != null && Traverse.Create(thingComp).Property("IsAnimatingNow", null).GetValue<bool>();
                if (flag2)
                {
                    return false;
                }
                AdeptusMechanicus.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<AdeptusMechanicus.CompOversizedWeapon>(thingWithComps);
                AdeptusMechanicus.CompActivatableEffect compActivatableEffect = ThingCompUtility.TryGetComp<AdeptusMechanicus.CompActivatableEffect>(thingWithComps);
                bool flag3 = compOversizedWeapon != null;
                if (flag3)
                {
                    bool flag4 = false;
                    float num = aimAngle - 90f;
                    if (___pawn == null)
                    {
                        return true;
                    }
                    if (aimAngle > 20f && aimAngle < 160f)
                    {
                        Mesh plane = MeshPool.plane10;
                        num += eq.def.equippedAngleOffset;
                    }
                    else if (aimAngle > 200f && aimAngle < 340f)
                    {
                        Mesh plane10Flip = MeshPool.plane10Flip;
                        flag4 = true;
                        num -= 180f;
                        num -= eq.def.equippedAngleOffset;
                    }
                    else
                    {
                        num = AdjustOffsetAtPeace(eq, ___pawn, compOversizedWeapon, num);
                    }
                    if (compOversizedWeapon.Props != null && !PawnUtility.IsFighting(___pawn) && compOversizedWeapon.Props.verticalFlipNorth && ___pawn.Rotation == Rot4.North)
                    {
                        num += 180f;
                    }
                    if (!PawnUtility.IsFighting(___pawn) || ___pawn.TargetCurrentlyAimingAt == null)
                    {
                        num = AdjustNonCombatRotation(___pawn, num, compOversizedWeapon);
                    }
                    num %= 360f;
                    Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
                    bool flag10 = graphic_StackCount != null;
                    Material matSingle;
                    if (flag10)
                    {
                        matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
                    }
                    else
                    {
                        matSingle = eq.Graphic.MatSingle;
                    }
                    Vector3 s;
                    if (enabled_AlienRaces)
                    {
                        Vector2 v = AlienRacesPatch(___pawn);
                        s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                    }
                    else
                    {
                        s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                    }
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 vector = HarmonyCompOversizedWeapon.AdjustRenderOffsetFromDir(___pawn, compOversizedWeapon);
                    matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
                    Graphics.DrawMesh((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, matSingle, 0);
                    bool flag11 = compOversizedWeapon.Props != null && compOversizedWeapon.Props.isDualWeapon;
                    if (flag11)
                    {
                        vector = new Vector3(-1f * vector.x, vector.y, vector.z);
                        bool flag12 = ___pawn.Rotation == Rot4.North || ___pawn.Rotation == Rot4.South;
                        Mesh mesh;
                        if (flag12)
                        {
                            num += 135f;
                            num %= 360f;
                            mesh = ((!flag4) ? MeshPool.plane10Flip : MeshPool.plane10);
                        }
                        else
                        {
                            vector = new Vector3(vector.x, vector.y - 0.1f, vector.z + 0.15f);
                            mesh = ((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip);
                        }
                        matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
                        Graphics.DrawMesh(mesh, matrix, matSingle, 0);
                    }
                    if (compActivatableEffect!=null)
                    {
                        HarmonyCompActivatableEffect.DrawEquipmentAimingPostFix(___pawn, eq, drawLoc, aimAngle);
                    }
                    return false;
                }
            }
            return true;
        }
        public static bool DrawEquipmentAimingDualWieldPreFix(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Pawn ___pawn = eq.TryGetComp<CompEquippable>().PrimaryVerb.CasterPawn;

            ThingWithComps offHandEquip = null;
            if (___pawn.equipment == null)
            {
                return true;
            }
            if (___pawn.equipment.TryGetOffHandEquipment(out ThingWithComps result))
            {
                offHandEquip = result;
            }
            if (offHandEquip != null)
            {
                return true;
            }
            ThingWithComps thingWithComps = eq as ThingWithComps;
            bool flag = thingWithComps != null;
            if (flag)
            {
                ThingComp thingComp = thingWithComps.AllComps.FirstOrDefault((ThingComp y) => y.GetType().ToString() == "CompDeflector.CompDeflector" || y.GetType().BaseType.ToString() == "CompDeflector.CompDeflector");
                bool flag2 = thingComp != null && Traverse.Create(thingComp).Property("IsAnimatingNow", null).GetValue<bool>();
                if (flag2)
                {
                    return false;
                }
                AdeptusMechanicus.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<AdeptusMechanicus.CompOversizedWeapon>(thingWithComps);
                AdeptusMechanicus.CompActivatableEffect compActivatableEffect = ThingCompUtility.TryGetComp<AdeptusMechanicus.CompActivatableEffect>(thingWithComps);
                bool flag3 = compOversizedWeapon != null;
                if (flag3)
                {
                    bool flag4 = false;
                    float num = aimAngle - 90f;
                    Mesh mesh;
                    if (___pawn == null)
                    {
                        return true;
                    }
                    if (aimAngle > 20f && aimAngle < 160f)
                    {
                        mesh = MeshPool.plane10;
                        num += eq.def.equippedAngleOffset;
                    }
                    else if (aimAngle > 200f && aimAngle < 340f)
                    {
                        mesh = MeshPool.plane10Flip;
                        flag4 = true;
                        num -= 180f;
                        num -= eq.def.equippedAngleOffset;
                    }
                    else
                    {
                        mesh = MeshPool.plane10;
                        num = AdjustOffsetAtPeace(eq, ___pawn, compOversizedWeapon, num);
                    }
                    if (compOversizedWeapon.Props != null && !PawnUtility.IsFighting(___pawn) && compOversizedWeapon.Props.verticalFlipNorth && ___pawn.Rotation == Rot4.North)
                    {
                        num += 180f;
                    }
                    if (!PawnUtility.IsFighting(___pawn) || ___pawn.TargetCurrentlyAimingAt == null)
                    {
                        num = AdjustNonCombatRotation(___pawn, num, compOversizedWeapon);
                    }
                    num %= 360f;
                    Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
                    bool flag10 = graphic_StackCount != null;
                    Material matSingle;
                    if (flag10)
                    {
                        matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
                    }
                    else
                    {
                        matSingle = eq.Graphic.MatSingle;
                    }
                    Vector3 s;
                    if (enabled_AlienRaces)
                    {
                        Vector2 v = AlienRacesPatch(___pawn);
                        s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                    }
                    else
                    {
                        s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                    }
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 vector = HarmonyCompOversizedWeapon.AdjustRenderOffsetFromDir(___pawn, compOversizedWeapon);
                    matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
                    Graphics.DrawMesh((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, matSingle, 0);
                    bool flag11 = compOversizedWeapon.Props != null && compOversizedWeapon.Props.isDualWeapon;
                    if (flag11)
                    {
                        vector = new Vector3(-1f * vector.x, vector.y, vector.z);
                        bool flag12 = ___pawn.Rotation == Rot4.North || ___pawn.Rotation == Rot4.South;
                        if (flag12)
                        {
                            num += 135f;
                            num %= 360f;
                            mesh = ((!flag4) ? MeshPool.plane10Flip : MeshPool.plane10);
                        }
                        else
                        {
                            vector = new Vector3(vector.x, vector.y - 0.1f, vector.z + 0.15f);
                            mesh = ((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip);
                        }
                        matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
                        Graphics.DrawMesh(mesh, matrix, matSingle, 0);
                    }
                    if (compActivatableEffect != null)
                    {
                        HarmonyCompActivatableEffect.DrawEquipmentAimingPostFix(___pawn, eq, drawLoc, aimAngle);
                    }
                    return false;
                }
            }
            return true;
        }
        public static bool DrawEquipmentAimingOverride(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Pawn ___pawn = eq.TryGetComp<CompEquippable>().PrimaryVerb.CasterPawn;
            ThingWithComps thingWithComps = eq as ThingWithComps;
            AdeptusMechanicus.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<AdeptusMechanicus.CompOversizedWeapon>(thingWithComps);
            if (compOversizedWeapon == null)
            {
                return true;
            }
            Log.Message("oversized weapon with Dual Wield active");
            float num = aimAngle - 90f;
            bool flag = false;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            Vector3 s;
            if (enabled_AlienRaces)
            {
                Vector2 v = AlienRacesPatch(___pawn);
                s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
            }
            else
            {
                s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            }
            Material matSingle;
            Matrix4x4 matrix = default(Matrix4x4);
            Vector3 vector = HarmonyCompOversizedWeapon.AdjustRenderOffsetFromDir(___pawn, compOversizedWeapon);
            matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }
            Graphics.DrawMesh((!flag) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, matSingle, 0);
            Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
            return false;
        }

        static Vector2 AlienRacesPatch(Pawn pawn)
        {
            AlienRace.ThingDef_AlienRace alienDef = pawn.def as AlienRace.ThingDef_AlienRace;
            Vector2 s = alienDef.alienRace.generalSettings.alienPartGenerator.customDrawSize;
            return s;
        }

        private static float AdjustOffsetAtPeace(Thing eq, Pawn pawn, CompOversizedWeapon compOversizedWeapon, float num)
        {
            Mesh mesh;
            mesh = MeshPool.plane10;
            var offsetAtPeace = eq.def.equippedAngleOffset;
            if (compOversizedWeapon.Props != null && (!pawn.IsFighting() && compOversizedWeapon.Props.verticalFlipOutsideCombat))
            {
                offsetAtPeace += 180f;
            }
            num += offsetAtPeace;
            return num;
        }

        private static float AdjustNonCombatRotation(Pawn pawn, float num, CompOversizedWeapon compOversizedWeapon)
        {
            if (compOversizedWeapon.Props != null)
            {
                if (pawn.Rotation == Rot4.North)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentNorth;
                }
                else if (pawn.Rotation == Rot4.East)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentEast;
                }
                else if (pawn.Rotation == Rot4.West)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentWest;
                }
                else if (pawn.Rotation == Rot4.South)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentSouth;
                }
            }
            return num;
        }

        private static Vector3 AdjustRenderOffsetFromDir(Pawn pawn, CompOversizedWeapon compOversizedWeapon)
        {
            var curDir = pawn.Rotation;
         
            Vector3 curOffset = Vector3.zero;
         
            if (compOversizedWeapon.Props != null)
            {
         
                curOffset = compOversizedWeapon.Props.northOffset;
                if (curDir == Rot4.East)
                {
                    curOffset = compOversizedWeapon.Props.eastOffset;
                }
                else if (curDir == Rot4.South)
                {
                    curOffset = compOversizedWeapon.Props.southOffset;
                }
                else if (curDir == Rot4.West)
                {
                    curOffset = compOversizedWeapon.Props.westOffset;
                }
            }
         
            return curOffset;
        }

        public static void get_DefaultGraphic_PostFix(Thing __instance, Graphic ___graphicInt, ref Graphic __result)
        {
            if (___graphicInt == null) return;
            if (__instance.ParentHolder is Pawn) return;

            var compOversizedWeapon = __instance.TryGetComp<CompOversizedWeapon>();
            if (compOversizedWeapon != null)
            {
                //Following commented-out section is an unnecessary "optimization" that actually hurts performance due to the reflection involved.
                //var activatableEffect =
                //    thingWithComps.AllComps.FirstOrDefault(
                //        y => y.GetType().ToString().Contains("ActivatableEffect"));
                //if (activatableEffect != null)
                //{
                //    var getPawn = Traverse.Create(activatableEffect).Property("GetPawn").GetValue<Pawn>();
                //    if (getPawn != null)
                //        return;
                //}
                if (compOversizedWeapon.Props?.groundGraphic == null)
                {
                    ___graphicInt.drawSize = __instance.def.graphicData.drawSize;
                    __result = ___graphicInt;
                }
                else // compOversizedWeapon.Props.groundGraphic != null
                {
                    if (compOversizedWeapon.IsEquipped)
                    {
                        ___graphicInt.drawSize = __instance.def.graphicData.drawSize;
                        __result = ___graphicInt;
                    }
                    else
                    {
                        if (compOversizedWeapon.Props.groundGraphic.GraphicColoredFor(__instance) is Graphic
                            newResult)
                        {
                            newResult.drawSize = compOversizedWeapon.Props.groundGraphic.drawSize;
                            __result = newResult;
                        }
                        else
                        {
                            ___graphicInt.drawSize = __instance.def.graphicData.drawSize;
                            __result = ___graphicInt;
                        }
                    }
                }
            }
        }
    }
}