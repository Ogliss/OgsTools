using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using Verse.Sound;
using System.Reflection.Emit;
using UnityEngine;
using System.Reflection;

namespace OgsCompOversizedWeapon
{
	//    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
	// Token: 0x020000FB RID: 251
//	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAimingOverride")]
	internal static class Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler
    {
		// Token: 0x060004A1 RID: 1185 RVA: 0x0002500C File Offset: 0x0002320C
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            /*
            for (int i = 0; i < list.Count; i++)
            {
                if (list[index: i].OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32) })))
                {
                //    Log.Message("CodeInstruction " + i + " of " + list.Count + " " + list[i].operand);
                }
            }
            */
            /*
            list[list.Count - 3].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler), "DrawMeshModified", null, null);
			
            list.InsertRange(list.Count - 3, new CodeInstruction[]
			{
				new CodeInstruction(OpCodes.Ldarg_0, null),
			//	new CodeInstruction(OpCodes.Ldloc_0, null)
            });
            */

            foreach (CodeInstruction item in list)
            {

                if (item.OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32) })))
                {
                    item.operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler), "DrawMeshModified", null, null);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_1);
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_0);
                }
            //    Log.Message("CodeInstruction " + list.IndexOf(item) +" of " + list.Count + " " + item.operand + " " + item);
                yield return item;
            }
            /*
            list = instructions.ToList<CodeInstruction>();
            for (int i = 0; i < instructions.Count(); i++)
            {
            //    Log.Message("CodeInstruction " + i + " of " + list.Count + " " + list[i].operand + " " + list[i]);
            }
            */
            yield break;
		}

        // Token: 0x060004A0 RID: 1184 RVA: 0x00024F4C File Offset: 0x0002314C
        private static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq)
        {
            
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

    }
}
