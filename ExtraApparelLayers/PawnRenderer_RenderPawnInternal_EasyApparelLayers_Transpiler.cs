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
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) }), HarmonyPriority(Priority.Last)]
    public static class PawnRenderer_RenderPawnInternal_EasyApparelLayers_Transpiler
    {
        public static int layerCount = DefDatabase<ApparelLayerDef>.AllDefs.Where(x=> x.drawOrder < ApparelLayerDefOf.Shell.drawOrder).Count();
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo lastLayer = AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod();
            MethodInfo baseHeadOffsetAt = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt));
            FieldInfo shell = AccessTools.Field(typeof(ApparelLayerDefOf), "Shell");
            MethodInfo underShell = AccessTools.Method(typeof(PawnRenderer_RenderPawnInternal_EasyApparelLayers_Transpiler), nameof(UnderShell));
            MethodInfo overShell = AccessTools.Method(typeof(PawnRenderer_RenderPawnInternal_EasyApparelLayers_Transpiler), nameof(OverShell));
            var instructionsList = new List<CodeInstruction>(instructions);
            bool underShellYPatched = false;
            bool overShellYPatched = false;
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (i > 1 && instruction.opcode == OpCodes.Bne_Un_S && instructionsList[i - 1].OperandIs(shell) && instructionsList[i - 2].OperandIs(lastLayer))
                {
                    //    Log.Message("LastLayer "+i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: typeof(Main).GetMethod("LastLayerShellOrHigher"));
                    instruction = new CodeInstruction(OpCodes.Brfalse, instruction.operand);
                }

                if (!underShellYPatched && instruction.opcode == OpCodes.Ldc_R4 && instruction.OperandIs((float)3f / 980f))
                {
                    underShellYPatched = true;
                    //    Log.Message("underShellYPatched "+i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return instruction;
                    yield return new CodeInstruction(opcode: OpCodes.Ldloc_S, 7);
                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: underShell);
                }

                if (!overShellYPatched && i > 1 && i < instructionsList.Count -2)
                {
                    if (instructionsList[index: i + 1].opcode == OpCodes.Ldloc_0 && instructionsList[index: i].opcode == OpCodes.Ldloc_2 && instructionsList[index: i -1 ].opcode == OpCodes.Ldloc_1)
                    {
                        //    Log.Message("overShellYPatched "+i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                        overShellYPatched = true;
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 19);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 22);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                        instruction = new CodeInstruction(OpCodes.Call, overShell);
                    }
                }
                yield return instruction;
            }
        }

        public static float UnderShell(float original, List<Material> list)
        {
            float result = original;
            result = (9f / list.Count) / 980f;
        //    Log.Message("<Shell layerCount: " + list.Count + " Increment: " + result);
            return result;
        }
        public static Vector3 OverShell(Vector3 original, Vector3 headPos, Vector3 hairPos, ApparelGraphicRecord apparelGraphicRecord, List<ApparelGraphicRecord> list, Rot4 bodyFacing)
        {
            Vector3 result = original;
            Vector3 headoffset =  (bodyFacing == Rot4.North ? hairPos : headPos) - original;
            List<ApparelGraphicRecord> shellList = list.FindAll(x => Main.LastLayerShellOrHigher(x.sourceApparel.def.apparel.LastLayer, ApparelLayerDefOf.Shell));
            int shellInd = shellList.IndexOf(apparelGraphicRecord);
            float yspace = headoffset.y / (shellList.Count + 1);
            if (bodyFacing == Rot4.North)
            {
                yspace = -yspace;
            }
            float increment = yspace * shellInd;
            result.y += increment;
        //    Log.Message("Shell apparelGraphic " + shellInd + ": " + apparelGraphicRecord.sourceApparel.LabelShortCap + " original: " + original.y + " headPos: " + headPos.y + " headoffset: " + headoffset.y + " Increment: " + increment + " yspace: " + yspace + " result: " + result.y);
            return result;
        }
    }
}
