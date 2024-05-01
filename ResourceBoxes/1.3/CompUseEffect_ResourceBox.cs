using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace ResourceBoxes
{
    // Token: 0x0200025F RID: 607
    public class CompProperties_UseEffect_ResourceBox : CompProperties_UseEffect
    {
        // Token: 0x06000ACD RID: 2765 RVA: 0x000562EC File Offset: 0x000546EC
        public CompProperties_UseEffect_ResourceBox()
        {
            this.compClass = typeof(CompUseEffect_ResourceBox);
        }
        public List<ThingDefCountClass> possibleItems = new List<ThingDefCountClass>();
        public List<List<ThingDefCount>> possibleItemLists = new List<List<ThingDefCount>>();
        public List<Pair<ThingDef, int>> possibleItemsWithWeight = new List<Pair<ThingDef, int>>();
        public int PerItemCount = -1;
        public bool randomCount = false;
        public int minItems = 1;
        public int maxItems = 1;
        public int minPerItem = 1;
        public int maxPerItem = 1;
        public int maxUses = 1;
        public SoundDef soundDef = null;
        public bool destoryOnUse = true;
        public bool spawnAll = false;
        public float destroyChance = 1f;
        public int maxWorth = -1;
        public string fixedQuality = string.Empty;
        public QualityCategory minQuality = QualityCategory.Awful;
        public QualityCategory maxQuality = QualityCategory.Legendary;
    }

    // Token: 0x02000002 RID: 2
    public class CompUseEffect_ResourceBox : CompUseEffect
    {
        public CompProperties_UseEffect_ResourceBox PropsResourceBox
        {
            get
            {
                return (CompProperties_UseEffect_ResourceBox)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.timesUsed, "timesUsed", 0, true);
        }

        public int timesUsed = 0;
        // Token: 0x06000002 RID: 2 RVA: 0x00002067 File Offset: 0x00000267
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (PropsResourceBox.soundDef != null)
            {
                SoundStarter.PlayOneShotOnCamera(PropsResourceBox.soundDef, usedBy.MapHeld);
            }
            this.OpenBox(usedBy);
            Rand.PushState();
            if (PropsResourceBox.destoryOnUse && Rand.Chance(PropsResourceBox.destroyChance))
            {
                if (this.parent.stackCount == 1)
                {
                    this.parent.Destroy();
                }
                else
                {
                    this.parent.stackCount--;
                }
            }
            Rand.PopState();
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            return base.CanBeUsedBy(p, out failReason);
        }

        // Token: 0x06000003 RID: 3 RVA: 0x0000209C File Offset: 0x0000029C
        protected virtual void OpenBox(Pawn usedBy)
        {
            IntVec3 position = this.parent.Position;
            Map map = this.parent.Map;
            map.wealthWatcher.ForceRecount(false);
            List<Thing> list = this.CreateLoot();
            foreach (Thing thing in list)
            {
                GenPlace.TryPlaceThing(thing, position, map, (ThingPlaceMode.Near), null, null);
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002160 File Offset: 0x00000360
        private List<Thing> CreateLoot()
        {
            List<Thing> list = new List<Thing>();
            int ItemCount;
            if (PropsResourceBox.spawnAll)
            {
                ItemCount = PropsResourceBox.possibleItems.Count;
            }
            else
            {
                Rand.PushState();
                ItemCount = Rand.RangeInclusive(PropsResourceBox.minItems, PropsResourceBox.maxItems);
                Rand.PopState();
            }
            for (int i = 0; i < ItemCount; i++)
            {
                ThingDef named = null;
                int PerItemCount;
                if (PropsResourceBox.PerItemCount != -1)
                {
                    PerItemCount = PropsResourceBox.PerItemCount;
                }
                else
                {
                    if (PropsResourceBox.randomCount)
                    {
                        Rand.PushState();
                        PerItemCount = Rand.RangeInclusive(PropsResourceBox.minPerItem, PropsResourceBox.maxPerItem);
                        Rand.PopState();
                    }
                    else
                    {
                        PerItemCount = PropsResourceBox.possibleItems[i].count;
                    }
                }
                if (!PropsResourceBox.possibleItems.NullOrEmpty())
                {
                    if (PropsResourceBox.spawnAll)
                    {
                        named = PropsResourceBox.possibleItems[i].thingDef;
                    }
                    else
                    {
                        Rand.PushState();
                        int element = Rand.RangeInclusive(0, PropsResourceBox.possibleItems.Count - 1);
                        Rand.PopState();
                        named = PropsResourceBox.possibleItems[element].thingDef;
                    }
                }
                if (named != null)
                {
                    Thing thing = ThingMaker.MakeThing(named, GenStuff.RandomStuffFor(named));
                    thing.stackCount = PerItemCount;
                    CompQuality compQuality = ThingCompUtility.TryGetComp<CompQuality>(thing);
                    if (compQuality != null)
                    {
                        QualityCategory qualityCategory = this.Quality;
                        compQuality.SetQuality(qualityCategory, 0);
                    }
                    CompArt compArt = ThingCompUtility.TryGetComp<CompArt>(thing);
                    if (compArt != null)
                    {
                        compArt.InitializeArt(0);
                    }
                    list.Add(thing);
                }
            }
            return list;
        }

        public QualityCategory Quality
        {
            get
            {
                switch (PropsResourceBox.fixedQuality)
                {
                    case "Awful":
                        return QualityCategory.Awful;
                    case "Poor":
                        return QualityCategory.Poor;
                    case "Normal":
                        return QualityCategory.Normal;
                    case "Good":
                        return QualityCategory.Good;
                    case "Excellent":
                        return QualityCategory.Excellent;
                    case "Masterwork":
                        return QualityCategory.Masterwork;
                    case "Legendary":
                        return QualityCategory.Legendary;
                    default:
                        Rand.PushState();
                        QualityCategory Quality = (QualityCategory)Rand.RangeInclusive((int)PropsResourceBox.minQuality, (int)PropsResourceBox.maxQuality);
                        Rand.PopState();
                        return Quality;
                }

            }
        }

        // Token: 0x06000005 RID: 5 RVA: 0x000023F8 File Offset: 0x000005F8
        public override string CompInspectStringExtra()
        {
            return " \n" + this.parent.DescriptionFlavor;
        }
    }
}
