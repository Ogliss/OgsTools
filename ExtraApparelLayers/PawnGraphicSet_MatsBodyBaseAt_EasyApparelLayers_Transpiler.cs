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
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo LastLayer = AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod();
            FieldInfo shell = AccessTools.Field(typeof(ApparelLayerDefOf), "Shell");
            var instructionsList = new List<CodeInstruction>(instructions);

            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (i > 1 && instruction.opcode == OpCodes.Beq_S && instructionsList[i-1].OperandIs(shell) && instructionsList[i-2].OperandIs(LastLayer))
                {
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: typeof(Main).GetMethod("LastLayerShellOrHigher"));
                    instruction = new CodeInstruction(OpCodes.Brtrue, instruction.operand);
                }
                yield return instruction;
            }
        }

    }

}
