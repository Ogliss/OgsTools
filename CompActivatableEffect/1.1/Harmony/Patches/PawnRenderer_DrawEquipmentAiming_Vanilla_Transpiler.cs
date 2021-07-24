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
	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
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
					instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ActivatableEffectUtil), "DrawMeshModified", null, null));
					if (Prefs.DevMode) Log.Message("ActivatableEffect: DrawEquipmentAiming_Vanilla_Transpiled");
				}
				yield return instruction;
			}
		}

		
	}
}
