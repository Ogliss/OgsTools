using Verse;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using System.Reflection.Emit;

namespace ExtraApparelLayers
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    public static class PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler
    {
        static MethodInfo lastLayer = AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod();
        static FieldInfo overhead = AccessTools.Field(typeof(ApparelLayerDefOf), "Overhead");
        static MethodInfo overOverhead = AccessTools.Method(typeof(PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler), nameof(OverOverhead));
        static bool overOverheadPatched = false;
        static bool overInFrontOfFacePatched = false;
        static bool firstLayerPatched = false;
        static bool secondLayerPatched = false;
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                // extra Overhead layers
                if (EasyApparelLayers_Main.settings.overheadLastLayerPatch && (!firstLayerPatched || !secondLayerPatched) && i > 1 && instructionsList[i - 2].OperandIs(lastLayer) && instructionsList[i - 1].OperandIs(overhead))
                {
                    if (firstLayerPatched) secondLayerPatched = true;
                    firstLayerPatched = true;
                //    Log.Message("DrawHeadHair LastLayer overhead " + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: typeof(EasyApparelLayers_Main).GetMethod("LastLayerOverheadOrHigher"));
                    instruction = new CodeInstruction(OpCodes.Brfalse, instruction.operand);
                }
                /*
                if (EasyApparelLayers_Main.settings.overheadYSpacePatch && !overOverheadPatched && i > 1 && instruction.opcode == OpCodes.Ldloc_S && ((LocalBuilder)instruction.operand).LocalIndex == 13 && ((LocalBuilder)instructionsList[i - 1].operand).LocalIndex == 15)
                {
                    overOverheadPatched = true;
                    Log.Message("overOverheadYPatched " + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return instruction; // Vector3 loc2
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // Vector3 Rootloc
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 16); // int j
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // List<ApparelGraphicRecord> list
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4); // Rot4 bodyFacing

                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: overOverhead);
                }
                */
                /*
                if (EasyApparelLayers_Main.settings.overheadInFrontOfFaceYSpacePatch && !overInFrontOfFacePatched && i > 1 && instruction.opcode == OpCodes.Ldloc_S && ((LocalBuilder)instruction.operand).LocalIndex == 19 && ((LocalBuilder)instructionsList[i - 1].operand).LocalIndex == 15)
                {
                    overInFrontOfFacePatched = true;
                        Log.Message("overInFrontOfFacePatched " + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return instruction; // Vector3 loc2
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // Vector3 Rootloc
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 16); // int j
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // List<ApparelGraphicRecord> list
                    yield return new CodeInstruction(OpCodes.Ldarg_S, 4); // Rot4 bodyFacing

                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: overOverhead);
                }
                */
                yield return instruction;
            }
            if (EasyApparelLayers_Main.settings.overheadLastLayerPatch && !firstLayerPatched || !secondLayerPatched)
            {
                Log.Warning($"Warning!! - DrawHeadHair LastLayer NOT patched - Only apparel with Last Layer Overhead or EyeCover will render on this layer");
            }
            /*
            if (EasyApparelLayers_Main.settings.overheadYSpacePatch && !overOverheadPatched)
            {
                Log.Warning($"Warning!! - DrawHeadHair Y Space NOT patched - Only apparel with Last Layer Overhead or EyeCover will render on this layer");
            }
            if (EasyApparelLayers_Main.settings.overheadInFrontOfFaceYSpacePatch && !overInFrontOfFacePatched)
            {
                Log.Warning($"Warning!! - DrawHeadHair Y Space In Front of Face NOT patched - Only apparel with Last Layer Overhead or EyeCover will render on this layer");
            }
            */
        }

        public static Vector3 OverOverhead(Vector3 original, Vector3 root, int j, List<ApparelGraphicRecord> list, Rot4 bodyFacing)
        {
            Vector3 result = original;
            Vector3 r = root;
            ApparelGraphicRecord apparelGraphicRecord = list[j];
            r.y += 0.036734693f;
            Vector3 headoffset = r - original;
            List<ApparelGraphicRecord> shellList = list.FindAll(x => EasyApparelLayers_Main.LastLayerOverheadOrHigher(x.sourceApparel.def.apparel.LastLayer));
            int shellInd = shellList.IndexOf(apparelGraphicRecord);
            float yspace = headoffset.y / (shellList.Count + 1);

            if (bodyFacing == Rot4.North && apparelGraphicRecord.sourceApparel.def.apparel.hatRenderedFrontOfFace)
            {
                yspace = -yspace;
            }

            float increment = yspace * shellInd;
            result.y += increment;
            //    Log.Message("Overhead apparelGraphic " + shellInd + ": " + apparelGraphicRecord.sourceApparel.LabelShortCap + " original: " + original.y + " root: " + root.y + " offset: " + headoffset.y + " Increment: " + increment + " yspace: " + yspace + " result: " + result.y);
            return result;
        }

    }
}
