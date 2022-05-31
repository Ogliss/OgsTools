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

    [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
    public static class PawnRenderer_DrawBodyApparel_EasyApparelLayers_Transpiler
    {
        public static int layerCount = DefDatabase<ApparelLayerDef>.AllDefs.Where(x => x.drawOrder < ApparelLayerDefOf.Shell.drawOrder).Count();
        private static bool overShellYPatched = false;
        static bool layerPatched = false;
        static MethodInfo lastLayer = AccessTools.Property(typeof(ApparelProperties), "LastLayer").GetGetMethod();
        static MethodInfo baseHeadOffsetAt = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.BaseHeadOffsetAt));
        static FieldInfo shell = AccessTools.Field(typeof(ApparelLayerDefOf), "Shell");
        static FieldInfo overhead = AccessTools.Field(typeof(ApparelLayerDefOf), "Overhead");
        static MethodInfo overShell = AccessTools.Method(typeof(PawnRenderer_DrawBodyApparel_EasyApparelLayers_Transpiler), nameof(OverShell));
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                // cut y space between layers
                if (EasyApparelLayers_Main.settings.shellLastLayerPatch && !layerPatched && i > 1 && instruction.opcode == OpCodes.Bne_Un && instructionsList[i - 1].OperandIs(shell) && instructionsList[i - 2].OperandIs(lastLayer))
                {
                    layerPatched = true;
                    //    Log.Message("DrawBodyApparel LastLayer shell" + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return new CodeInstruction(opcode: OpCodes.Call, operand: typeof(EasyApparelLayers_Main).GetMethod("LastLayerShellOrHigher"));
                    instruction = new CodeInstruction(OpCodes.Brfalse, instruction.operand);
                }
                if (EasyApparelLayers_Main.settings.shellYSpacePatch && !overShellYPatched && i > 1 && i < instructionsList.Count - 2 && instructionsList[index: i].opcode == OpCodes.Stloc_S && ((LocalBuilder)instructionsList[index: i].operand).LocalIndex == 5)
                {
                //    Log.Message("DrawBodyApparel overShellYPatched " + i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    overShellYPatched = true;
                    yield return new CodeInstruction(OpCodes.Ldloc, 3); // ApparelGraphicRecord apparelGraphicRecord
                    yield return new CodeInstruction(OpCodes.Ldloc, 0); // List<ApparelGraphicRecord> list
                    yield return new CodeInstruction(OpCodes.Ldarg, 5); // Rot4 bodyFacing
                    /*
                    */
                    yield return new CodeInstruction(OpCodes.Call, overShell);
                }
                yield return instruction;
            }
            if (EasyApparelLayers_Main.settings.shellLastLayerPatch && !layerPatched)
            {
                Log.Warning($"Warning!! - DrawBodyApparel LastLayer NOT patched - Only apparel with Last Layer Shell will render on this layer");
            }
            if (EasyApparelLayers_Main.settings.shellYSpacePatch && !overShellYPatched)
            {
                Log.Warning($"Warning!! - DrawBodyApparel Spacing NOT patched - Shell apparel will render with vanilla spacing");
            }
        }

        public static Vector3 OverShell(Vector3 original, ApparelGraphicRecord apparelGraphicRecord, List<ApparelGraphicRecord> list, Rot4 bodyFacing)
        {
            if (!EasyApparelLayers_Main.settings.shellYSpacePatch)
            {
                return original;
            }
            float y = original.y;
            Vector3 root = original;
            Vector3 result = original;
            List<ApparelGraphicRecord> shellList = list.FindAll(x => EasyApparelLayers_Main.LastLayerShellOrHigher(x.sourceApparel.def.apparel.LastLayer));
            int shellInd = shellList.IndexOf(apparelGraphicRecord);
            float yspace;
            float b;
            float h;
            if (bodyFacing != Rot4.North)
            {
                result.y -= (PawnRenderer.YOffset_Shell / 2);// (0.02027027f - (0.02027027f / shellList.Count));
                h = PawnRenderer.YOffset_Head;
                b = PawnRenderer.YOffset_Shell;
            }
            else
            {
                result.y -= (PawnRenderer.YOffset_Head - PawnRenderer.YOffset_Shell) / 2;// - (0.023166021f / shellList.Count));
                h = PawnRenderer.YOffset_Shell;
                b = PawnRenderer.YOffset_Head;
            }
            yspace = (h - (b / 2)) / (shellList.Count + 1);
            // 0.022166021f

            //0.028957527f
            /*
            if (bodyFacing == Rot4.North)
            {
                yspace = -yspace;
            }
            float result = (b/2) / (list.Count + 1);
            */
            float increment =  yspace * shellInd;
            result.y += yspace + increment;
        //    Log.Message($"DrawBodyApparel Shell apparelGraphic[{shellInd}]: {apparelGraphicRecord.sourceApparel.LabelShortCap}, root: {root}, original: {original}, Increment: {increment} yspace: {yspace} result: {result}");
            return result;
        }


    }
}
