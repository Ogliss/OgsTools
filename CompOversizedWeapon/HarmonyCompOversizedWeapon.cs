using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace OgsCompOversizedWeapon
{
    [StaticConstructorOnStartup]
    public static class HarmonyCompOversizedWeapon
    {
        public static bool enabled_AlienRaces;
        public static bool enabled_rooloDualWield;
        public static bool enabled_CombatExtended;
        static HarmonyCompOversizedWeapon()
        {
            enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
            enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces"); 
            enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "Roolo.DualWield");
            var harmony = new Harmony("rimworld.Ogliss.comps.oversized");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (enabled_rooloDualWield)
            {
            //    Log.Message("OgsCompOversizedWeapon Dual Wield Compatability mode");
                DualWieldPatch(harmony);
            }
            else
            {
            //    Log.Message("Dual Wield NOT detected");
                /*
                harmony.Patch(typeof(PawnRenderer).GetMethod("DrawEquipmentAiming"),
                    new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(DrawEquipmentAimingPreFix)), null);
                */
            }
            if (enabled_CombatExtended)
            {
            //    CEPatch(harmony);
            }
            harmony.Patch(AccessTools.Method(typeof(Thing), "get_DefaultGraphic"), null,
                new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(get_DefaultGraphic_PostFix)));
        }

        public static void CEPatch(Harmony harmony)
        {
            MethodInfo method = AccessTools.TypeByName("CombatExtended.HarmonyCE.Harmony_PawnRenderer_DrawEquipmentAiming").GetMethod("DrawMeshModified");
            MethodInfo patch = typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler).GetMethod("DrawMeshModifiedCE");

            if (patch == null)
            {
                Log.Error("patch Prefix is null", false);
            }
            if (method == null)
            {
                Log.Error("method is null", false);
            }
            if (harmony.Patch(method, new HarmonyMethod(patch)) == null)
            {
                Log.Error("PawnRenderer_DrawEquipmentAiming_Transpiler_Patch_CE failed.", false);
            }
            else
            {
                Log.Message("PawnRenderer_DrawEquipmentAiming_Transpiler_Patch_CE patched", false);
            }
        }

        public static void DualWieldPatch(Harmony harmony)
        {
            harmony.Patch(typeof(DualWield.Harmony.PawnRenderer_DrawEquipmentAiming).GetMethod("DrawEquipmentAimingOverride"),
                new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(DrawEquipmentAimingOverride)), 
                null
                /*
                ,
                new HarmonyMethod(typeof(Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler), nameof(Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler.Transpiler))
                */
                );
            /*
            harmony.Patch(typeof(PawnRenderer).GetMethod("DrawEquipmentAiming"),
                new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(DrawEquipmentAimingDualWieldPreFix)), null);
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

                OgsCompOversizedWeapon.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<OgsCompOversizedWeapon.CompOversizedWeapon>(thingWithComps);
            //    OgsCompActivatableEffect.CompActivatableEffect compActivatableEffect = ThingCompUtility.TryGetComp<OgsCompActivatableEffect.CompActivatableEffect>(thingWithComps);
                bool flag3 = compOversizedWeapon != null;
                if (flag3)
                {
                    if (compOversizedWeapon.CompDeflectorIsAnimatingNow) return false;
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
                    if (enabled_AlienRaces && ___pawn.RaceProps.Humanlike)
                    {
                        Vector2 v = AlienRacesPatch(___pawn);
                        s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                    }
                    else
                    {
                        Vector2 v = ___pawn.Graphic.data.drawSize;
                        //    s = new Vector3(eq.def.graphicData.drawSize.x + v.x, 1f, eq.def.graphicData.drawSize.y + v.y);
                    //    s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                        s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                    }
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 vector = HarmonyCompOversizedWeapon.AdjustRenderOffsetFromDir(___pawn, compOversizedWeapon);
                    matrix.SetTRS(drawLoc + vector, Quaternion.AngleAxis(num, Vector3.up), s);
                //    Log.Message("remderomg "+eq.LabelShortCap +" at "+s);
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
                    /*
                    if (compActivatableEffect!=null)
                    {
                        OgsCompActivatableEffect.HarmonyCompActivatableEffect.DrawEquipmentAimingPostFix(___pawn, eq, drawLoc, aimAngle);
                    }
                    */
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
                OgsCompOversizedWeapon.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<OgsCompOversizedWeapon.CompOversizedWeapon>(thingWithComps);
                bool flag3 = compOversizedWeapon != null;
                if (flag3)
                {
                    if (compOversizedWeapon.CompDeflectorIsAnimatingNow) return false;
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
                    if (enabled_AlienRaces && ___pawn.RaceProps.Humanlike)
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
                //    Log.Message("remderomg " + eq.LabelShortCap + " at " + s);
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
                    bool compActivatableEffect = thingWithComps.AllComps.Any(x=> x.GetType().Name.Contains("OgsCompActivatableEffect.HarmonyCompActivatableEffect"));
                    if (compActivatableEffect)
                    {
                        //return true;
                    //    GenTypes.GetTypeInAnyAssembly("OgsCompActivatableEffect.HarmonyCompActivatableEffect", "OgsCompActivatableEffect").GetMethod("DrawEquipmentAimingPostFix").Invoke(null, new Type[] { typeof(Pawn), typeof(Thing), typeof(Vector3), typeof(float) });
                        //    HarmonyCompActivatableEffect.DrawEquipmentAimingPostFix(___pawn, eq, drawLoc, aimAngle);
                    }
                    
                    return false;
                }
            }
            return true;
        }
        public static bool DrawEquipmentAimingOverrideOld(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Pawn ___pawn = eq.TryGetComp<CompEquippable>().PrimaryVerb.CasterPawn;
            ThingWithComps thingWithComps = eq as ThingWithComps;
            OgsCompOversizedWeapon.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<OgsCompOversizedWeapon.CompOversizedWeapon>(thingWithComps);
            if (compOversizedWeapon == null)
            {
                return true;
            }
            float num = aimAngle - 90f;
            bool flag = false;
        //    Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            Vector3 s;
            if (enabled_AlienRaces && ___pawn.RaceProps.Humanlike)
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
        //    Log.Message("remderomg " + eq.LabelShortCap + " at " + s);
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }
            if (___pawn.equipment.COWTryGetOffHandEquipment(out ThingWithComps thingy))
            {

                if (thingy == eq)
                {
                    flag = !flag;
                    if (___pawn.Rotation == Rot4.West)
                    {
                        flag = !flag;

                    }
                    
                    if (___pawn.Rotation == Rot4.North)
                    {
                        flag = !flag;
                        if (___pawn.TargetCurrentlyAimingAt != null)
                        {
                            flag = !flag;

                        }
                    }
                }
            }
            else
            {
                if (___pawn.Rotation == Rot4.East)
                {
                    flag = !flag;

                }
                
                if (___pawn.Rotation == Rot4.North)
                {
                    flag = !flag;
                }
                
            }

            Graphics.DrawMesh((flag) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, matSingle, 0);
        //    Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
            return false;
        }
        
        public static bool DrawEquipmentAimingOverride(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Pawn ___pawn = eq.TryGetComp<CompEquippable>().PrimaryVerb.CasterPawn;
            ThingWithComps thingWithComps = eq as ThingWithComps;
            OgsCompOversizedWeapon.CompOversizedWeapon compOversizedWeapon = ThingCompUtility.TryGetComp<OgsCompOversizedWeapon.CompOversizedWeapon>(thingWithComps);
            if (compOversizedWeapon == null)
            {
                return true;
            }
            float num = aimAngle - 90f;
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
            if (enabled_AlienRaces && ___pawn.RaceProps.Humanlike)
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
            Graphics.DrawMesh(mesh, matrix, matSingle, 0);
        //    Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
            return false;
        }

        public static Vector2 AlienRacesPatch(Pawn pawn)
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
            //    Log.Message("am i doing anything? :" + __instance);
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