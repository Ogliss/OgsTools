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
using OgsCompOversizedWeapon.ExtentionMethods;

namespace OgsCompOversizedWeapon
{
    //	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    public static class PawnRenderer_DrawEquipmentAiming_YayoPrefix_Transpiler
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            FieldInfo pawn = AccessTools.Field(typeof(PawnRenderer), "pawn");
			for (int i = 0; i < list.Count; i++)
			{
				CodeInstruction instruction = list[i];
				if (instruction.OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32) })))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1); // Thing eq
					yield return new CodeInstruction(OpCodes.Ldloc_0); // Pawn pawn // this will need to be fixed when Yayos fixed
					instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_YayoPrefix_Transpiler), "DrawMeshModified", null, null));
					if (Prefs.DevMode) Log.Message("Oversized: DrawEquipmentAiming_YayoPrefix_Transpiled");
				}
				yield return instruction;
			}
		}

        private static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, Pawn pawn)
        {
            CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();
            if (pawn == null) return;
            if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null || eq == null)
            {
                OversizedUtil.Draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
                return;
            }
            Vector3 s;
            if (pawn.RaceProps.Humanlike)
            {
                if (HarmonyPatches_OversizedWeapon.enabled_AlienRaces)
                {
                    Vector2 v = AlienRaceUtility.AlienRacesPatch(pawn, eq);
                    float f = Mathf.Max(v.x, v.y);
                    s = new Vector3(eq.def.graphicData.drawSize.x * f, 1f, eq.def.graphicData.drawSize.y * f);
                }
                else
                {
                    s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                }
            }
            else
            {
                Vector2 v = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                s = new Vector3(eq.def.graphicData.drawSize.x + v.x / 10, 1f, eq.def.graphicData.drawSize.y + v.y / 10);
            }
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(position, rotation, s);
            Log.Message("i am being run, yes?");
            OversizedUtil.Draw(mesh, matrix, mat, layer, eq, pawn, position, rotation);
        }
    }
}
