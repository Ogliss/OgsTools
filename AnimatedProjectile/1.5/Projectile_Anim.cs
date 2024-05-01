using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace AnimatedProjectile
{
    public class Projectile_Anim : Projectile
    {
        public AnimatedProjectileProperties Props => this.def.projectile as AnimatedProjectileProperties;

        public float Drawsize => Props.growerStartSize == Props.growerEndSize ? (Props.growerEndSize * (this.Props.growerDistance ? Traveled : 1)) : Mathf.Lerp(Props.growerStartSize, Props.growerEndSize, (this.Props.growerDistance ? Traveled : 1));
        public float Distancetotravel => launcher.Position.DistanceTo(usedTarget.Cell);
        public float Distancetraveled => launcher.Position.DistanceTo(this.Position);
        public float Traveled => (Distancetraveled / Distancetotravel);
        public int FrameTicks => this.Props?.ticksPerFrame ?? (this.def.HasModExtension<AnimatedProjectileExtension>() ? this.def.GetModExtension<AnimatedProjectileExtension>().ticksPerFrame : 5);

        public override void Tick()
        {
            this.age++;
            base.Tick();
            checked
            {
                bool flag;
                if (this.def.graphicData.graphicClass == typeof(Graphic_Flicker))
                {
                    flag = this.TicksforFrame == 0 && base.Map != null;
                    if (flag)
                    {
                        gfxint++;
                        if (gfxint >= subGraphics.Length)
                        {
                            gfxint = 0;
                        }
                        this.TicksforFrame = FrameTicks;
                    }
                }
                if (this.def.graphicData.graphicClass == typeof(Graphic_Random))
                {
                    flag = this.TicksforFrame == 0 && base.Map != null;
                    if (flag)
                    {
                        Rand.PushState();
                        gfxint = Rand.RangeInclusive(0, subGraphics.Length-1);
                        Rand.PopState();
                        if (gfxint >= subGraphics.Length)
                        {
                            gfxint = 0;
                        }
                        this.TicksforFrame = FrameTicks;
                    }
                }
                this.TicksforFrame--;
            }
        }
        
        public override Graphic Graphic
        {
            get
            {
            //    Log.Message(string.Format("Subgraphic used: {0}", gfxint));
                if (subGraphics!=null)
                {
                    return subGraphics[gfxint];
                }
                return base.Graphic;
            }
        }
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (this.def.graphicData.graphicClass == typeof(Graphic_Flicker))
            {
                traverse = Traverse.Create(base.Graphic);
                subGraphics = (Graphic[])subgraphicsFlicker.GetValue(base.Graphic);
            //    Log.Message(string.Format("Subgraphics: {0}", subGraphics.Length));
            }
            if (this.def.graphicData.graphicClass == typeof(Graphic_Random))
            {
                traverse = Traverse.Create(base.Graphic);
                subGraphics = (Graphic[])subgraphicsRandom.GetValue(base.Graphic);
            //    Log.Message(string.Format("Subgraphics: {0}", subGraphics.Length));
            }

        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Mesh mesh = MeshPool.GridPlane(this.def.graphicData.drawSize * Drawsize);
            Graphics.DrawMesh(mesh, drawLoc, this.ExactRotation, Graphic.MatSingle, 0);
            base.Comps_PostDraw();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.gfxint, "gfxint", -1);
            Scribe_Values.Look(ref this.TicksforFrame, "TicksforFrame");
            Scribe_Values.Look(ref this.age, "age");

        }

        Traverse traverse;
        Graphic[] subGraphics;
        public static FieldInfo subgraphicsFlicker = typeof(Graphic_Flicker).GetField("subGraphics", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        public static FieldInfo subgraphicsRandom = typeof(Graphic_Random).GetField("subGraphics", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
        private int gfxint = 0;
        private int TicksforFrame = 5;
        private int age = 0;
    }
}
