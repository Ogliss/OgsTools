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

namespace OgsCompActivatableEffect
{
    // Token: 0x020000FB RID: 251
    //	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    internal static class Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler
	{
		// Token: 0x060004A1 RID: 1185 RVA: 0x0002500C File Offset: 0x0002320C
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
		//	Log.Message("Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler");
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();



			List<CodeInstruction> instructionList = instructions.ToList();
			List<CodeInstruction> newInstructionList = instructions.ToList();

			for (int i = 0; i < instructionList.Count; i++)
			{
				CodeInstruction instruction = instructionList[index: i];
				if (i > 1 && instructionList[index: i - 1].OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32) })))
				{
					List<CodeInstruction> newlist = new List<CodeInstruction>();
					Log.Message("Draw Vector At: " + (i - 1) + " " + instructionList[index: i - 1].opcode + " " + instructionList[index: i - 1].operand);
					for (int i2 = 0; i2 < 19; i2++)
					{
						int cur = 2 + i2;
						newlist.Add(instructionList[index: i - cur]);
						Log.Message(": " + (i - cur) + " " + instructionList[index: i - cur].opcode + " " + instructionList[index: i - cur].operand);
					}
					newlist.Reverse();
					newlist.Add(new CodeInstruction(opcode: OpCodes.Ldarg_1));
					newlist.Add(new CodeInstruction(opcode: OpCodes.Ldarg_3));
					newlist.Add(new CodeInstruction(opcode: OpCodes.Call, operand: typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler).GetMethod("DrawMeshModified")));
					newlist.Add(new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 7));

					newInstructionList.InsertRange(i, newlist);
					/*
					yield return instruction;
					yield return new CodeInstruction(opcode: instructionList[(i - 8)].opcode);
					yield return new CodeInstruction(opcode: instructionList[(i - 7)].opcode);
					yield return new CodeInstruction(opcode: instructionList[(i - 6)].opcode);
					yield return new CodeInstruction(opcode: instructionList[(i - 5)].opcode);
					yield return new CodeInstruction(opcode: instructionList[(i - 4)].opcode);
					yield return new CodeInstruction(opcode: instructionList[(i - 3)].opcode);
					yield return new CodeInstruction(opcode: instructionList[(i - 2)].opcode);
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_1);
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_3);
					yield return new CodeInstruction(opcode: OpCodes.Call, operand: typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler).GetMethod("DrawMeshModified"));

					instruction = new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 7);
					*/


				}
				if (i > 1 && instructionList[index: i - 1].OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(Int32) })))
				{
					Log.Message("Draw Matrix At: " + (i - 1) + " " + instructionList[index: i - 1].opcode + " " + instructionList[index: i - 1].operand);
					/*
					yield return instruction;
					yield return new CodeInstruction(opcode: OpCodes.Ldloc_0);
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_1);
					yield return new CodeInstruction(opcode: OpCodes.Ldarg_3);
					yield return new CodeInstruction(opcode: OpCodes.Call, operand: typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler).GetMethod("DrawMeshModified"));

					instruction = new CodeInstruction(opcode: OpCodes.Ldarg_S, operand: 7);
					*/
				}

			//	yield return instruction;
			}
			/*
			list[list.Count - 2].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler), "DrawMeshModified", null, null);
			list.InsertRange(list.Count - 2, new CodeInstruction[]
			{
				new CodeInstruction(OpCodes.Ldarg_1, null),
				new CodeInstruction(OpCodes.Ldarg_3, null)
			});

			*/
			return newInstructionList;
		}

		private static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
		{
			ThingWithComps thingWithComps = eq as ThingWithComps;
			CompEquippable equippable = eq.TryGetComp<CompEquippable>();
			Pawn pawn = equippable.PrimaryVerb.CasterPawn;
			OgsCompOversizedWeapon.CompOversizedWeapon compOversized = thingWithComps.TryGetComp<OgsCompOversizedWeapon.CompOversizedWeapon>();
			var compActivatableEffect = thingWithComps?.GetComp<OgsCompActivatableEffect.CompActivatableEffect>();
			if (compActivatableEffect?.Graphic == null) return;

			var matSingle = compActivatableEffect.Graphic.MatSingle;
			//if (mesh == null) mesh = MeshPool.GridPlane(thingWithComps.def.graphicData.drawSize);
			Vector3 s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
			if (compOversized != null)
			{
				/*
                if (enabled_AlienRaces && pawn.RaceProps.Humanlike)
                {
                    Vector2 v = OgsCompOversizedWeapon.HarmonyCompOversizedWeapon.AlienRacesPatch(___pawn);
                    s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                }
                else
                {
                    s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                }
                */

				if (HarmonyCompActivatableEffect.enabled_AlienRaces && pawn.RaceProps.Humanlike)
				{
					Vector2 v = OgsCompOversizedWeapon.HarmonyCompOversizedWeapon.AlienRacesPatch(pawn);
					s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
				}
				else
				{
					s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
				}
				//    Log.Message("DrawEquipmentAimingPostFix compOversized offset: "+ offset + " Rotation: "+rotation + " size: " + s);
			}
			var matrix = default(Matrix4x4);
			matrix.SetTRS(position, rotation, s);
			Graphics.DrawMesh(mesh, matrix, matSingle, 0);
		}
	}
}
