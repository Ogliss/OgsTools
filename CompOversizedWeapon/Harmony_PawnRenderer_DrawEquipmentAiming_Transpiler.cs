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

namespace OgsCompOversizedWeapon
{
	//    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
	// Token: 0x020000FB RID: 251
	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
	internal static class Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler
	{
		// Token: 0x060004A1 RID: 1185 RVA: 0x0002500C File Offset: 0x0002320C
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();

            list[list.Count - 2].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler), "DrawMeshModified", null, null);
			list.InsertRange(list.Count - 2, new CodeInstruction[]
			{
				new CodeInstruction(OpCodes.Ldarg_1, null),
				new CodeInstruction(OpCodes.Ldarg_3, null)
            });
			return list;
		}

        // Token: 0x060004A0 RID: 1184 RVA: 0x00024F4C File Offset: 0x0002314C
        private static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
        {
            CompOversizedWeapon compOversized = eq.TryGetComp<CompOversizedWeapon>();
            CompEquippable equippable = eq.TryGetComp<CompEquippable>();
            Pawn pawn = equippable.PrimaryVerb.CasterPawn;
            if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null)
            {
                Graphics.DrawMesh(mesh, position, rotation, mat, layer);
                return;
            }
            bool flag4 = false;
            float num = aimAngle - 90f;
            if (aimAngle > 20f && aimAngle < 160f)
            {
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                flag4 = true;
            }
            else
            {
                rotation = Quaternion.AngleAxis(AdjustOffsetAtPeace(eq, pawn, compOversized, rotation.eulerAngles.y), Vector3.up);
            }
            if (compOversized.Props != null && !PawnUtility.IsFighting(pawn) && compOversized.Props.verticalFlipNorth && pawn.Rotation == Rot4.North)
            {
                rotation = Quaternion.AngleAxis(rotation.eulerAngles.y + 180f, Vector3.up);
                //    num += 180f;
            }
            if (!PawnUtility.IsFighting(pawn) || pawn.TargetCurrentlyAimingAt == null)
            {
                rotation = Quaternion.AngleAxis(rotation.eulerAngles.y + AdjustNonCombatRotation(pawn, 0, compOversized), Vector3.up);
            }
            num %= 360f;
            Vector3 s;
            if (pawn.Rotation == Rot4.East)
            {
                flag4 = !flag4;

            }
            if (pawn.RaceProps.Humanlike)
            {
                if (HarmonyCompOversizedWeapon.enabled_AlienRaces)
                {
                    Vector2 v = AlienRacesPatch(pawn);
                    s = new Vector3(eq.def.graphicData.drawSize.x * v.x, 1f, eq.def.graphicData.drawSize.y * v.y);
                }
                else
                {
                    s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                }
            }
            else
            {
                Vector2 v = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                s = new Vector3(eq.def.graphicData.drawSize.x + v.x/10, 1f, eq.def.graphicData.drawSize.y + v.y/10);
            }
            Vector3 offset = AdjustRenderOffsetFromDir(pawn, compOversized);
            //    vector = Vector3Utility.RotatedBy(vector, rotation.eulerAngles.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(position + offset, rotation, s);
            compOversized.renderAngle = rotation;
            compOversized.renderPos = offset;
            compOversized.drawScale = s;
            if (HarmonyCompOversizedWeapon.enabled_rooloDualWield)
                Graphics.DrawMesh((flag4) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, mat, layer);
            else
                Graphics.DrawMesh((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, mat, layer);
            if (compOversized.Props != null && compOversized.Props.isDualWeapon)
            {
                if (pawn.Rotation == Rot4.North || pawn.Rotation == Rot4.South)
                {
                    offset = new Vector3(-1f * offset.x, offset.y, offset.z);
                    rotation = Quaternion.AngleAxis((rotation.eulerAngles.y - ((eq.def.equippedAngleOffset + AdjustNonCombatRotation(pawn, 0, compOversized)) * 2  + 90)) % 360, Vector3.up);
                    /*
                    rotation = Quaternion.AngleAxis(rotation.eulerAngles.y + 135f, Vector3.up);
                    rotation = Quaternion.AngleAxis(rotation.eulerAngles.y % 360f, Vector3.up);
                    */
                    mesh = ((!flag4) ? MeshPool.plane10Flip : MeshPool.plane10);
                }
                else
                {
                    offset = new Vector3(offset.x-0.1f, offset.y - 0.1f, offset.z + 0.15f);
                    mesh = ((!flag4) ? MeshPool.plane10 : MeshPool.plane10Flip);
                }
                compOversized.renderAngleDual = rotation;
                compOversized.renderPosDual = offset;
                matrix.SetTRS(position + offset, rotation, s);
                Graphics.DrawMesh(mesh, matrix, mat, layer);
            }
        }

        public static Vector2 AlienRacesPatch(Pawn pawn)
        {
            AlienRace.ThingDef_AlienRace alienDef = pawn.def as AlienRace.ThingDef_AlienRace;
            Vector2 s = alienDef.alienRace.generalSettings.alienPartGenerator.customDrawSize;
            return s;
        }

        private static float AdjustOffsetAtPeace(Thing eq, Pawn pawn, CompOversizedWeapon compOversizedWeapon, float num)
        {
            Mesh mesh;
            mesh = MeshPool.plane10;
            var offsetAtPeace = eq.def.equippedAngleOffset;
            if (compOversizedWeapon.Props != null && (!pawn.IsFighting() && compOversizedWeapon.Props.verticalFlipOutsideCombat))
            {
                offsetAtPeace += 180f;
            }
            num += offsetAtPeace;
            return num;
        }

        private static float AdjustNonCombatRotation(Pawn pawn, float num, CompOversizedWeapon compOversizedWeapon)
        {
            if (compOversizedWeapon.Props != null)
            {
                if (pawn.Rotation == Rot4.North)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentNorth;
                }
                else if (pawn.Rotation == Rot4.East)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentEast;
                }
                else if (pawn.Rotation == Rot4.West)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentWest;
                }
                else if (pawn.Rotation == Rot4.South)
                {
                    num += compOversizedWeapon.Props.angleAdjustmentSouth;
                }
            }
            return num;
        }

        private static Vector3 AdjustRenderOffsetFromDir(Pawn pawn, CompOversizedWeapon compOversizedWeapon)
        {
            var curDir = pawn.Rotation;

            Vector3 curOffset = Vector3.zero;

            if (compOversizedWeapon.Props != null)
            {

                curOffset = compOversizedWeapon.Props.northOffset;
                if (curDir == Rot4.East)
                {
                    curOffset = compOversizedWeapon.Props.eastOffset;
                }
                else if (curDir == Rot4.South)
                {
                    curOffset = compOversizedWeapon.Props.southOffset;
                }
                else if (curDir == Rot4.West)
                {
                    curOffset = compOversizedWeapon.Props.westOffset;
                }
            }

            return curOffset;
        }

    }
}
