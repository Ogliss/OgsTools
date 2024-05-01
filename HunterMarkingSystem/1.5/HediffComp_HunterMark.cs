using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem
{
    public class HediffCompProperties_HunterMark : HediffCompProperties
    {
        public HediffCompProperties_HunterMark()
        {
            this.compClass = typeof(HediffComp_HunterMark);
        }
        public MarkDrawerType markDrawerType;
        public string markGraphicPath;
        public Shader shader = ShaderDatabase.Cutout;
        public Vector2 drawSize = Vector2.one;
        public Color color = Color.white;
        public Color colorTwo = Color.white;
    }

    [StaticConstructorOnStartup]
    public class HediffComp_HunterMark : HediffComp
    {
        public HediffCompProperties_HunterMark Props
		{
			get
			{
				return (HediffCompProperties_HunterMark)this.props;
			}
        }
        public Comp_Markable Markable => Pawn.TryGetComp<Comp_Markable>();
        public MarkData markData
        {
            get
            {
                if (Def.defName.Contains(HunterMarkingSystem.Markedkey))
                {
                    return Markable.markDataKill;
                }
                else
                {
                    return Markable.markDataKillNew;
                }
            }
        }
        public BloodStatusMode BloodStatus => this.Markable != null ? this.Markable.BloodStatus : BloodStatusMode.NoComp;
        public Corpse corpse => this.Markable != null ? this.Markable.Markcorpse : null;
        public Pawn pawn => this.Markable != null ? this.Markable.Mark : null;
        public PawnKindDef pawnKindDef => Markable.markDataKill.kindDef ?? null;
        public string MarkHedifftype;
        public override bool CompShouldRemove => base.CompShouldRemove;
        /*
        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (pawnKindDef != null)
                {
                    return pawnKindDef.LabelCap;
                }
                return null;
            }
        }
        */
        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (markData != null)
                {
                    if (BloodStatus>=BloodStatusMode.Unmarked)
                    {
                        return markData.Humanlike ? markData.Name + " (" + markData.Label + ")" : markData.Label;
                    }
                }
                else
                {
                    return "Unblooded";
                }
                return null;
            }
        }

        public override string CompTipStringExtra => base.CompTipStringExtra;

        public override string CompDebugString()
        {
            return Markable.MarkScore.ToString();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
        }


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            /*
            try
            {
                UpdateGraphicsFor(Pawn);
            }
            catch (Exception)
            {

            }
            */
        }

        public static void UpdateGraphicsFor(Pawn pawn)
        {
            if (pawn != null)
            {
                pawn.drawer.renderer.renderTree.SetDirty();
                PortraitsCache.SetDirty(pawn);
                PortraitsCache.Clear();
                PortraitsCache.PortraitsCacheUpdate();
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

        }

        public bool hasMat => !Props.markGraphicPath.NullOrEmpty();
        public Material ImplantMaterial(Pawn pawn, Rot4 bodyFacing)
        {
            string path;
            if (this.Props.markDrawerType != MarkDrawerType.Body)
            {
                path = Props.markGraphicPath;
            }
            else
            {
                path = Props.markGraphicPath + "_" + pawn.story.bodyType.ToString();
            }
            return GraphicDatabase.Get<Graphic_Multi>(path, Props.shader, Props.drawSize, Props.color, Props.colorTwo).MatAt(bodyFacing);
        }

        public void DrawMark(HediffComp_HunterMark comp, Pawn pawn, PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, PawnRenderFlags flags)
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
                if (selected) Log.Message($"RenderPawnInternal drawing Mark {comp.parent.Label} for {pawn.Name} rootLoc:{rootLoc}, angle:{angle}, renderBody:{renderBody}, bodyFacing:{bodyFacing.ToStringHuman()}, headFacing:{headFacing.ToStringHuman()}, bodyDrawType:{bodyDrawType}, portrait:{portrait}");
                //    Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                Material mat = comp.ImplantMaterial(pawn, pawn.RaceProps.Humanlike ? headFacing : bodyFacing);
                //    GenDraw.DrawMeshNowOrLater(headFacing == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, loc2, quaternion, mat, true);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(loc2, quaternion, s);
                GenDraw.DrawMeshNowOrLater((pawn.RaceProps.Humanlike ? headFacing : bodyFacing) == Rot4.West ? MeshPool.plane10Flip : MeshPool.plane10, matrix, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
            }
        }

        public void GetAltitudeOffset(Pawn pawn, MarkOffsetDefExtension defExtension, Hediff hediff, Rot4 rotation, out float OffsetX, out float OffsetY, out float OffsetZ, out float DrawSizeX, out float DrawSizeZ, out float ang)
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



    public enum MarkDrawerType
    {
        Undefined,
        Body,
        Head
    }
}
