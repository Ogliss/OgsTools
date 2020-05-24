using System;
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
                new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(get_Graphic_PostFix)));
        }


        /// <summary>
        ///     Adds another "layer" to the equipment aiming if they have a
        ///     weapon with a CompActivatableEffect.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="eq"></param>
        /// <param name="drawLoc"></param>
        /// <param name="aimAngle"></param>
        public static bool DrawEquipmentAimingPreFix(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
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
                    Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
                    if (value == null)
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
                        num = AdjustOffsetAtPeace(eq, value, compOversizedWeapon, num);
                    }
                    if (compOversizedWeapon.Props != null && !PawnUtility.IsFighting(value) && compOversizedWeapon.Props.verticalFlipNorth && value.Rotation == Rot4.North)
                    {
                        num += 180f;
                    }
                    if (!PawnUtility.IsFighting(value) || value.TargetCurrentlyAimingAt == null)
                    {
                        num = AdjustNonCombatRotation(value, num, compOversizedWeapon);
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
                        Vector2 v = AlienRacesPatch(value);
                        s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                    }
                    else
                    {
                        s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                    }
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 vector = HarmonyCompOversizedWeapon.AdjustRenderOffsetFromDir(value, compOversizedWeapon);
                    matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
                    Graphics.DrawMesh((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, matSingle, 0);
                    bool flag11 = compOversizedWeapon.Props != null && compOversizedWeapon.Props.isDualWeapon;
                    if (flag11)
                    {
                        vector = new Vector3(-1f * vector.x, vector.y, vector.z);
                        bool flag12 = value.Rotation == Rot4.North || value.Rotation == Rot4.South;
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
                        HarmonyCompActivatableEffect.DrawEquipmentAimingPostFix(__instance, eq, drawLoc, aimAngle);
                    }
                    return false;
                }
            }
            return true;
        }
        public static bool DrawEquipmentAimingDualWieldPreFix(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            ThingWithComps offHandEquip = null;
            Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (value.equipment == null)
            {
                return true;
            }
            if (value.equipment.TryGetOffHandEquipment(out ThingWithComps result))
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
                    if (value == null)
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
                        num = AdjustOffsetAtPeace(eq, value, compOversizedWeapon, num);
                    }
                    if (compOversizedWeapon.Props != null && !PawnUtility.IsFighting(value) && compOversizedWeapon.Props.verticalFlipNorth && value.Rotation == Rot4.North)
                    {
                        num += 180f;
                    }
                    if (!PawnUtility.IsFighting(value) || value.TargetCurrentlyAimingAt == null)
                    {
                        num = AdjustNonCombatRotation(value, num, compOversizedWeapon);
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
                        Vector2 v = AlienRacesPatch(value);
                        s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                    }
                    else
                    {
                        s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                    }
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 vector = HarmonyCompOversizedWeapon.AdjustRenderOffsetFromDir(value, compOversizedWeapon);
                    matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
                    Graphics.DrawMesh((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, matSingle, 0);
                    bool flag11 = compOversizedWeapon.Props != null && compOversizedWeapon.Props.isDualWeapon;
                    if (flag11)
                    {
                        vector = new Vector3(-1f * vector.x, vector.y, vector.z);
                        bool flag12 = value.Rotation == Rot4.North || value.Rotation == Rot4.South;
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
                        HarmonyCompActivatableEffect.DrawEquipmentAimingPostFix(__instance, eq, drawLoc, aimAngle);
                    }
                    return false;
                }
            }
            return true;
        }
        public static bool DrawEquipmentAimingOverride(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
        {
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
            Pawn value = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
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
                Vector2 v = AlienRacesPatch(value);
                s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
            }
            else
            {
                s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            }
            Material matSingle;
            Matrix4x4 matrix = default(Matrix4x4);
            Vector3 vector = HarmonyCompOversizedWeapon.AdjustRenderOffsetFromDir(value, compOversizedWeapon);
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

        public static void get_Graphic_PostFix(Thing __instance, ref Graphic __result)
        {
            ThingWithComps val = __instance as ThingWithComps;
            if (val == null)
            {
                return;
            }
            AdeptusMechanicus.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<AdeptusMechanicus.CompOversizedWeapon>((Thing)(object)val);
            if (compOversizedWeapon == null)
            {
                return;
            }
            Graphic value = Traverse.Create((object)__instance).Field("graphicInt").GetValue<Graphic>();
            if (value == null || ((Thing)val).ParentHolder is Pawn)
            {
                return;
            }
            ThingComp val2 = val.AllComps.FirstOrDefault((ThingComp y) => ((object)y).GetType().ToString().Contains("ActivatableEffect"));
            if (val2 != null && Traverse.Create((object)val2).Property("GetPawn", (object[])null).GetValue<Pawn>() != null)
            {
                return;
            }
            if (compOversizedWeapon.Props?.groundGraphic == null)
            {
                value.drawSize = __instance.def.graphicData.drawSize;
                __result = value;
                return;
            }
            if (compOversizedWeapon.IsEquipped)
            {
                value.drawSize = __instance.def.graphicData.drawSize;
                __result = value;
                return;
            }
            CompProperties_OversizedWeapon props = compOversizedWeapon.Props;
            Graphic val3;
            if (props == null)
            {
                val3 = null;
            }
            else
            {
                GraphicData groundGraphic = props.groundGraphic;
                val3 = ((groundGraphic != null) ? groundGraphic.GraphicColoredFor(__instance) : null);
            }
            Graphic val4 = val3;
            if (val4 != null)
            {
                val4.drawSize = compOversizedWeapon.Props.groundGraphic.drawSize;
                __result = val4;
            }
            else
            {
                value.drawSize = __instance.def.graphicData.drawSize;
                __result = value;
            }
        }
    }
}