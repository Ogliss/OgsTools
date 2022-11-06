using Verse;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using System.Reflection.Emit;
using System.Linq;

namespace ExtraApparelLayers
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair"), HarmonyPriority(Priority.Last)]
    public static class PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler
    {
        static FieldInfo onHeadLocField = AccessTools.TypeByName("PawnRenderer").GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.GetFields().Any(y => y.Name.Contains("onHeadLoc"))).GetField("onHeadLoc");
        static FieldInfo rootLocField = AccessTools.TypeByName("PawnRenderer").GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.GetFields().Any(y => y.Name.Contains("rootLoc"))).GetField("rootLoc");
        static FieldInfo bodyFacingField = AccessTools.TypeByName("PawnRenderer").GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.GetFields().Any(y => y.Name.Contains("bodyFacing"))).GetField("bodyFacing");
        static MethodInfo lastLayer = AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod();
        static FieldInfo overhead = AccessTools.Field(typeof(ApparelLayerDefOf), "Overhead");
        static FieldInfo graphics = AccessTools.Field(typeof(PawnRenderer), "graphics");
        static FieldInfo apparelGraphics = AccessTools.Field(typeof(PawnGraphicSet), "apparelGraphics");
        static MethodInfo overOverhead = AccessTools.Method(typeof(PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler), nameof(OverOverhead));
        static MethodInfo incOverhead = AccessTools.Method(typeof(PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler), nameof(IncOverhead));

        static bool overOverheadPatched = false;
        static bool overInFrontOfFacePatched = false;
        static bool firstLayerPatched = false;
        static bool secondLayerPatched = false;
        static bool orderedList = false;
        public static List<ApparelGraphicRecord> records;
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
        //    Log.Message("Troublesome Transpiler starts here");
            var instructionsList = new List<CodeInstruction>(instructions);
            int z = 0;
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (instruction.operand != null && instruction.operand.ToString().Contains("g__DrawApparel")) Log.Message($"{instruction.opcode}.{instruction.operand} ({instruction.GetType()}) @ {i}");
                if (instruction.opcode == OpCodes.Stloc_1 && !orderedList)
                {
                //    Log.Message($"DrawHeadHair List<ApparelGraphicRecord> Reorder Patch");
                    orderedList = true;
                       yield return new CodeInstruction(opcode: OpCodes.Call, operand: typeof(PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler).GetMethod("apparelGraphicRecordsOrdered"));

                }
                // Allow new Overhead layers
                if ((!firstLayerPatched || !secondLayerPatched) && i > 1 && instructionsList[i - 2].OperandIs(lastLayer) && instructionsList[i - 1].OperandIs(overhead))
                {
                    if (firstLayerPatched)
                    {
                        secondLayerPatched = true;
                    }
                    string s = secondLayerPatched ? "for render" : "for flags";
                //    Log.Message($"DrawHeadHair LastLayer overhead {s} {i} opcode: {instruction.opcode} operand: {instruction.operand}");
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: LayerReplacement);
                    instruction = new CodeInstruction(OpCodes.Brtrue, instruction.operand);
                    firstLayerPatched = true;
                }
                
                yield return instruction;
            }
            if (EasyApparelLayers_Main.settings.overheadLastLayerPatch && !firstLayerPatched || !secondLayerPatched)
            {

                Log.Warning($"Warning!! - DrawHeadHair LastLayer NOT patched - Only apparel with Last Layer Overhead or EyeCover will render on this layer");
            }
        }

        static MethodInfo LayerReplacement = AccessTools.Method(typeof(PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler), nameof(LastLayer));
        public static bool LastLayer(ApparelLayerDef LastLayer, ApparelLayerDef layer)
        {
            bool result = LastLayer == layer;
            if (EasyApparelLayers_Main.settings.overheadLastLayerPatch && layer == ApparelLayerDefOf.Overhead)
            {
                result = ApparelLayerUtility.LastLayer(LastLayer, layer);
            }
        //    Log.Message($"DrawHeadHair Checking {LastLayer} Vs {layer} = {result}");
            return result;
        }

        public static List<ApparelGraphicRecord> apparelGraphicRecordsOrdered(List<ApparelGraphicRecord> list)
        {
            records = list.OrderBy(x => x.sourceApparel.def.apparel.LastLayer.drawOrder).ToList();
        //    Log.Message($"DrawHeadHair apparelGraphicRecordsOrdered returning {records.Count}/{list.Count} appareal, in draw order of last layer");
            return records;
        }

        static IEnumerable<CodeInstruction> CompilerGenereatedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            int ldcr4Patched = 0;
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                // Cut Y Space between Overhead layers
                if (!overOverheadPatched && instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(rootLocField))
                {
                    overOverheadPatched = true;
                    //      Log.Message("overOverheadYPatched " + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return instruction; // Vector3 loc2
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // PawnRenderer
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // ApparelGraphicRecord

                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: overOverhead);
                }
                // Cut Y Space between Overhead InFrontOfFace layers
                if (instruction.opcode == OpCodes.Ldc_R4)
                {
                    ldcr4Patched++;
                    overInFrontOfFacePatched = true;
                    //        Log.Message("overInFrontOfFacePatched " + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return instruction; // Vector3 loc2
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // PawnRenderer
                    yield return new CodeInstruction(OpCodes.Ldarg_1); // ApparelGraphicRecord
                    instruction = new CodeInstruction(opcode: OpCodes.Call, operand: incOverhead);
                }
                
                yield return instruction;
            }
            
            if (EasyApparelLayers_Main.settings.overheadYSpacePatch && !overOverheadPatched)
            {
                Log.Warning($"Warning!! - DrawHeadHair Y Space NOT patched - Only apparel with Last Layer Overhead or EyeCover will render on this layer");
            }
            if (EasyApparelLayers_Main.settings.overheadInFrontOfFaceYSpacePatch && !overInFrontOfFacePatched)
            {
                Log.Warning($"Warning!! - DrawHeadHair Y Space In Front of Face NOT patched - Only apparel with Last Layer Overhead or EyeCover will render on this layer");
            }
            
        }

        public static Vector3 OverOverhead(Vector3 original, PawnRenderer instance, ApparelGraphicRecord apparelGraphicRecord)
        {
            Vector3 result = original;
            if (!EasyApparelLayers_Main.settings.overheadYSpacePatch)
            {
                return result;
            }
            List<ApparelGraphicRecord> List = instance.graphics.apparelGraphics.FindAll(x => ApparelLayerUtility.LastLayerOverhead(x.sourceApparel.def.apparel.LastLayer));
            int shellInd = List.IndexOf(apparelGraphicRecord);
            float yspace = ApparelLayerUtility.headgearYSpace / List.Count;
            float increment = yspace * shellInd;
            result.y += increment;
            // Log.Message("Overhead apparelGraphic " + shellInd + ": " + apparelGraphicRecord.sourceApparel.LabelShortCap + " original: " + original + " Increment: " + increment + " yspace: " + yspace + " result: " + result);
            return result;
        }
        public static float IncOverhead(float original, PawnRenderer instance, ApparelGraphicRecord apparelGraphicRecord)
        {
            float result = original;
            if (!EasyApparelLayers_Main.settings.overheadInFrontOfFaceYSpacePatch)
            {
                return result;
            }
            List<ApparelGraphicRecord> shellList = instance.graphics.apparelGraphics.FindAll(x => ApparelLayerUtility.LastLayerOverhead(x.sourceApparel.def.apparel.LastLayer));
            int shellInd = shellList.IndexOf(apparelGraphicRecord);
            float yspace = ApparelLayerUtility.headgearYSpace / (shellList.Count + 1);
            float increment = yspace * shellInd;
            result += increment;
        //        Log.Message("IncOverhead apparelGraphic " + shellInd + ": " + apparelGraphicRecord.sourceApparel.LabelShortCap + " original: " + original  +  " Increment: " + increment + " yspace: " + yspace + " result: " + result);
            return result;
        }

    }
}
