using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using HunterMarkingSystem.Settings;
using HunterMarkingSystem.ExtensionMethods;

namespace HunterMarkingSystem
{
    // Hediff_Implant Drawer
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(RotDrawMode), typeof(PawnRenderFlags) })]
    static class HMS_PawnRenderer_RenderPawnInternal_Patch
    {
        static void Prefix(PawnRenderer __instance, Pawn ___pawn, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        {
            bool portrait = flags.FlagSet(PawnRenderFlags.Portrait);
            bool headStump = flags.FlagSet(PawnRenderFlags.HeadStump);
            bool invisible = flags.FlagSet(PawnRenderFlags.Invisible);
            if (invisible)
            {
                return;
            }
            Pawn pawn = ___pawn;
            if (pawn!=null)
            {
                Comp_Markable markable = pawn.TryGetComp<Comp_Markable>();
                if (markable!=null)
                {
                    Hediff hd = pawn.health.hediffSet.hediffs.Find(x => x.TryGetComp<HediffComp_HunterMark>() != null);
                    if (hd != null)
                    {
                        HediffComp_HunterMark comp = hd.TryGetComp<HediffComp_HunterMark>();
                        if (comp.BloodStatus > HMSUtility.BloodStatusMode.Unblooded)
                        {
                            if (comp != null && comp.hasMat && !portrait)
                            {
                                DrawMark(comp, pawn, __instance, rootLoc, angle, renderBody, bodyFacing, bodyFacing, bodyDrawType, portrait, headStump);
                            }
                        }
                    }
                }
            }
        }

        static void DrawMark(HediffComp_HunterMark comp, Pawn pawn, PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            bool selected = Find.Selector.SelectedObjects.Contains(pawn) && Prefs.DevMode;
            Rot4 rot = bodyFacing;
            Vector3 vector3 = pawn.RaceProps.Humanlike ? __instance.BaseHeadOffsetAt(headFacing) : new Vector3();
            Vector3 s = new Vector3(pawn.BodySize * 1.75f, pawn.BodySize * 1.75f, pawn.BodySize * 1.75f);
            bool hasdefext = pawn.def.HasModExtension<MarkOffsetDefExtension>();
            if (hasdefext)
            {
                MarkOffsetDefExtension defExtension = pawn.def.GetModExtension<MarkOffsetDefExtension>() ?? new MarkOffsetDefExtension();// ?? ThingDefOf.Human.GetModExtension<MarkOffsetDefExtension>();
                if (defExtension != null)
                {
                    GetAltitudeOffset(pawn, defExtension, comp.parent, rot, out float X, out float Y, out float Z, out float DsX, out float DsZ, out float ang);
                    vector3.x += X;
                    vector3.y += Y;
                    vector3.z += Z;
                    angle += ang;
                    s.x = DsX;
                    s.z = DsZ;

                }
            }
            else
            {
                if (pawn.RaceProps.Humanlike)
                {
                    vector3.z += 0.25f;
                }
            }
            if (pawn.RaceProps.Humanlike)
            {
                vector3.x += 0.01f;
                vector3.z += -0.35f;
            }
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 b = quaternion * vector3;
            Vector3 vector = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.02734375f;
                vector.y += 0.0234375f;
            }
            else
            {
                a.y += 0.0234375f;
                vector.y += 0.02734375f;
            }
            Vector3 loc2 = rootLoc + b;
            loc2.y += 0.03105f;
            bool flag = false;
            if (!flag && bodyDrawType != RotDrawMode.Dessicated)
            {
                //    Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                Material mat = comp.ImplantMaterial(pawn, pawn.RaceProps.Humanlike ? headFacing : bodyFacing);
                //    GenDraw.DrawMeshNowOrLater(headFacing == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, loc2, quaternion, mat, true);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(loc2, quaternion, s);
                Graphics.DrawMesh((pawn.RaceProps.Humanlike ? headFacing : bodyFacing) == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix, mat, 0);
            }
        }

        static void GetAltitudeOffset(Pawn pawn, MarkOffsetDefExtension defExtension, Hediff hediff, Rot4 rotation, out float OffsetX, out float OffsetY, out float OffsetZ, out float DrawSizeX, out float DrawSizeZ, out float ang)
        {
            MarkOffsetDefExtension myDef = defExtension;
            if (pawn.RaceProps.Humanlike)
            {
                if (rotation == Rot4.North)
                {
                    OffsetX = myDef.NorthXOffset;
                    OffsetY = myDef.NorthYOffset;
                    OffsetZ = myDef.NorthZOffset;
                    DrawSizeX = myDef.NorthXDrawSize;
                    DrawSizeZ = myDef.NorthZDrawSize;
                    ang = myDef.NorthAngle;
                }
                else if (rotation == Rot4.West)
                {
                    OffsetX = myDef.WestXOffset;
                    OffsetY = myDef.WestYOffset;
                    OffsetZ = myDef.WestZOffset;
                    DrawSizeX = myDef.WestXDrawSize;
                    DrawSizeZ = myDef.WestZDrawSize;
                    ang = myDef.WestAngle;
                }
                else if (rotation == Rot4.East)
                {
                    OffsetX = myDef.EastXOffset;
                    OffsetY = myDef.EastYOffset;
                    OffsetZ = myDef.EastZOffset;
                    DrawSizeX = myDef.EastXDrawSize;
                    DrawSizeZ = myDef.EastZDrawSize;
                    ang = myDef.EastAngle;
                }
                else if (rotation == Rot4.South)
                {
                    OffsetX = myDef.SouthXOffset;
                    OffsetY = myDef.SouthYOffset;
                    OffsetZ = myDef.SouthZOffset;
                    DrawSizeX = myDef.SouthXDrawSize;
                    DrawSizeZ = myDef.SouthZDrawSize;
                    ang = myDef.SouthAngle;
                }
                else
                {
                    OffsetX = 0f;
                    OffsetY = 0f;
                    OffsetZ = 0f;
                    DrawSizeX = 1f;
                    DrawSizeZ = 1f;
                    ang = 0f;
                }
                if (myDef.ApplyBaseHeadOffset)
                {
                    OffsetX = myDef.SouthXOffset + pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).x;
                    OffsetY = myDef.SouthYOffset + pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).y;
                    OffsetZ = myDef.SouthZOffset + pawn.Drawer.renderer.BaseHeadOffsetAt(rotation).z;
                }
            }
            else
            {
                if (rotation == Rot4.North)
                {
                    OffsetX = myDef.NorthXOffset;
                    OffsetY = myDef.NorthYOffset;
                    OffsetZ = myDef.NorthZOffset;
                    DrawSizeX = myDef.NorthXDrawSize;
                    DrawSizeZ = myDef.NorthZDrawSize;
                    ang = myDef.NorthAngle;
                }
                else if (rotation == Rot4.West)
                {
                    OffsetX = myDef.WestXOffset;
                    OffsetY = myDef.WestYOffset;
                    OffsetZ = myDef.WestZOffset;
                    DrawSizeX = myDef.WestXDrawSize;
                    DrawSizeZ = myDef.WestZDrawSize;
                    ang = myDef.WestAngle;
                }
                else if (rotation == Rot4.East)
                {
                    OffsetX = myDef.EastXOffset;
                    OffsetY = myDef.EastYOffset;
                    OffsetZ = myDef.EastZOffset;
                    DrawSizeX = myDef.EastXDrawSize;
                    DrawSizeZ = myDef.EastZDrawSize;
                    ang = myDef.EastAngle;
                }
                else if (rotation == Rot4.South)
                {
                    OffsetX = myDef.SouthXOffset;
                    OffsetY = myDef.SouthYOffset;
                    OffsetZ = myDef.SouthZOffset;
                    DrawSizeX = myDef.SouthXDrawSize;
                    DrawSizeZ = myDef.SouthZDrawSize;
                    ang = myDef.SouthAngle;
                }
                else
                {
                    OffsetX = 0f;
                    OffsetY = 0f;
                    OffsetZ = 0f;
                    DrawSizeX = 1f;
                    DrawSizeZ = 1f;
                    ang = 0f;
                }
            }
        }
    }

}