using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedGraphics.HarmonyInstance;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AdvancedGraphics
{
    // Token: 0x0200024E RID: 590
    public class CompProperties_AdvancedGraphic : CompProperties
    {
        // Token: 0x06000AB1 RID: 2737 RVA: 0x0005598C File Offset: 0x00053D8C
        public CompProperties_AdvancedGraphic()
        {
            this.compClass = typeof(CompAdvancedGraphic);
        }
        public bool randomised = false;
        public bool onlyArtable = false;
        public bool quality = false;
        public string tagged = string.Empty;
        public QualityCategory minQuality = QualityCategory.Masterwork;
        public List<QualityGraphic> qualityGraphics = new List<QualityGraphic>();
    }
    // Token: 0x02000C69 RID: 3177
    [StaticConstructorOnStartup]
    public class CompAdvancedGraphic : ThingComp
    {
        public bool initalized = false;
        public CompProperties_AdvancedGraphic Props
        {
            get
            {
                return (CompProperties_AdvancedGraphic)this.props;
            }
        }
        public List<QualityGraphic> qualityGraphics => Props.qualityGraphics;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                //    Graphic(parent.Graphic);
            }
        }

        public int Randomized
        {
            get
            {
                int i = 0;
                Rand.PushState();
                i = Rand.Range(0, possibleTexturelist.Count);
                Rand.PopState();
                initalized = true;
                return i;
            }
        }
        public int Quality
        {
            get
            {
                int i = 0;
                CompQuality quality = parent.TryGetComp<CompQuality>();
                if (quality == null)
                {
                    Log.Warning(string.Format("WARNING!! {0} is set to use quality graphics but has no CompQuality, using random graphic", parent.Label));
                    i = Randomized;
                }
                else
                {
                    if ((int)quality.Quality >= (int)Props.minQuality)
                    {
                        int i2 = (int)quality.Quality - (int)Props.minQuality + 1;
                        i = Math.Min(i2, possibleTexturelist.Count - 1);
                    }
                }
                initalized = true;
                return i;
            }
        }

        public List<Texture2D> possibleTexturelist = new List<Texture2D>();
        public override void PostPostMake()
        {
            base.PostPostMake();
            //    Graphic(parent.Graphic);
        }

        public Graphic current
        {
            get
            {
                if (_graphic != null)
                {
                    return _graphic;
                }
                return parent.Graphic;
            }
            set
            {
                _graphic = value;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.gfxint, "gfxint", -1);
            Scribe_Values.Look(ref this.initalized, "initalized", false);
            //    Scribe_Values.Look<Graphic>(ref this._graphic, "_graphic");
        }
        public Graphic _graphic;
        public int gfxint = -1;

    }
    public struct QualityGraphic
    {
        public QualityCategory Quality;
        public GraphicData GraphicData;
    }
}
