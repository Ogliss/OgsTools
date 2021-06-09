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
//	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    public static class PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler
	{
		public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();

			for (int i = 0; i < list.Count; i++)
			{
				CodeInstruction instruction = list[i];
				if (instruction.OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32) })))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), "DrawMeshModified", null, null));
					if (Prefs.DevMode) Log.Message("ActivatableEffect: DrawEquipmentAiming_Vanilla_Transpiled");
				}
				yield return instruction;
			}
		}

		public static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq)
		{
			CompEquippable equippable = eq.TryGetComp<CompEquippable>();
			Pawn pawn = equippable.PrimaryVerb.CasterPawn;
			draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
			return;
		}
		
		public static void draw(Mesh mesh, Matrix4x4 matrix, Material mat, int layer, Thing eq, Pawn pawn, Vector3 position, Quaternion rotation)
		{
            if (matrix == default(Matrix4x4))
            {
				Graphics.DrawMesh(mesh, position, rotation, mat, layer);
			}
            else Graphics.DrawMesh(mesh, matrix, mat, layer);
			HarmonyPatches_ActivatableEffect.DrawMeshModified(mesh, matrix, mat, layer, eq,pawn, position, rotation);
		}

	}
}
