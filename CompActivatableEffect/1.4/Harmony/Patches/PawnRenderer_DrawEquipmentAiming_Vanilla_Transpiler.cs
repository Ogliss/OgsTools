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
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            MethodInfo graphicsDrawMesh = AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int) });

            for (int i = 0; i < list.Count; i++)
			{
				CodeInstruction instruction = list[i];
				if (instruction.OperandIs(graphicsDrawMesh))
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), nameof(DrawMeshModified), null, null));
					if (Prefs.DevMode) Log.Message("ActivatableEffect: DrawEquipmentAiming_Vanilla_Transpiled");
				}
                /*
				if (i == list.Count-1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), nameof(DrawMeshModified), null, null));
					if (Prefs.DevMode) Log.Message("ActivatableEffect: DrawEquipmentAiming_Vanilla_Transpiled");
				}
                */
				yield return instruction;
			}
		}


        public static void DrawMeshModified(Mesh mesh, Matrix4x4 matrix, int layer, Thing eq)
        {
            ThingWithComps thingWithComps = eq as ThingWithComps;
            var compActivatableEffect = thingWithComps?.GetComp<CompActivatableEffect>();
            if (compActivatableEffect?.Graphic == null) return;
            if (!compActivatableEffect.IsActiveNow) return;
            var matSingle = compActivatableEffect.Graphic.MatSingle;
            Graphics.DrawMesh(mesh, matrix, matSingle, layer);
        }
    }
}
