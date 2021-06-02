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
	public static class Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler
	{
		public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            if (enabled_CombatExtended)
			{
			//	list[list.Count - 2].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler), "DrawMeshModified", null, null);
			}
            else
			{
				list[list.Count - 2].operand = AccessTools.Method(typeof(Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler), "DrawMeshModified", null, null);
				list.InsertRange(list.Count - 2, new CodeInstruction[]
				{
				new CodeInstruction(OpCodes.Ldarg_1, null),
				new CodeInstruction(OpCodes.Ldarg_3, null)
				});
			}
			return list;
		}

		public static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
		{

			CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();
			CompEquippable equippable = eq.TryGetCompFast<CompEquippable>();
			Pawn pawn = equippable.PrimaryVerb.CasterPawn;
			if (pawn == null) return;
			if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null || eq == null)
			{
				draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
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
			bool Aiming = Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.CurrentlyAiming(stance_Busy);
			Vector3 offsetMainHand = default(Vector3);
			Vector3 offsetOffHand = default(Vector3);
            if (compOversized.Props != null)
			{
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.SetAnglesAndOffsets(eq, thingWithComps, aimAngle, pawn, ref offsetMainHand, ref offsetOffHand, ref offHandAngle, ref mainHandAngle, Aiming, DualWeapon && Aiming);

			}
			if (DualWeapon)
			{

				Vector3 drawLoc = position + offsetMainHand;
				compOversized.renderPos = drawLoc;
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.DrawEquipmentAimingOverride(mesh, eq, drawLoc, offHandAngle, compOversized, equippable, pawn);

			}

			if (Aiming && localTargetInfo != null)
			{
				mainHandAngle = Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.GetAimingRotation(pawn, localTargetInfo);
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
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.DrawEquipmentAimingOverride(mesh, thingWithComps, drawLoc2, (DualWeapon ? mainHandAngle : aimAngle), compOversized, equippable, pawn, DualWeapon);
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
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.DrawEquipmentAimingOverride(mesh, thingWithComps,  drawLoc2, (DualWeapon ? mainHandAngle : aimAngle), compOversized, equippable, pawn, false);
			}

		}
		
		public static bool DrawMeshModifiedCE(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq, float aimAngle)
		{

			CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();
			CompEquippable equippable = eq.TryGetCompFast<CompEquippable>();
			Pawn pawn = equippable.PrimaryVerb.CasterPawn;
			if (pawn == null) return true;
			if (compOversized == null || (compOversized != null && compOversized.CompDeflectorIsAnimatingNow) || pawn == null || eq == null)
			{
				Graphics.DrawMesh(mesh, position, rotation, mat, layer);
				return true;
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
			bool Aiming = Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.CurrentlyAiming(stance_Busy);
			Vector3 offsetMainHand = default(Vector3);
			Vector3 offsetOffHand = default(Vector3);
            if (compOversized.Props != null)
			{
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.SetAnglesAndOffsets(eq, thingWithComps, aimAngle, pawn, ref offsetMainHand, ref offsetOffHand, ref offHandAngle, ref mainHandAngle, Aiming, DualWeapon && Aiming);

			}
			if (DualWeapon)
			{

				Vector3 drawLoc = position + offsetMainHand;
				compOversized.renderPos = drawLoc;
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.DrawEquipmentAimingOverride(mesh, eq, drawLoc, offHandAngle, compOversized, equippable, pawn);

			}

			if (Aiming && localTargetInfo != null)
			{
				mainHandAngle = Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.GetAimingRotation(pawn, localTargetInfo);
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
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.DrawEquipmentAimingOverride(mesh, thingWithComps, drawLoc2, (DualWeapon ? mainHandAngle : aimAngle), compOversized, equippable, pawn, DualWeapon);
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
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.DrawEquipmentAimingOverride(mesh, thingWithComps,  drawLoc2, (DualWeapon ? mainHandAngle : aimAngle), compOversized, equippable, pawn, false);
			}
			return false;

		}

		// Token: 0x0600007C RID: 124 RVA: 0x000060A0 File Offset: 0x000042A0
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
				if (HarmonyCompOversizedWeapon.enabled_AlienRaces)
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
			draw(mesh, matrix, matSingle, 0, eq, pawn, drawLoc, rotation);
		}

		public static float meleeXOffset = 0.4f;
		public static float rangedXOffset = 0.1f;
		public static float meleeZOffset = 0f;
		public static float rangedZOffset = 0f;
		public static float meleeAngle = 270f;
		public static bool meleeMirrored = true;
		public static float rangedAngle = 135f;
		public static bool rangedMirrored = true;
		// Token: 0x0600007D RID: 125 RVA: 0x00006190 File Offset: 0x00004390
		public static void SetAnglesAndOffsets(Thing eq, ThingWithComps offHandEquip, float aimAngle, Thing thing, ref Vector3 offsetMainHand, ref Vector3 offsetOffHand, ref float mainHandAngle, ref float offHandAngle, bool mainHandAiming, bool offHandAiming)
		{
			CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();

			CompProperties_OversizedWeapon PropsOversized = compOversized.Props;

			Pawn pawn = thing as Pawn;

			bool Melee = pawn != null;
			if (Melee)
			{
				Melee = Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.IsMeleeWeapon(pawn.equipment.Primary);
			}

			bool Dual = false;
            if (PropsOversized != null)
            {
				Dual = PropsOversized.isDualWeapon;
			}
			float num = meleeMirrored ? (360f - meleeAngle) : meleeAngle;
			float num2 = rangedMirrored ? (360f - rangedAngle) : rangedAngle;
			Vector3 offset = AdjustRenderOffsetFromDir(thing.Rotation, compOversized, offHandAiming);
			if (thing.Rotation == Rot4.East)
			{
				offsetMainHand.z += offset.z;
				offsetMainHand.x += offset.x;
				offsetOffHand.y = -1f;
				offsetOffHand.z = 0.1f;
				offsetOffHand.z += offset.z;
				offsetOffHand.x += offset.x;
                if (PropsOversized != null)
				{
					mainHandAngle += PropsOversized.angleAdjustmentEast;
				}
				offHandAngle = mainHandAngle;
			}
			else
			{
				if (thing.Rotation == Rot4.West)
				{
					if (Dual) offsetMainHand.y = -1f;
					offsetMainHand.z += offset.z;
					offsetMainHand.x += offset.x;
					offsetOffHand.z = -0.1f;
					offsetOffHand.z += offset.z;
					offsetOffHand.x += offset.x;
					if (PropsOversized != null)
					{
						mainHandAngle += PropsOversized.angleAdjustmentWest;
					}
					offHandAngle = mainHandAngle;
				}
				else
				{
					if (thing.Rotation == Rot4.North)
					{
						if (!mainHandAiming)
						{
							offsetMainHand.x = offset.x + (Dual ? (Melee ? meleeXOffset : rangedXOffset) : 0);
							offsetOffHand.x = -offset.x + (Melee ? -meleeXOffset : -rangedXOffset);
							offsetMainHand.z = offset.z + (Dual ? (Melee ? meleeZOffset : rangedZOffset) : 0);
							offsetOffHand.z = offset.z + (Melee ? meleeZOffset : rangedZOffset);
							if (PropsOversized != null)
							{
								offHandAngle = PropsOversized.angleAdjustmentNorth + (Melee ? meleeAngle : rangedAngle);
								mainHandAngle = -PropsOversized.angleAdjustmentNorth + (Melee ? num : num2);
							}
						}
						else
						{
							offsetOffHand.x = -0.1f;
						}
					}
					else
					{
						if (!mainHandAiming)
						{
							offsetMainHand.y = 1f;
							offsetMainHand.x = -offset.x + (Dual ? (Melee ? -meleeXOffset : -rangedXOffset) : 0);
							offsetOffHand.x = offset.x + (Melee ? meleeXOffset : rangedXOffset);
							offsetMainHand.z = offset.z + (Dual ? (Melee ? meleeZOffset : rangedZOffset) : 0);
							offsetOffHand.z = offset.z + (Melee ? meleeZOffset : rangedZOffset);
							if (PropsOversized != null)
							{
								offHandAngle = -PropsOversized.angleAdjustmentSouth + (Melee ? num : num2);
								mainHandAngle = PropsOversized.angleAdjustmentSouth + (Melee ? meleeAngle : rangedAngle);
							}
						}
						else
						{
							offsetOffHand.y = 1f;
							offHandAngle = (!Melee ? num : num2);
							offsetOffHand.x = 0.1f;
						}
					}
				}
			}
			if (!thing.Rotation.IsHorizontal)
			{
				if (compOversized.Props != null)
				{

					/*

					offHandAngle += (float)((pawn.Rotation == Rot4.North) ? record.extraRotation : (-(float)record.extraRotation));
					mainHandAngle += (float)((pawn.Rotation == Rot4.North) ? (-(float)compOversized.extraRotation) : compOversized.extraRotation);
					*/
				}
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000064EC File Offset: 0x000046EC
		private static float GetAimingRotation(Pawn pawn, LocalTargetInfo focusTarg)
		{
			bool hasThing = focusTarg.HasThing;
			Vector3 a;
			if (hasThing)
			{
				a = focusTarg.Thing.DrawPos;
			}
			else
			{
				a = focusTarg.Cell.ToVector3Shifted();
			}
			float result = 0f;
			bool flag = (a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f;
			if (flag)
			{
				result = (a - pawn.DrawPos).AngleFlat();
			}
			return result;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00006568 File Offset: 0x00004768
		private static bool CurrentlyAiming(Stance_Busy stance)
		{
			return stance != null && !stance.neverAimWeapon && stance.focusTarg.IsValid;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00006594 File Offset: 0x00004794
		private static bool IsMeleeWeapon(ThingWithComps eq)
		{
			bool flag = eq == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				CompEquippable compEquippable = eq.TryGetCompFast<CompEquippable>();
				bool flag2 = compEquippable != null;
				if (flag2)
				{
					bool isMeleeAttack = compEquippable.PrimaryVerb.IsMeleeAttack;
					if (isMeleeAttack)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}
		public static void draw(Mesh mesh, Matrix4x4 matrix, Material mat, int layer, Thing eq, Pawn pawn, Vector3 position, Quaternion rotation)
		{
            if (matrix == default(Matrix4x4))
            {
				Graphics.DrawMesh(mesh, position, rotation, mat, layer);
			}
            else Graphics.DrawMesh(mesh, matrix, mat, layer);
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

		private static Vector3 AdjustRenderOffsetFromDir(Rot4 curDir, CompOversizedWeapon compOversizedWeapon, bool Offhand = false)
		{

			Vector3 curOffset = Vector3.zero;

			if (compOversizedWeapon.Props != null)
			{

				curOffset = Offhand ? -compOversizedWeapon.Props.southOffset : compOversizedWeapon.Props.southOffset;
				if (curDir == Rot4.North)
				{
					curOffset = Offhand ? -compOversizedWeapon.Props.northOffset : compOversizedWeapon.Props.northOffset;
				}
				else if (curDir == Rot4.East)
				{
					curOffset = Offhand ? -compOversizedWeapon.Props.eastOffset : compOversizedWeapon.Props.eastOffset;
				}
				else if (curDir == Rot4.West)
				{
					curOffset = Offhand ? -compOversizedWeapon.Props.westOffset : compOversizedWeapon.Props.westOffset;
				}
			}

			return curOffset;
		}

	}
}
