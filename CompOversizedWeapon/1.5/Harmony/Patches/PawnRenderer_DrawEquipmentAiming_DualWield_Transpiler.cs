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
    public static class PawnRenderer_DrawEquipmentAiming_DualWield_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();

            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                if (instruction.OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(Int32) })))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_DualWield_Transpiler), "DrawMeshModified", null, null));
                    if (Prefs.DevMode) Log.Message("Oversized: DrawEquipmentAiming_DualWield_Transpiled");
                }
                yield return instruction;
            }
        }

        private static void DrawMeshModified(Mesh mesh, Matrix4x4 matrix, Material material, int layer, Thing eq, float aimAngle)
        {
            CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();
            CompEquippable equippable = eq.TryGetCompFast<CompEquippable>();
            Pawn pawn = equippable.PrimaryVerb.CasterPawn;
            if (pawn == null) return;
            if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null || eq == null)
            {
            //    OversizedUtil.Draw(mesh, matrix, material, layer, eq, pawn);
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
            OversizedUtil.Draw(mesh, matrix, material, 0);
        }

    }
    public static class PawnRenderer_DrawEquipmentAiming_DualWieldPrefix_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            MethodInfo target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null);
            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                if (instruction.OperandIs(target))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_DualWieldPrefix_Transpiler), "DrawEquipmentAimingOverride", null, null));
                    if (Prefs.DevMode) Log.Message("Oversized: DrawEquipmentAiming_DualWieldPrefix_Transpiled");
                }
                yield return instruction;
            }
        }


        public static void DrawEquipmentAimingOverride(Thing eq, Vector3 drawLoc, float aimAngle, ThingWithComps offHandEquip = null)
        {
            CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();
            CompEquippable equippable = eq.TryGetCompFast<CompEquippable>();
            Pawn pawn = equippable.PrimaryVerb.CasterPawn;
            bool flip = offHandEquip != null && eq == offHandEquip;
            float num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material matSingle;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }
            if (HarmonyPatches_OversizedWeapon.enabled_YayosCombat && flip)
            {
            //    Log.Message("Yayos Oversized offhand");
                if (pawn.Rotation == Rot4.East || pawn.Rotation == Rot4.West)
                {
                    mesh = mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
                }
            }

            DrawMeshModified(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0, eq, pawn, compOversized);
        }

        private static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, Pawn pawn, CompOversizedWeapon compOversized = null)
        {
            if (pawn == null) return;
            if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null || eq == null)
            {
            //    OversizedUtil.Draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
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
        //    OversizedUtil.Draw(mesh, matrix, mat, layer, eq, pawn, position, rotation);
        }

    }
}
