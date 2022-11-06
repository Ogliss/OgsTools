using System.Linq;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System;
using System.Reflection;
using RimWorld;
using System.Reflection.Emit;

namespace ExtraApparelLayers
{
    [HarmonyPatch(typeof(PawnGraphicSet), "MatsBodyBaseAt"), HarmonyPriority(Priority.Last)]
    public static class PawnGraphicSet_MatsBodyBaseAt_EasyApparelLayers_Transpiler
    {
        public static bool LastLayerShellOrHigherPatched = false;
        public static bool LastLayerOverheadOrHigherPatched = false;
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo LastLayer = AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod();
            FieldInfo shell = AccessTools.Field(typeof(ApparelLayerDefOf), "Shell");
            FieldInfo overhead = AccessTools.Field(typeof(ApparelLayerDefOf), "Overhead");
            var instructionsList = new List<CodeInstruction>(instructions);

            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (!LastLayerShellOrHigherPatched && i > 1 && instruction.opcode == OpCodes.Beq 
                    && instructionsList[i-1].OperandIs(shell) && instructionsList[i-2].OperandIs(LastLayer))
                {
                    LastLayerShellOrHigherPatched = true;
                //    Log.Message($"MatsBodyBaseAt LastLayerShell patched");
                    yield return new CodeInstruction(
                        opcode: OpCodes.Call, 
                        operand: LayerReplacement);
                    instruction = new CodeInstruction(OpCodes.Brtrue, instruction.operand);
                }
                if (!LastLayerOverheadOrHigherPatched && i > 1 && instruction.opcode == OpCodes.Beq_S 
                    && instructionsList[i-1].OperandIs(overhead) && instructionsList[i-2].OperandIs(LastLayer))
                {
                    LastLayerOverheadOrHigherPatched = true;
                //    Log.Message($"MatsBodyBaseAt LastLayerOverhead patched");
                    yield return new CodeInstruction(
                        opcode: OpCodes.Call, 
                        operand: LayerReplacement);
                    instruction = new CodeInstruction(OpCodes.Brtrue, instruction.operand);
                }
                yield return instruction;
            }
        }

        static MethodInfo LayerReplacement = AccessTools.Method(typeof(PawnGraphicSet_MatsBodyBaseAt_EasyApparelLayers_Transpiler), nameof(LastLayer));
        public static bool LastLayer(ApparelLayerDef LastLayer, ApparelLayerDef layer)
        {
            if (EasyApparelLayers_Main.settings.overheadLastLayerPatch && layer == ApparelLayerDefOf.Overhead)
            {
                return ApparelLayerUtility.LastLayer(LastLayer, layer);
            }
            return LastLayer == layer;
        }

    }

}
