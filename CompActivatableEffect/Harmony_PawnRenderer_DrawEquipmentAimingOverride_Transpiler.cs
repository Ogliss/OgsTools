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

        public static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
        {
            CompEquippable equippable = eq.TryGetComp<CompEquippable>();
            Pawn pawn = equippable.PrimaryVerb.CasterPawn;
            Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
            return;
        }


    }
}
