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
	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    public static class PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler
	{
		public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
		public static bool enabled_YayosCombat = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "com.yayo.combat3");
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();

			for (int i = 0; i < list.Count; i++)
			{
				CodeInstruction instruction = list[i];
				if (instruction.OperandIs(AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), parameters: new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(Int32) })))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1);
					yield return new CodeInstruction(OpCodes.Ldarg_3);
					instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), "DrawMeshModified", null, null));
					if (Prefs.DevMode) Log.Message("Oversized: DrawEquipmentAiming_Vanilla_Transpiled");
				}
				yield return instruction;
			}
		}

		public static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
		{
            if (enabled_YayosCombat)
            {
				Log.Message("Yayos Oversized");
            }
			CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();
			CompEquippable equippable = eq.TryGetCompFast<CompEquippable>();
			Pawn pawn = equippable.PrimaryVerb.CasterPawn;
			if (pawn == null) return;
			if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null || eq == null)
			{
				OversizedUtil.Draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
				return;
			}
			ThingWithComps thingWithComps = eq as ThingWithComps;
			bool DualWeapon = compOversized.Props != null && compOversized.Props.isDualWeapon;
			float offHandAngle = aimAngle;
			float mainHandAngle = aimAngle;
			Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
			LocalTargetInfo localTargetInfo = null;
			if (stance_Busy != null && !stance_Busy.neverAimWeapon)
			{
				localTargetInfo = stance_Busy.focusTarg;
			}
			bool Aiming = OversizedUtil.CurrentlyAiming(stance_Busy);
			Vector3 offsetMainHand = default(Vector3);
			Vector3 offsetOffHand = default(Vector3);
            if (compOversized.Props != null)
			{
				OversizedUtil.SetAnglesAndOffsets(eq, thingWithComps, aimAngle, pawn, ref offsetMainHand, ref offsetOffHand, ref offHandAngle, ref mainHandAngle, Aiming, DualWeapon && Aiming);

			}
			if (DualWeapon)
			{

				Vector3 drawLoc = position + offsetMainHand;
				compOversized.renderPos = drawLoc;
				PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler.DrawEquipmentAimingOverride(mesh, eq, drawLoc, offHandAngle, compOversized, equippable, pawn);

			}

			if (Aiming && localTargetInfo != null)
			{
				mainHandAngle = OversizedUtil.GetAimingRotation(pawn, localTargetInfo);
				offsetMainHand.y += 0.1f;
				Vector3 drawLoc2 = pawn.DrawPos + new Vector3(0f, 0f, 0.4f).RotatedBy(mainHandAngle) + (DualWeapon ? offsetOffHand : offsetMainHand);
				if (DualWeapon)
				{
					compOversized.renderPosDual = drawLoc2;
				}
				else
				{
					compOversized.renderPos = drawLoc2;
				}
				PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler.DrawEquipmentAimingOverride(mesh, thingWithComps, drawLoc2, (DualWeapon ? mainHandAngle : aimAngle), compOversized, equippable, pawn, DualWeapon);
			}
			else
			{
				Vector3 drawLoc2 = position + (DualWeapon ? offsetOffHand : offsetMainHand);
				if (DualWeapon)
				{
					compOversized.renderPosDual = drawLoc2;
				}
				else
				{
					compOversized.renderPos = drawLoc2;
				}
				PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler.DrawEquipmentAimingOverride(mesh, thingWithComps,  drawLoc2, (DualWeapon ? mainHandAngle : aimAngle), compOversized, equippable, pawn, false);
			}

		}

		public static void DrawEquipmentAimingOverride(Mesh mesh, Thing eq, Vector3 drawLoc, float aimAngle, CompOversizedWeapon compOversized, CompEquippable equippable, Pawn pawn, bool offhand = false)
		{
			float num = aimAngle - 90f;
			if (aimAngle > 20f && aimAngle < 160f)
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			else
			{
				if (aimAngle > 200f && aimAngle < 340f)
				{
					mesh = offhand ? (mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10) : MeshPool.plane10Flip;
					num -= 180f;
					num -= eq.def.equippedAngleOffset;
				}
				else
				{
					mesh = MeshPool.plane10;
					num += eq.def.equippedAngleOffset;
				}
			}
			num %= 360f;
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
			Quaternion rotation = Quaternion.AngleAxis(num, Vector3.up);
			matrix.SetTRS(drawLoc, rotation, s);
			Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
			bool flag3 = graphic_StackCount != null;
			Material matSingle;
			if (flag3)
			{
				matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
			}
			else
			{
				matSingle = eq.Graphic.MatSingle;
			}
			OversizedUtil.Draw(mesh, matrix, matSingle, 0, eq, pawn, drawLoc, rotation);
		}

	}
}
