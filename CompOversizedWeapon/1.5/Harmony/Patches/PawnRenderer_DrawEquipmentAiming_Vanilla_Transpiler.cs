using System;
using System.Collections.Generic;
using System.Linq;
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
    // OgsCompOversizedWeapon.GraphicData_Equippable
    public class GraphicData_Equippable : GraphicData
    {
        public GraphicData groundGraphic = null;
        public Offsets offsets;
        public bool verticalFlipOutsideCombat = false;
        public bool verticalFlipNorth = false;
        public bool isDualWeapon = false;
        public bool useAlienRacesDrawsize = false;
        public bool meleeMirrored = true;
        public bool rangedMirrored = true;

        public float OffsetAngleFor(Rot4 rot, bool offhand = false)
		{
            OffsetRotation atRot = offsets.AtRot(rot);
            float result = atRot.angleAdjustment;
            bool isOffhand = false;
            bool invert = false;
            if (offhand)
            {
                if (!atRot.angleAdjustmentOffhand.HasValue)
                {
                    invert = true;
                }
                else result = atRot.angleAdjustmentOffhand.Value;
            }
            else
            {
                result = atRot.angleAdjustment;
            }
            result = invert ? -result : result;
            return result;

        }
		public Vector3 OffsetPosFor(Rot4 rot, bool offhand = false)
        {
            Vector3 result = new Vector3(0, 0, 0);
            OffsetRotation atRot = offsets.AtRot(rot);
            bool invert = false;
            bool invert2 = false;

            Vector3 tmp =  atRot.offset;
            Vector3 tmp2 = offsets.offset;
            if (offhand)
            {
                if (!atRot.offsetOffhand.HasValue)
                {
                    invert = true;
                }
                else tmp = atRot.offsetOffhand.Value;
                if (!offsets.offsetOffhand.HasValue)
                {
                    invert2 = true;
                }
                else tmp2 = offsets.offsetOffhand.Value;
            }
            if (tmp != null)
            {
                result += new Vector3(invert ? -tmp.x : tmp.x, tmp.y, tmp.z);
            }
            if (tmp2 != null)
            {
                result += new Vector3(invert2 ? -tmp2.x : tmp2.x, tmp2.y, tmp2.z);
            }
            return result;

        }

        public class Offsets
        {
            public Vector3 offset = new Vector3(0, 0, 0);
            public Vector3? offsetOffhand;
            public Vector2 size;
            public Vector2 sizeOffhand;
            public OffsetRotation south;
            public OffsetRotation north;
            public OffsetRotation east;
            public OffsetRotation west;

			public OffsetRotation AtRot(Rot4 rot)
			{
				if (rot == Rot4.North)
				{
					return north;
				}
				if (rot == Rot4.South)
				{
					return south;
				}
				if (rot == Rot4.East)
				{
					return east;
				}
				if (rot == Rot4.West)
				{
					return west;
                }
                return null;
            }
        }

        public class OffsetRotation
        {
            public Vector3 offset;
            public Vector3? offsetOffhand;
            public Vector2 size;
            public Vector2? sizeOffhand;
            public bool canFlip = false;
            public bool canFlipOffhand = false;
            public float angleAdjustment = 0f;
            public float? angleAdjustmentOffhand;
        }


    }

	[HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
	public static class PawnRenderer_DrawEquipment_Vanilla_Transpiler
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            ConstructorInfo vec3Ctor = AccessTools.Constructor(typeof(Vector3), new Type[] { typeof(float), typeof(float), typeof(float) });
            FieldInfo pawnRendererPawn = AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn));
            FieldInfo pawnEquipment = AccessTools.Field(typeof(Pawn), nameof(Pawn.equipment));
        //    MethodInfo drawEquipmentAiming = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming));
            MethodInfo graphicsDrawMesh = AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int) });
            MethodInfo vec3RotatedBy = AccessTools.Method(type: typeof(Vector3Utility), name: nameof(Vector3Utility.RotatedBy), new Type[] { typeof(Vector3), typeof(float) });
            MethodInfo equipmentPrimary = AccessTools.PropertyGetter(type: typeof(Pawn_EquipmentTracker), name: nameof(Pawn_EquipmentTracker.Primary));
            int vec3CtorCount = 0;
            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                /*
                if (instruction.OperandIs(vec3Ctor))
                {
                    vec3CtorCount++;
                    if (vec3CtorCount > 1)
                    {
                        Log.Message($"{i}  opcode: {instruction.opcode} operand: {instruction.operand}");
                        yield return instruction;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipment_Vanilla_Transpiler), "Offset2", null, null));
                        if (vec3RotatedBy != null && i + 2 < list.Count-2 && list[i+2].OperandIs(vec3RotatedBy))
                        {
                            Log.Message($"{i}  Aiming!!");
                        }
                    }
                }
                */
                /*
                if (instruction.OperandIs(drawEquipmentAiming))
                {
                    yield return instruction;
                    yield return new CodeInstruction(list[i - 6].opcode);
                    yield return new CodeInstruction(list[i - 5].opcode, list[i - 5].operand);
                    yield return new CodeInstruction(list[i - 4].opcode, list[i - 4].operand);
                    yield return new CodeInstruction(list[i - 3].opcode, list[i - 3].operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(list[i - 1].opcode, list[i - 1].operand);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipment_Vanilla_Transpiler), "DrawEquipmentAiming", null, null));
                }
                */
                /*
				// modifies base position of rendered weapons before rotation
				if (instruction.opcode == OpCodes.Stloc_0)
                {

                    yield return new CodeInstruction(OpCodes.Stloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldloca_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipment_Vanilla_Transpiler), "Offset", null, null));

                }
				*/
                yield return instruction;
            }
        } 
        public static Vector3 Offset2(Vector3 vector, PawnRenderer instance)
        {
			Thing eq = instance.pawn.equipment.Primary;
            if (eq?.def.graphicData != null && eq?.def.graphicData is GraphicData_Equippable equippable)
            {
                return vector + equippable.OffsetPosFor(instance.pawn.Rotation);//.RotatedBy(aimAngle);
            }
            return vector;
        }
        
        public static void DrawEquipmentAiming(Thing eq, Vector3 rootLoc, float aimAngle, PawnRenderer instance, PawnRenderFlags flags)
        {
            if (eq?.def.graphicData != null && eq?.def.graphicData is GraphicData_Equippable equippable)
            {
                if (equippable.isDualWeapon)
                {
                    Vector3 vector = new Vector3(0f, (instance.pawn.Rotation == Rot4.North) ? -0.0028957527f : 0.03474903f, 0f);

                    Stance_Busy stance_Busy = instance.pawn.stances.curStance as Stance_Busy;
                    bool aiming = (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid && (flags & PawnRenderFlags.NeverAimWeapon) == PawnRenderFlags.None);

                    float equipmentDrawDistanceFactor = instance.pawn.ageTracker.CurLifeStage.equipmentDrawDistanceFactor;
                    float offHandAngle = aimAngle + equippable.OffsetAngleFor(instance.pawn.Rotation, true);
                    if (aiming)
                    {
                        vector += rootLoc + (new Vector3(0f, 0f, 0.4f + instance.pawn.equipment.Primary.def.equippedDistanceOffset) + equippable.OffsetPosFor(instance.pawn.Rotation, true)).RotatedBy(aimAngle) * equipmentDrawDistanceFactor;
                    }
                    else
                    {
                        if (instance.pawn.Rotation == Rot4.South)
                        {
                            vector += rootLoc + new Vector3(0f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                        }
                        if (instance.pawn.Rotation == Rot4.North)
                        {
                            vector += rootLoc + new Vector3(0f, 0f, -0.11f) * equipmentDrawDistanceFactor;
                        }
                        if (instance.pawn.Rotation == Rot4.East)
                        {
                            vector += rootLoc + new Vector3(0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                        }
                        if (instance.pawn.Rotation == Rot4.West)
                        {
                            vector += rootLoc + new Vector3(-0.2f, 0f, -0.22f) * equipmentDrawDistanceFactor;
                        }
                        vector += equippable.OffsetPosFor(instance.pawn.Rotation, true);
                    }
                //    instance.DrawEquipmentAiming(eq, vector, aimAngle + offHandAngle);
                }

            }
        }

        public static Vector3 Offset(Vector3 vector, PawnRenderer instance, ref float aimAngle)
        {
			Thing eq = instance.pawn.equipment.Primary;
            if (eq?.def.graphicData != null && eq?.def.graphicData is GraphicData_Equippable equippable)
            {
                Vector3 offsetMainHand = default(Vector3);
                Vector3 offsetOffHand = default(Vector3);
                Stance_Busy stance_Busy = instance.pawn.stances.curStance as Stance_Busy;
                bool Aiming = OversizedUtil.CurrentlyAiming(stance_Busy);
                if (equippable.isDualWeapon)
				{
                    float offHandAngle = aimAngle + equippable.OffsetAngleFor(instance.pawn.Rotation, true);
                    Vector3 vector2 = vector + equippable.OffsetPosFor(instance.pawn.Rotation, true).RotatedBy(offHandAngle);
                //    instance.DrawEquipmentAiming(eq, vector2, offHandAngle);
                }
                aimAngle += equippable.OffsetAngleFor(instance.pawn.Rotation);
                return vector + equippable.OffsetPosFor(instance.pawn.Rotation);//.RotatedBy(aimAngle);
            }
            return vector;
        }


        public static void SetAnglesAndOffsets(GraphicData_Equippable Preps, Thing thing, ref Vector3 offsetMainHand, ref Vector3 offsetOffHand, ref float mainHandAngle, ref float offHandAngle, bool mainHandAiming, bool offHandAiming)
        {
            Pawn pawn = thing as Pawn;
            bool Melee = pawn != null;
            if (Melee)
            {
                Melee = OversizedUtil.IsMeleeWeapon(pawn.equipment.Primary);
            }

            bool Dual = false;
            if (Preps != null)
            {
                Dual = Preps.isDualWeapon;
            }
            float num = Preps.meleeMirrored ? (360f - OversizedUtil.meleeAngle) : OversizedUtil.meleeAngle;
            float num2 = Preps.rangedMirrored ? (360f - OversizedUtil.rangedAngle) : OversizedUtil.rangedAngle;
            offsetMainHand = Preps.OffsetPosFor(thing.Rotation);
            offsetOffHand = Preps.OffsetPosFor(thing.Rotation, offHandAiming);
            if (Preps != null)
            {
                mainHandAngle += Preps.OffsetAngleFor(thing.Rotation);
                offHandAngle += Preps.OffsetAngleFor(thing.Rotation, offHandAiming);
            }
            if (thing.Rotation == Rot4.East)
            {
                offsetOffHand.y = -1f;
                offsetOffHand.z = 0.1f;
            }
            else
            {
                if (thing.Rotation == Rot4.West)
                {
                    if (Dual) offsetMainHand.y = -1f;
                    offsetOffHand.z = -0.1f;
                }
                else
                {
                    if (thing.Rotation == Rot4.North)
                    {
                        if (!mainHandAiming)
                        {
                            offsetMainHand.x += (Dual ? (Melee ? OversizedUtil.meleeXOffset : OversizedUtil.rangedXOffset) : 0);
                            offsetOffHand.x += -(Melee ? -OversizedUtil.meleeXOffset : -OversizedUtil.rangedXOffset);
                            offsetMainHand.z += (Dual ? (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset) : 0);
                            offsetOffHand.z += (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset);
                            if (Preps != null)
                            {
                                offHandAngle += (Melee ? OversizedUtil.meleeAngle : OversizedUtil.rangedAngle);
                                mainHandAngle += -(Melee ? num : num2);
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
                            offsetMainHand.x += -(Dual ? (Melee ? -OversizedUtil.meleeXOffset : -OversizedUtil.rangedXOffset) : 0);
                            offsetOffHand.x += (Melee ? OversizedUtil.meleeXOffset : OversizedUtil.rangedXOffset);
                            offsetMainHand.z += (Dual ? (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset) : 0);
                            offsetOffHand.z += (Melee ? OversizedUtil.meleeZOffset : OversizedUtil.rangedZOffset);
                            if (Preps != null)
                            {
                                offHandAngle += -(Melee ? num : num2);
                                mainHandAngle += (Melee ? OversizedUtil.meleeAngle : OversizedUtil.rangedAngle);
                            }
                        }
                        else
                        {
                            offsetOffHand.y = 1f;
                            offHandAngle += (!Melee ? num : num2);
                            offsetOffHand.x = 0.1f;
                        }
                    }
                }
            }
        }

    }

    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    public static class PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler
    {
        public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "CETeam.CombatExtended");
        public static bool enabled_YayosCombat = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "com.yayo.combat3");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            List<CodeInstruction> list = instructions.ToList<CodeInstruction>();
            ConstructorInfo vec3Ctor = AccessTools.Constructor(type: typeof(Vector3), new Type[] { typeof(float), typeof(float), typeof(float) });
            MethodInfo matrixTRS = AccessTools.Method(type: typeof(Matrix4x4), name: nameof(Matrix4x4.TRS));
            MethodInfo graphicsDrawMesh = AccessTools.Method(type: typeof(Graphics), name: nameof(Graphics.DrawMesh), new Type[] { typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int) });
            FieldInfo pawnRendererPawn = AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn));
            FieldInfo meshflipped = AccessTools.Field(typeof(MeshPool), nameof(MeshPool.plane10Flip));
            FieldInfo offhand = AccessTools.Field(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), nameof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler.offhand));
            MethodInfo offhandEq = AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), nameof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler.DrawOffhandEquipmentAiming));
            MethodInfo label = AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), nameof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler.label));
            FieldInfo equippedAngleOffset = AccessTools.Field(typeof(ThingDef), nameof(ThingDef.equippedAngleOffset));
            FieldInfo thingDef = AccessTools.Field(typeof(Thing), nameof(Thing.def));
            FieldInfo graphicData = AccessTools.Field(typeof(ThingDef), nameof(ThingDef.graphicData));
            FieldInfo isDualWeapon = AccessTools.Field(typeof(GraphicData_Equippable), nameof(GraphicData_Equippable.isDualWeapon));


            Label postOffhand = il.DefineLabel();

            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                // weapon size tweak
                if (instruction.OperandIs(matrixTRS))
                {
                    if (Prefs.DevMode) Log.Message($"{i}  opcode: {instruction.opcode} operand: {instruction.operand} Weapon Matrix tweak");
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    instruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PawnRenderer_DrawEquipmentAiming_Vanilla_Transpiler), nameof(GetMatrix), null, null));
                }

                if (instruction.OperandIs(graphicsDrawMesh))
                {
                    yield return instruction;
                    // need to wrap these in an if block
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, thingDef);
                    yield return new CodeInstruction(OpCodes.Ldfld, graphicData);
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(GraphicData_Equippable));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, postOffhand);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, thingDef);
                    yield return new CodeInstruction(OpCodes.Ldfld, graphicData);
                    yield return new CodeInstruction(OpCodes.Castclass, typeof(GraphicData_Equippable));
                    yield return new CodeInstruction(OpCodes.Ldfld, isDualWeapon);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, postOffhand);
                    // modify values for second weapon
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                    yield return new CodeInstruction(OpCodes.Call, offhandEq);
                    // draw second weapon
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Call, graphicsDrawMesh);
                    instruction = new CodeInstruction(OpCodes.Call, label).WithLabels(postOffhand);
                }
                yield return instruction;
            }
        }

        public static void label()
        {

        }

        public static Mesh GetMesh(Mesh mesh, Thing eq)
        {
			if (eq.def.graphicData is GraphicData_Equippable equippable && equippable.isDualWeapon)
            {
                if (offhand)
                {
                    mesh = mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
                    Log.Message($"render offhand for {eq} base mesh {mesh}");
                }
                else
                {
                    Log.Message($"render mainhand for {eq} base mesh {mesh}");
                }
            }
            return mesh;

        }
        public static Matrix4x4 GetMatrix(Vector3 pos, Quaternion q, Vector3 size, PawnRenderer instance, Thing eq, float num, float aimAngle)
        {
            Pawn pawn = instance.pawn;
			if (pawn == null)
            {
            //   Log.Message("pawn is null");
                return Matrix4x4.TRS(pos,q,size);
            }
            float angle = num;
            if (eq.def.graphicData != null && eq.def.graphicData is GraphicData_Equippable equippable)
            {
                if (equippable.isDualWeapon)
                {
                    Stance_Busy stance_Busy = instance.pawn.stances.curStance as Stance_Busy;
                    bool aiming = (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid);
                //    aimAngle = aimAngle + equippable.OffsetAngleFor(instance.pawn.Rotation, true);
                    if (aiming)
                    {
                        pos += equippable.OffsetPosFor(instance.pawn.Rotation, false).RotatedBy(aimAngle);
                    }
                    else
                    {
                        pos += equippable.OffsetPosFor(instance.pawn.Rotation, false);
                    }
                }
                float am = pawn.Rotation != Rot4.North ? equippable.OffsetAngleFor(pawn.Rotation) : -equippable.OffsetAngleFor(pawn.Rotation);
            //    Log.Message($"check for angle modification {am}");
                angle += am;
                angle %= 360f;
            }
            Vector3 s;
            if (pawn.RaceProps.Humanlike)
            {
                if (HarmonyPatches_OversizedWeapon.enabled_AlienRaces)
                {
                    Vector2 v = AlienRaceUtility.AlienRacesPatch(pawn, eq);
                    float f = Mathf.Max(v.x, v.y);
                    s = new Vector3(size.x * f, size.y, size.z * f);
                }
                else
                {
                    s = size;
                }
            }
            else
            {
                Vector2 v = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                s = new Vector3(size.x + v.x / 10, size.y, size.z + v.y / 10);
            }
        //    Log.Message($"Matrix4x4 MH is {pos} aimAngle({aimAngle}) AngleAxis({num},{angle}) {s}");
            return Matrix4x4.TRS(pos, Quaternion.AngleAxis(angle, Vector3.up), s);
        }

        public static void DrawOffhandEquipmentAiming(PawnRenderer instance, Thing eq, Vector3 drawLoc, float aimAngle, CompEquippable compEquippable, 
            ref Mesh mesh, ref Matrix4x4 matrix, float num, Vector3 s)
        {
            Pawn pawn = instance.pawn;
            Stance_Busy stance_Busy = instance.pawn.stances.curStance as Stance_Busy;
            bool aiming = (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid);
            GraphicData_Equippable equippable = eq?.def.graphicData as GraphicData_Equippable;
            
            num = (pawn.Rotation == Rot4.South || pawn.Rotation == Rot4.North ? -aimAngle : aimAngle) + (pawn.Rotation != Rot4.East ? 90f : -90f);
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh =  mesh == MeshPool.plane10 && pawn.Rotation == Rot4.South || pawn.Rotation == Rot4.North ? MeshPool.plane10Flip : MeshPool.plane10;
                num -= eq.def.equippedAngleOffset + equippable.OffsetAngleFor(pawn.Rotation, true);
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
            //    mesh = mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
            //    num += 180f;
                num -= eq.def.equippedAngleOffset + equippable.OffsetAngleFor(pawn.Rotation, true);
            }
            else
            {
                if (aiming) mesh = mesh == MeshPool.plane10 ? MeshPool.plane10Flip : MeshPool.plane10;
                num += eq.def.equippedAngleOffset + equippable.OffsetAngleFor(pawn.Rotation, true);
            }
            num %= 360f;

            if (compEquippable != null)
            {
                Vector3 b;
                float num2;
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out b, out num2, aimAngle);
                drawLoc += b;
                num += num2;
            }
            /*
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material material;
            if (graphic_StackCount != null)
            {
                material = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq);
            }
            else
            {
                material = eq.Graphic.MatSingleFor(eq);
            }
            */
            float angle = num;
            float am =  equippable.OffsetAngleFor(pawn.Rotation, true);

            if (aiming)
            {
                drawLoc += equippable.OffsetPosFor(instance.pawn.Rotation, true).RotatedBy(aimAngle);
            }
            else
            {
                drawLoc += equippable.OffsetPosFor(instance.pawn.Rotation, true);
            }

            
        //    Log.Message($"check for angleOffhand modification {am}");
            angle += am;
            angle %= 360f;
        //    Log.Message($"Matrix4x4 OH is {drawLoc} aimAngle({aimAngle}) AngleAxis({num},{angle}) {s}");
            num = angle;
            matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(angle, Vector3.up), s);
        //    Graphics.DrawMesh(mesh, matrix, material, 0);
        }


        public static bool offhand;


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
            CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
            if (compEquippable != null)
            {
                Vector3 b;
                float num2;
                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out b, out num2, aimAngle);
                drawLoc += b;
                num += num2;
            }
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material material;
            if (graphic_StackCount != null)
            {
                material = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq);
            }
            else
            {
                material = eq.Graphic.MatSingleFor(eq);
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
            Matrix4x4 matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), s);
            OversizedUtil.Draw(mesh, matrix, material, 0);
		}

	}
}
