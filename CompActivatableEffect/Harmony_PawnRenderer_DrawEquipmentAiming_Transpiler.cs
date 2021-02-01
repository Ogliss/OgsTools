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
	//    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
	// Token: 0x020000FB RID: 251
//	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
	public static class Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler
	{
		// Token: 0x060004A1 RID: 1185 RVA: 0x0002500C File Offset: 0x0002320C
		public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            if (enabled_CombatExtended)
			{
			//	list[list.Count - 2].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler), "DrawMeshModified", null, null);
			}
            else
			{
				list[list.Count - 2].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler), "DrawMeshModified", null, null);
				list.InsertRange(list.Count - 2, new CodeInstruction[]
				{
				new CodeInstruction(OpCodes.Ldarg_1, null),
				new CodeInstruction(OpCodes.Ldarg_3, null)
				});
			}
			return list;
		}

		public static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
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
			HarmonyCompActivatableEffect.DrawMeshModified(mesh, matrix, mat, layer, eq,pawn, position, rotation);
		}

	}
}
