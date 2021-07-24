using DualWield;
using DualWield.Storage;
using HarmonyLib;
using OgsCompOversizedWeapon.ExtentionMethods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace OgsCompOversizedWeapon
{
    // OgsCompOversizedWeapon.OversizedUtil.Draw
    [StaticConstructorOnStartup]
    public static class OversizedUtil
    {

        public static void Draw(Mesh mesh, Matrix4x4 matrix, Material mat, int layer, Thing eq, Pawn pawn, Vector3 position, Quaternion rotation)
        {
            if (matrix == default(Matrix4x4))
            {
                Graphics.DrawMesh(mesh, position, rotation, mat, layer);
            }
            else Graphics.DrawMesh(mesh, matrix, mat, layer);
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
			CompOversizedWeapon compOversized = eq.TryGetCompFast<CompOversizedWeapon>();

			CompProperties_OversizedWeapon PropsOversized = compOversized.Props;

			Pawn pawn = thing as Pawn;

			bool Melee = pawn != null;
			if (Melee)
			{
				Melee = IsMeleeWeapon(pawn.equipment.Primary);
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

		public static float GetAimingRotation(Pawn pawn, LocalTargetInfo focusTarg)
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

		public static bool CurrentlyAiming(Stance_Busy stance)
		{
			return stance != null && !stance.neverAimWeapon && stance.focusTarg.IsValid;
		}

		public static bool IsMeleeWeapon(ThingWithComps eq)
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

		public static Vector3 AdjustRenderOffsetFromDir(Rot4 curDir, CompOversizedWeapon compOversizedWeapon, bool Offhand = false)
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


		#region DualWield
		public static void AddOffHandEquipment(this Pawn_EquipmentTracker instance, ThingWithComps newEq)
        {

			ThingOwner<ThingWithComps> value; // = Traverse.Create(instance).Field("equipment").GetValue<ThingOwner<ThingWithComps>>();
			value = instance.equipment;

			ExtendedDataStorage extendedDataStorage = Base.Instance.GetExtendedDataStorage();
            bool flag = extendedDataStorage != null;
            if (flag)
            {
                extendedDataStorage.GetExtendedDataFor(newEq).isOffHand = true;
                LessonAutoActivator.TeachOpportunity(DW_DefOff.DW_Penalties, 0);
                LessonAutoActivator.TeachOpportunity(DW_DefOff.DW_Settings, 0);
                value.TryAdd(newEq, true);
            }
        }

        public static bool TryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
        {
            result = null;
            bool flag = instance.pawn.HasMissingArmOrHand();
            bool result2;
            if (flag)
            {
                result2 = false;
            }
            else
            {
                ExtendedDataStorage extendedDataStorage = Base.Instance.GetExtendedDataStorage();
                foreach (ThingWithComps thingWithComps in instance.AllEquipmentListForReading)
                {
                    ExtendedThingWithCompsData extendedThingWithCompsData;
                    bool flag2 = extendedDataStorage.TryGetExtendedDataFor(thingWithComps, out extendedThingWithCompsData) && extendedThingWithCompsData.isOffHand;
                    if (flag2)
                    {
                        result = thingWithComps;
                        return true;
                    }
                }
                result2 = false;
            }
            return result2;
        }
        #endregion

    }
}
