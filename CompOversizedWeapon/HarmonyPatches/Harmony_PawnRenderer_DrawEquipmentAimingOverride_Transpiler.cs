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
    public static class Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();

            int t = list.Count - 3;
            
            list[t].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler), "DrawMeshModified", null, null);
            list.InsertRange(t, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0, null),
                new CodeInstruction(OpCodes.Ldarg_2, null)
            });
            
            return list;
        }

        private static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
        {
            CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();
            CompEquippable equippable = eq.TryGetCompFast<CompEquippable>();
            Pawn pawn = equippable.PrimaryVerb.CasterPawn;
            if (pawn == null) return;
            if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null || eq == null)
            {
                Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
                return;
            }
            Vector3 s;
            if (pawn.RaceProps.Humanlike)
            {
                if (HarmonyCompOversizedWeapon.enabled_AlienRaces)
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
            Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.draw(mesh, matrix, mat, 0, eq, pawn, position, rotation);
        }

    }
}
