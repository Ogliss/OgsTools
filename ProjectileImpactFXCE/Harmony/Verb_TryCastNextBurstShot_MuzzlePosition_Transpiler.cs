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
using OgsCompOversizedWeapon;

namespace ProjectileImpactFX.HarmonyInstance
{

    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    public static class Verb_TryCastNextBurstShot_MuzzlePosition_Transpiler
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();

			Type type = AccessTools.TypeByName("OgsCompOversizedWeapon.CompOversizedWeapon");
			Oversized = type != null;
			for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                if (instruction.OperandIs(AccessTools.Method(typeof(MoteMaker), "MakeStaticMote", new[] { typeof(IntVec3), typeof(Map), typeof(ThingDef), typeof(float) })))
                {
                    //    Log.Message(i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    instruction.operand = AccessTools.Method(typeof(Verb_TryCastNextBurstShot_MuzzlePosition_Transpiler), "ThrowMuzzleFlash", null, null);
                    //    Log.Message(i + " opcode: " + instruction.opcode + " operand: " + instruction.operand);
                }
                yield return instruction;
            }
        }

		public static bool Oversized;
        public static void ThrowMuzzleFlash(IntVec3 cell, Map map, ThingDef moteDef, float scale, Verb verb)
        {
            if (verb.EquipmentSource != null)
            {
                if (verb.verbProps.range > 1.48f)
                {
                    ThingDef mote = moteDef;
                    Vector3 origin = verb.CasterIsPawn ? verb.CasterPawn.Drawer.DrawPos : verb.Caster.DrawPos;
                    CompEquippable equippable = verb.EquipmentSource.TryGetComp<CompEquippable>();
                    float aimAngle = (verb.CurrentTarget.CenterVector3 - origin).AngleFlat();
					if (Oversized)
					{
						OversizedWeapon(verb.EquipmentSource, aimAngle, verb, ref origin);
					}
                    if (verb.EquipmentSource.def.HasModExtension<BarrelOffsetExtension>())
                    {
                        BarrelOffsetExtension ext = verb.EquipmentSource.def.GetModExtension<BarrelOffsetExtension>();
                        EffectProjectileExtension ext2 = verb.GetProjectile().HasModExtension<EffectProjectileExtension>() ? verb.GetProjectile().GetModExtension<EffectProjectileExtension>() : null;
                        float offset = ext.barrellength;
                        origin += (verb.CurrentTarget.CenterVector3 - origin).normalized * (verb.EquipmentSource.def.graphic.drawSize.magnitude * (offset));
                        if (ext2 != null && ext2.muzzleFlare)
                        {
                            ThingDef muzzleFlaremote = DefDatabase<ThingDef>.GetNamed(!ext2.muzzleSmokeDef.NullOrEmpty() ? ext2.muzzleFlareDef : "Mote_SparkFlash");
                            MoteMaker.MakeStaticMote(origin, map, muzzleFlaremote, ext2.muzzleFlareSize);
                        }
                        else if (ext.muzzleFlare)
                        {
                            ThingDef muzzleFlaremote = DefDatabase<ThingDef>.GetNamed(!ext.muzzleSmokeDef.NullOrEmpty() ? ext.muzzleFlareDef : "Mote_SparkFlash");
                            MoteMaker.MakeStaticMote(origin, map, muzzleFlaremote, ext.muzzleFlareSize);
                        }
                        if (ext2 != null && ext2.muzzleSmoke)
                        {
                            string muzzleSmokemote = !ext2.muzzleSmokeDef.NullOrEmpty() ? ext2.muzzleSmokeDef : "OG_Mote_SmokeTrail";
                            TrailThrower.ThrowSmoke(origin, ext2.muzzleSmokeSize, map, muzzleSmokemote);
                        }
                        else if (ext.muzzleSmoke)
                        {
                            string muzzleSmokemote = !ext.muzzleSmokeDef.NullOrEmpty() ? ext.muzzleSmokeDef : "OG_Mote_SmokeTrail";
                            TrailThrower.ThrowSmoke(origin, ext.muzzleSmokeSize, map, muzzleSmokemote);
                        }
                    }
                    MoteMaker.MakeStaticMote(origin, map, mote, scale);
                    return;
                }
            }

            {
                MoteMaker.MakeStaticMote(cell.ToVector3Shifted(), map, moteDef, scale);
            }
		}

		public static void OversizedWeapon(ThingWithComps weapon, float aimAngle, Verb verb, ref Vector3 origin)
		{
			OgsCompOversizedWeapon.CompOversizedWeapon compOversized = weapon.TryGetComp<OgsCompOversizedWeapon.CompOversizedWeapon>();
			if (compOversized != null)
			{
				bool DualWeapon = compOversized.Props != null && compOversized.Props.isDualWeapon;
				Vector3 offsetMainHand = default(Vector3);
				Vector3 offsetOffHand = default(Vector3);
				float offHandAngle = aimAngle;
				float mainHandAngle = aimAngle;
				Harmony_PawnRenderer_DrawEquipmentAiming_Transpiler.SetAnglesAndOffsets(compOversized.parent, compOversized.parent, aimAngle, verb.Caster, ref offsetMainHand, ref offsetOffHand, ref offHandAngle, ref mainHandAngle, true, DualWeapon && !compOversized.FirstAttack);
				if (DualWeapon) Log.Message("Throwing flash for " + compOversized.parent.LabelCap + " offsetMainHand: " + offsetMainHand + " offsetOffHand: " + offsetOffHand + " Using " + (!compOversized.FirstAttack ? "OffHand" : "MainHand") + " FirstAttack: " + compOversized.FirstAttack);
				origin += DualWeapon && !compOversized.FirstAttack ? offsetOffHand : offsetMainHand;
				// origin += compOversized.AdjustRenderOffsetFromDir(equippable.PrimaryVerb.CasterPawn, !compOversized.FirstAttack);
				if (compOversized.Props.isDualWeapon) compOversized.FirstAttack = !compOversized.FirstAttack;
			}
		}

		public static float meleeXOffset = 0.4f;
		public static float rangedXOffset = 0.1f;
		public static float meleeZOffset = 0f;
		public static float rangedZOffset = 0f;
		public static float meleeAngle = 270f;
		public static bool meleeMirrored = true;
		public static float rangedAngle = 135f;
		public static bool rangedMirrored = true;

		public static void SetAnglesAndOffsets(Thing eq, ThingWithComps offHandEquip, float aimAngle, Thing thing, ref Vector3 offsetMainHand, ref Vector3 offsetOffHand, ref float mainHandAngle, ref float offHandAngle, bool mainHandAiming, bool offHandAiming)
		{
			CompOversizedWeapon compOversized = eq.TryGetComp<CompOversizedWeapon>();

			CompProperties_OversizedWeapon PropsOversized = compOversized.Props;

			Pawn pawn = thing as Pawn;

			bool Melee = pawn != null;
			if (Melee)
			{
				Melee = Verb_TryCastNextBurstShot_MuzzlePosition_Transpiler.IsMeleeWeapon(pawn.equipment.Primary);
			}

			bool Dual = false;
			if (PropsOversized != null)
			{
				Dual = PropsOversized.isDualWeapon;
			}
			float num = meleeMirrored ? (360f - meleeAngle) : meleeAngle;
			float num2 = rangedMirrored ? (360f - rangedAngle) : rangedAngle;

			Vector3 offset = Vector3.zero;

			if (compOversized?.Props != null)
			{

				offset = offHandAiming ? -compOversized.Props.southOffset : compOversized.Props.southOffset;
				if (thing.Rotation == Rot4.North)
				{
					offset = offHandAiming ? -compOversized.Props.northOffset : compOversized.Props.northOffset;
				}
				else if (thing.Rotation == Rot4.East)
				{
					offset = offHandAiming ? -compOversized.Props.eastOffset : compOversized.Props.eastOffset;
				}
				else if (thing.Rotation == Rot4.West)
				{
					offset = offHandAiming ? -compOversized.Props.westOffset : compOversized.Props.westOffset;
				}
			}
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
				CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
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
	}

}
