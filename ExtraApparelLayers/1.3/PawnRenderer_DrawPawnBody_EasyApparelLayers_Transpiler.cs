using System.Linq;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using System.Reflection.Emit;

namespace ExtraApparelLayers
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawPawnBody"), HarmonyPriority(Priority.Last)]
    public static class PawnRenderer_DrawPawnBody_EasyApparelLayers_Transpiler
    {
        public static int layerCount = DefDatabase<ApparelLayerDef>.AllDefs.Where(x => x.drawOrder < ApparelLayerDefOf.Shell.drawOrder).Count();
        private static bool underShellYPatched = false;
        static  MethodInfo lastLayer = AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod();
        static MethodInfo baseHeadOffsetAt = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt));
        static FieldInfo shell = AccessTools.Field(typeof(ApparelLayerDefOf), "Shell");
        static FieldInfo overhead = AccessTools.Field(typeof(ApparelLayerDefOf), "Overhead");
        static MethodInfo underShell = AccessTools.Method(typeof(PawnRenderer_DrawPawnBody_EasyApparelLayers_Transpiler), nameof(UnderShell));
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                // cut y space between layers
                if (!underShellYPatched && instruction.opcode == OpCodes.Ldc_R4 && instruction.OperandIs((float)0.0028957527f))
                {
                    underShellYPatched = true;
                //    Log.Message("DrawPawnBody underShellYPatched " + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return instruction; // float original
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_S, 3); // List<Material> list
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg, 1); // Vector3 rootLoc
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg, 3); // Rot4 facing
                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: underShell);
                }
                yield return instruction;
            }
            if (EasyApparelLayers_Main.settings.bodyYSpacePatch && !underShellYPatched)
            {
                Log.Warning($"Warning!! - DrawPawnBody NOT patched - Body apparel will render as per vanilla");
            }
        }

        public static float UnderShell(float original, List<Material> list, Vector3 rootLoc, Rot4 facing)
        {
            if (EasyApparelLayers_Main.settings.bodyYSpacePatch)
            {
                return original;
            }
            // result = (9f / list.Count) / 980f;
            //    vector.y += 0.008687258f;
            float b;
            float h;
            if (facing != Rot4.North)
            {
                h = 0.023166021f;
                b = 0.02027027f;
            }
            else
            {
                h = 0.02027027f;
                b = 0.023166021f;
            }
            float result = original / (list.Count + 1);
            Log.Message($"DrawPawnBody UnderShell layerCount: {list.Count}, Increment: {result}, OverShell: {result < PawnRenderer.YOffset_Shell}");
            return result;
        }


    }
}
