using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using Verse.Sound;
using System.Reflection.Emit;
using UnityEngine;
using System.Reflection;
using AdvancedGraphics.HarmonyInstance;

namespace AdvancedGraphics
{

    [HarmonyPatch(typeof(PawnRenderUtility), "DrawEquipmentAiming"), HarmonyPriority(Priority.First)]
    public static class PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler
    {
        public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
        public static bool enabled_YayosCombat = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "com.yayo.combat3");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            ConstructorInfo vec3Ctor = AccessTools.Constructor(type: typeof(Vector3), new Type[] { typeof(float), typeof(float), typeof(float) });
            MethodInfo matrixTRS = AccessTools.Method(type: typeof(Matrix4x4), name: nameof(Matrix4x4.TRS));
            MethodInfo graphicsDrawMesh = AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int) });
            FieldInfo pawnRendererPawn = AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn));
            FieldInfo meshflipped = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.plane10Flip));
            FieldInfo offhand = AccessTools.Field(typeof(PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler), nameof(PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler.offhand));
            MethodInfo offhandEq = AccessTools.Method(typeof(PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler), nameof(PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler.DrawOffhandEquipmentAiming));
            MethodInfo label = AccessTools.Method(typeof(PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler), nameof(PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler.label));
            FieldInfo equippedAngleOffset = AccessTools.Field(typeof(ThingDef), nameof(ThingDef.equippedAngleOffset));
            FieldInfo thingDef = AccessTools.Field(typeof(Thing), nameof(Thing.def));
            FieldInfo graphicData = AccessTools.Field(typeof(ThingDef), nameof(ThingDef.graphicData));
            FieldInfo isDualWeapon = AccessTools.Field(typeof(GraphicData_Equippable), nameof(GraphicData_Equippable.isDualWeapon));

            MethodInfo GetNewMatrix = AccessTools.Method(typeof(PawnRenderUtility_DrawEquipmentAiming_Vanilla_Transpiler), nameof(GetMatrix), null, null);


            Label postOffhand = il.DefineLabel();

            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                // weapon size tweak
                if (instruction.OperandIs(matrixTRS))
                {
                //    if (Prefs.DevMode) Log.Message($"{i}  opcode: {instruction.opcode} operand: {instruction.operand} Weapon Matrix tweak");
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    instruction = new CodeInstruction(OpCodes.Call, GetNewMatrix);
                }
                
                if (instruction.OperandIs(graphicsDrawMesh))
                {
                    yield return instruction;
                    // need to wrap these in an if block
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, thingDef);
                    yield return new CodeInstruction(OpCodes.Ldfld, graphicData);
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(GraphicData_Equippable));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, postOffhand);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, thingDef);
                    yield return new CodeInstruction(OpCodes.Ldfld, graphicData);
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(GraphicData_Equippable));
                    yield return new CodeInstruction(OpCodes.Ldfld, isDualWeapon);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, postOffhand);
                    // modify values for second weapon
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, offhandEq);
                    // draw second weapon
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Call, graphicsDrawMesh);
                    instruction = new CodeInstruction(OpCodes.Call, label).WithLabels(postOffhand);
                }
                
                yield return instruction;
            }
        }

        public static void label()
        {

        }

        public static Mesh GetMesh(Mesh mesh, Thing eq)
        {
			if (eq.def.graphicData is GraphicData_Equippable equippable && equippable.isDualWeapon)
            {
                if (offhand)
                {
                    mesh = mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
                    Log.Message($"render offhand for {eq} base mesh {mesh}");
                }
                else
                {
                    Log.Message($"render mainhand for {eq} base mesh {mesh}");
                }
            }
            return mesh;

        }
        public static Matrix4x4 GetMatrix(Vector3 pos, Quaternion q, Vector3 size, CompEquippable instance, Thing eq, float num, float aimAngle)
        {
            Pawn pawn = instance.Holder;
			if (pawn == null)
            {
            //   Log.Message("pawn is null");
                return Matrix4x4.TRS(pos,q,size);
            }
            float angle = num;
            if (eq.def.graphicData != null && eq.def.graphicData is GraphicData_Equippable equippable)
            {
                if (equippable.isDualWeapon)
                {
                    Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
                    bool aiming = (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid);
                //    aimAngle = aimAngle + equippable.OffsetAngleFor(instance.pawn.Rotation, true);
                    if (aiming)
                    {
                        pos += equippable.OffsetPosFor(pawn.Rotation, false).RotatedBy(aimAngle);
                    }
                    else
                    {
                        pos += equippable.OffsetPosFor(pawn.Rotation, false);
                    }
                }
                float am = pawn.Rotation != Rot4.North ? equippable.OffsetAngleFor(pawn.Rotation) : -equippable.OffsetAngleFor(pawn.Rotation);
            //    Log.Message($"check for angle modification {am}");
                angle += am;
                angle %= 360f;
            }
            Vector3 s;
            if (pawn.RaceProps.Humanlike)
            {
                if (Main.enabled_AlienRaces)
                {
                    Vector2 v = AlienRaceUtility.AlienRacesPatch(pawn, eq);
                    float f = Mathf.Max(v.x, v.y);
                    s = new Vector3(size.x * f, size.y, size.z * f);
                }
                else
                {
                    s = size;
                }
            }
            else
            {
                Vector2 v = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                s = new Vector3(size.x + v.x / 10, size.y, size.z + v.y / 10);
            }
        //    Log.Message($"Matrix4x4 MH is {pos} aimAngle({aimAngle}) AngleAxis({num},{angle}) {s}");
            return Matrix4x4.TRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);
        }

        public static void DrawOffhandEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle, CompEquippable compEquippable, 
            ref Mesh mesh, ref Matrix4x4 matrix, float num, Vector3 s)
        {
            Pawn pawn = compEquippable.Holder;
            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
            bool aiming = (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid);
            GraphicData_Equippable equippable = eq?.def.graphicData as GraphicData_Equippable;
            
            num = (pawn.Rotation == Rot4.South || pawn.Rotation == Rot4.North ? -aimAngle : aimAngle) + (pawn.Rotation != Rot4.East ? 90f : -90f);
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh =  mesh == MeshPool.plane10 && pawn.Rotation == Rot4.South || pawn.Rotation == Rot4.North ? MeshPool.plane10Flip : MeshPool.plane10;
                num -= eq.def.equippedAngleOffset + equippable.OffsetAngleFor(pawn.Rotation, true);
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
            //    mesh = mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
            //    num += 180f;
                num -= eq.def.equippedAngleOffset + equippable.OffsetAngleFor(pawn.Rotation, true);
            }
            else
            {
                if (aiming) mesh = mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
                num += eq.def.equippedAngleOffset + equippable.OffsetAngleFor(pawn.Rotation, true);
            }
            num %= 360f;

            if (compEquippable != null)
            {
                Vector3 b;
                float num2;
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out b, out num2, aimAngle);
                drawLoc += b;
                num += num2;
            }
            /*
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material material;
            if (graphic_StackCount != null)
            {
                material = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq);
            }
            else
            {
                material = eq.Graphic.MatSingleFor(eq);
            }
            */
            float angle = num;
            float am =  equippable.OffsetAngleFor(pawn.Rotation, true);

            if (aiming)
            {
                drawLoc += equippable.OffsetPosFor(pawn.Rotation, true).RotatedBy(aimAngle);
            }
            else
            {
                drawLoc += equippable.OffsetPosFor(pawn.Rotation, true);
            }

            
        //    Log.Message($"check for angleOffhand modification {am}");
            angle += am;
            angle %= 360f;
        //    Log.Message($"Matrix4x4 OH is {drawLoc} aimAngle({aimAngle}) AngleAxis({num},{angle}) {s}");
            num = angle;
            matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(angle, Vector3.up), s);
        //    Graphics.DrawMesh(mesh, matrix, material, 0);
        }


        public static bool offhand;

	}
}
