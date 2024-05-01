using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace AdvancedGraphics
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
	public static class PawnRenderer_DrawEquipment_Vanilla_Transpiler
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            ConstructorInfo vec3Ctor = AccessTools.Constructor(typeof(Vector3), new Type[] { typeof(float), typeof(float), typeof(float) });
            FieldInfo pawnRendererPawn = AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn));
            FieldInfo pawnEquipment = AccessTools.Field(typeof(Pawn), nameof(Pawn.equipment));
            MethodInfo drawEquipmentAiming = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming));
            MethodInfo graphicsDrawMesh = AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int) });
            MethodInfo vec3RotatedBy = AccessTools.Method(type: typeof(Vector3Utility), name: nameof(Vector3Utility.RotatedBy), new Type[] { typeof(Vector3), typeof(float) });
            MethodInfo equipmentPrimary = AccessTools.PropertyGetter(type: typeof(Pawn_EquipmentTracker), name: nameof(Pawn_EquipmentTracker.Primary));
            int vec3CtorCount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                /*
                if (instruction.OperandIs(vec3Ctor))
                {
                    vec3CtorCount++;
                    if (vec3CtorCount > 1)
                    {
                        Log.Message($"{i}  opcode: {instruction.opcode} operand: {instruction.operand}");
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipment_Vanilla_Transpiler), "Offset2", null, null));
                        if (vec3RotatedBy != null && i + 2 < list.Count-2 && list[i+2].OperandIs(vec3RotatedBy))
                        {
                            Log.Message($"{i}  Aiming!!");
                        }
                    }
                }
                */
                /*
                if (instruction.OperandIs(drawEquipmentAiming))
                {
                    yield return instruction;
                    yield return new CodeInstruction(list[i - 6].opcode);
                    yield return new CodeInstruction(list[i - 5].opcode, list[i - 5].operand);
                    yield return new CodeInstruction(list[i - 4].opcode, list[i - 4].operand);
                    yield return new CodeInstruction(list[i - 3].opcode, list[i - 3].operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(list[i - 1].opcode, list[i - 1].operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipment_Vanilla_Transpiler), "DrawEquipmentAiming", null, null));
                }
                */
                /*
				// modifies base position of rendered weapons before rotation
				if (instruction.opcode == OpCodes.Stloc_0)
                {

                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldloca_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipment_Vanilla_Transpiler), "Offset", null, null));

                }
				*/
                yield return instruction;
            }
        } 
        public static Vector3 Offset2(Vector3 vector, PawnRenderer instance)
        {
			Thing eq = instance.pawn.equipment.Primary;
            if (eq?.def.graphicData != null && eq?.def.graphicData is GraphicData_Equippable equippable)
            {
                return vector + equippable.OffsetPosFor(instance.pawn.Rotation);//.RotatedBy(aimAngle);
            }
            return vector;
        }
        
        public static void DrawEquipmentAiming(Thing eq, Vector3 rootLoc, float aimAngle, PawnRenderer instance, PawnRenderFlags flags)
        {
            if (eq?.def.graphicData != null && eq?.def.graphicData is GraphicData_Equippable equippable)
            {
                if (equippable.isDualWeapon)
                {
                    Vector3 vector = new Vector3(0f, (instance.pawn.Rotation == Rot4.North) ? -0.0028957527f : 0.03474903f, 0f);

                    Stance_Busy stance_Busy = instance.pawn.stances.curStance as Stance_Busy;
                    bool aiming = (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid && (flags & PawnRenderFlags.NeverAimWeapon) == PawnRenderFlags.None);

                    float equipmentDrawDistanceFactor = instance.pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
                    float offHandAngle = aimAngle + equippable.OffsetAngleFor(instance.pawn.Rotation, true);
                    if (aiming)
                    {
                        vector += rootLoc + (new Vector3(0f, 0f, 0.4f + instance.pawn.equipment.Primary.def.equippedDistanceOffset) + equippable.OffsetPosFor(instance.pawn.Rotation, true)).RotatedBy(aimAngle) * equipmentDrawDistanceFactor;
                    }
                    else
                    {
                        if (instance.pawn.Rotation == Rot4.South)
                        {
                            vector += rootLoc + new Vector3(0f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                        }
                        if (instance.pawn.Rotation == Rot4.North)
                        {
                            vector += rootLoc + new Vector3(0f, 0f, -0.11f) * equipmentDrawDistanceFactor;
                        }
                        if (instance.pawn.Rotation == Rot4.East)
                        {
                            vector += rootLoc + new Vector3(0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                        }
                        if (instance.pawn.Rotation == Rot4.West)
                        {
                            vector += rootLoc + new Vector3(-0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                        }
                        vector += equippable.OffsetPosFor(instance.pawn.Rotation, true);
                    }
                    instance.DrawEquipmentAiming(eq, vector, aimAngle + offHandAngle);
                }

            }
        }

        public static Vector3 Offset(Vector3 vector, PawnRenderer instance, ref float aimAngle)
        {
			Thing eq = instance.pawn.equipment.Primary;
            if (eq?.def.graphicData != null && eq?.def.graphicData is GraphicData_Equippable equippable)
            {
                Vector3 offsetMainHand = default(Vector3);
                Vector3 offsetOffHand = default(Vector3);
                Stance_Busy stance_Busy = instance.pawn.stances.curStance as Stance_Busy;
                bool Aiming = OversizedUtil.CurrentlyAiming(stance_Busy);
                if (equippable.isDualWeapon)
				{
                    float offHandAngle = aimAngle + equippable.OffsetAngleFor(instance.pawn.Rotation, true);
                    Vector3 vector2 = vector + equippable.OffsetPosFor(instance.pawn.Rotation, true).RotatedBy(offHandAngle);
                    instance.DrawEquipmentAiming(eq, vector2, offHandAngle);
                }
                aimAngle += equippable.OffsetAngleFor(instance.pawn.Rotation);
                return vector + equippable.OffsetPosFor(instance.pawn.Rotation);//.RotatedBy(aimAngle);
            }
            return vector;
        }


        public static void SetAnglesAndOffsets(GraphicData_Equippable Preps, Thing thing, ref Vector3 offsetMainHand, ref Vector3 offsetOffHand, ref float mainHandAngle, ref float offHandAngle, bool mainHandAiming, bool offHandAiming)
        {
            Pawn pawn = thing as Pawn;
            bool Melee = pawn != null;
            if (Melee)
            {
                Melee = OversizedUtil.IsMeleeWeapon(pawn.equipment.Primary);
            }

            bool Dual = false;
            if (Preps != null)
            {
                Dual = Preps.isDualWeapon;
            }
            float num = Preps.meleeMirrored ? (360f - OversizedUtil.meleeAngle) : OversizedUtil.meleeAngle;
            float num2 = Preps.rangedMirrored ? (360f - OversizedUtil.rangedAngle) : OversizedUtil.rangedAngle;
            offsetMainHand = Preps.OffsetPosFor(thing.Rotation);
            offsetOffHand = Preps.OffsetPosFor(thing.Rotation, offHandAiming);
            if (Preps != null)
            {
                mainHandAngle += Preps.OffsetAngleFor(thing.Rotation);
                offHandAngle += Preps.OffsetAngleFor(thing.Rotation, offHandAiming);
            }
            if (thing.Rotation == Rot4.East)
            {
                offsetOffHand.y = -1f;
                offsetOffHand.z = 0.1f;
            }
            else
            {
                if (thing.Rotation == Rot4.West)
                {
                    if (Dual) offsetMainHand.y = -1f;
                    offsetOffHand.z = -0.1f;
                }
                else
                {
                    if (thing.Rotation == Rot4.North)
                    {
                        if (!mainHandAiming)
                        {
                            offsetMainHand.x += (Dual ? (Melee ? OversizedUtil.meleeXOffset : OversizedUtil.rangedXOffset) : 0);
                            offsetOffHand.x += -(Melee ? -OversizedUtil.meleeXOffset : -OversizedUtil.rangedXOffset);
                            offsetMainHand.z += (Dual ? (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset) : 0);
                            offsetOffHand.z += (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset);
                            if (Preps != null)
                            {
                                offHandAngle += (Melee ? OversizedUtil.meleeAngle : OversizedUtil.rangedAngle);
                                mainHandAngle += -(Melee ? num : num2);
                            }
                        }
                        else
                        {
                            offsetOffHand.x = -0.1f;
                        }
                    }
                    else
                    {
                        if (!mainHandAiming)
                        {
                            offsetMainHand.y = 1f;
                            offsetMainHand.x += -(Dual ? (Melee ? -OversizedUtil.meleeXOffset : -OversizedUtil.rangedXOffset) : 0);
                            offsetOffHand.x += (Melee ? OversizedUtil.meleeXOffset : OversizedUtil.rangedXOffset);
                            offsetMainHand.z += (Dual ? (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset) : 0);
                            offsetOffHand.z += (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset);
                            if (Preps != null)
                            {
                                offHandAngle += -(Melee ? num : num2);
                                mainHandAngle += (Melee ? OversizedUtil.meleeAngle : OversizedUtil.rangedAngle);
                            }
                        }
                        else
                        {
                            offsetOffHand.y = 1f;
                            offHandAngle += (!Melee ? num : num2);
                            offsetOffHand.x = 0.1f;
                        }
                    }
                }
            }
        }

    }
}
