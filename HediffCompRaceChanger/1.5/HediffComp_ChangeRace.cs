using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace HediffCompRaceChanger
{
    public class ExtendedTraitEntry
    {
        public TraitDef def = null;
        public int degree = 0;
        public float Chance = 1f;
        public bool replaceIfFull = true;
    }
    // Token: 0x02000241 RID: 577
    public class HediffCompProperties_ChangeRace : HediffCompProperties
    {
        // Token: 0x06000FEE RID: 4078 RVA: 0x0005AFE0 File Offset: 0x000591E0
        public HediffCompProperties_ChangeRace()
        {
            this.compClass = typeof(HediffComp_ChangeRace);
        }

        public bool PermenantEffect = true;
        public bool onRemovedEffect = true;
        public bool onAddedEffect = true;

        public bool colourSkin = true;
        public bool colourSkinTwo = true;
        public bool colourHair = false;
        public bool colourHairTwo = false;
        public bool removeHair = true;
        public ThingDef raceDef = null;
        public bool changeHead = true;
        public string crownType = string.Empty;
        public bool changeBody = true;
        public BodyTypeDef bodyTypeDef = null;
        public List<ExtendedTraitEntry> traitsToAdd = new List<ExtendedTraitEntry>();
        public List<ExtendedTraitEntry> traitsToRemove = new List<ExtendedTraitEntry>();

        // Token: 0x04000BB0 RID: 2992
        public IntRange disappearsAfterTicks = new IntRange(1, 5);

        // Token: 0x04000BB1 RID: 2993
        public bool showRemainingTime = false;
    }
    // Token: 0x02000242 RID: 578
    public class HediffComp_ChangeRace : HediffComp
    {
        // Token: 0x17000327 RID: 807
        // (get) Token: 0x06000FEF RID: 4079 RVA: 0x0005AFF8 File Offset: 0x000591F8
        public HediffCompProperties_ChangeRace Props
        {
            get
            {
                return (HediffCompProperties_ChangeRace)this.props;
            }
        }

        // Token: 0x17000328 RID: 808
        // (get) Token: 0x06000FF0 RID: 4080 RVA: 0x0005B005 File Offset: 0x00059205
        public override bool CompShouldRemove
        {
            get
            {
                return base.CompShouldRemove || this.ticksToDisappear <= 0;
            }
        }

        // Token: 0x17000329 RID: 809
        // (get) Token: 0x06000FF1 RID: 4081 RVA: 0x0005B020 File Offset: 0x00059220
        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (!this.Props.showRemainingTime)
                {
                    return base.CompLabelInBracketsExtra;
                }
                return this.ticksToDisappear.TicksToSeconds().ToString("0.0");
            }
        }
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            CompPostPostRemoved();
        }
        public override void CompPostPostRemoved()
        {
            TransformPawn();
            base.CompPostPostRemoved();
        }

        public AlienRace.AlienPartGenerator.AlienComp compAlien => Pawn.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>();
        public AlienRace.ThingDef_AlienRace alienRace => (AlienRace.ThingDef_AlienRace)Props.raceDef ?? null;
        private void TransformPawn(bool changeDef = true, bool keep = false)
        {
            //sets position, faction and map
            IntVec3 intv = Pawn.Position;
            Faction faction = Pawn.Faction;
            Map map = Pawn.Map;
            RegionListersUpdater.DeregisterInRegions(Pawn, map);

            //Change Race to Props.raceDef
            if (changeDef && alienRace != null && alienRace != Pawn.def)
            {
                Pawn.def = alienRace;
                long ageB = Pawn.ageTracker.AgeBiologicalTicks;
                long ageC = Pawn.ageTracker.AgeChronologicalTicks;
                Pawn.ageTracker = new Pawn_AgeTracker(Pawn);
                Pawn.ageTracker.AgeBiologicalTicks = ageB;
                Pawn.ageTracker.AgeChronologicalTicks = ageC;
                if (!Pawn.RaceProps.hasGenders)
                {
                    Pawn.gender = Gender.None;
                }
                if (Props.removeHair)
                {
                    //    Pawn.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
                    Pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(Pawn);
                    //   Pawn.Drawer.renderer.graphics.hairGraphic;
                }
                else
                {
                    if (Props.colourHairTwo || Props.colourHair)
                    {

                        if (compAlien != null)
                        {
                            ColorChannelGenerator Alienhair = alienRace.alienRace.generalSettings.alienPartGenerator.colorChannels.Find(x => x.name == "hair");
                            AlienPartGenerator.ExposableValueTuple<UnityEngine.Color, UnityEngine.Color> hair;
                            if (compAlien.ColorChannels.TryGetValue("hair", out hair))
                            {
                                if (Props.colourHair && Alienhair?.first != null)
                                {
                                    hair.first = Alienhair.first.NewRandomizedColor();
                                }
                                if (Props.colourHairTwo && Alienhair?.second != null)
                                {
                                    hair.second = Alienhair.second.NewRandomizedColor();
                                }
                                compAlien.ColorChannels.SetOrAdd("hair", hair);
                            }
                            Pawn.Notify_ColorChanged();
                        }
                    }
                }
                //Change BodyType to Props.bodyTypeDef
                if (Props.changeBody)
                {

                    if (Props.bodyTypeDef != null)
                    {
                        ChangeBodyType(Pawn, Props.bodyTypeDef);
                    }
                    else
                    {
                        ChangeBodyType(Pawn, alienRace.alienRace.generalSettings.alienPartGenerator.bodyTypes[Rand.Range(0, alienRace.alienRace.generalSettings.alienPartGenerator.bodyTypes.Count)]);
                    }

                }
            }
            //Remove Disallowed Traits
            int maxTraits;
            if (MoreTraitSlotsUtil.TryGetMaxTraitSlots(out int max))
            {
                maxTraits = max;
            }
            else { maxTraits = 4; }
            if (!Props.traitsToRemove.NullOrEmpty())
            {
                if (Pawn.story.traits.allTraits.Any(x => Props.traitsToRemove.Any(y => y.def == x.def)))
                {
                    foreach (ExtendedTraitEntry item in Props.traitsToRemove)
                    {
                        if (Pawn.story.traits.HasTrait(item.def))
                        {
                            Rand.PushState();
                            if (Rand.Chance(item.Chance))
                            {
                                Pawn.story.traits.allTraits.Remove(Pawn.story.traits.allTraits.Find(x => x.def == item.def));
                            }
                            Rand.PopState();
                        }
                    }

                }
            }
            if (!Props.traitsToAdd.NullOrEmpty())
            {
                if (Props.traitsToAdd.Any(x => !Pawn.story.traits.HasTrait(x.def)))
                {

                    foreach (ExtendedTraitEntry item in Props.traitsToAdd)
                    {
                        if (!Pawn.story.traits.HasTrait(item.def))
                        {
                            Rand.PushState();
                            if (Rand.Chance(item.Chance))
                            {
                                bool replace = false;
                                int countTraits = Pawn.story.traits.allTraits.Count;
                                if (countTraits >= maxTraits)
                                {
                                    replace = true;
                                }
                                //   Log.Message(string.Format("i have {0} of a max of {1} traits", countTraits, maxTraits));
                                Trait replacedTrait = Pawn.story.traits.allTraits.Where(x => Props.traitsToAdd.Any(y => y.def != x.def)).RandomElement();
                                if (replace)
                                {
                                    Pawn.story.traits.allTraits.Remove(replacedTrait);
                                }
                                Pawn.story.traits.allTraits.Add(new Trait(item.def, item.degree));
                            }
                            Rand.PopState();
                        }
                    }
                }
            }
            RegionListersUpdater.RegisterInRegions(Pawn, map);
            map.mapPawns.UpdateRegistryForPawn(Pawn);

            //decache graphics
            Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            Find.ColonistBar.drawer.Notify_RecachedEntries();

            //save the pawn
            Pawn.ExposeData();
            if (Pawn.Faction != faction)
            {
                Pawn.SetFaction(faction);
            }
            //    pawn.Position = intv;
            
        }

        private void ChangeBodyType(Pawn pawn, BodyTypeDef bt)
        {
            AlienPartGenerator alienPartGenerator = alienRace.alienRace.generalSettings.alienPartGenerator;
            var storyTrv = Traverse.Create(pawn.story);
            var newStory = new Pawn_StoryTracker(pawn);
            var newStoryTrv = Traverse.Create(newStory);
            AccessTools.GetFieldNames(typeof(Pawn_StoryTracker))
                    .ForEach(f => newStoryTrv.Field(f).SetValue(storyTrv.Field(f).GetValue()));
            newStory.bodyType = bt;
            if (Props.colourSkinTwo || Props.colourSkin)
            {
                if (compAlien != null)
                {
                    ColorChannelGenerator Alienskin = alienRace.alienRace.generalSettings.alienPartGenerator.colorChannels.Find(x => x.name == "skin");
                    AlienPartGenerator.ExposableValueTuple<UnityEngine.Color, UnityEngine.Color> skin;
                    if (compAlien.ColorChannels.TryGetValue("skin", out skin))
                    {
                        if (Props.colourSkin && Alienskin?.first != null)
                        {
                            skin.first = Alienskin.first.NewRandomizedColor();
                        }
                        if (Props.colourSkinTwo && Alienskin?.second != null)
                        {
                            skin.second = Alienskin.second.NewRandomizedColor();
                        }
                        compAlien.ColorChannels.SetOrAdd("skin", skin);
                    }
                    Traverse.Create(newStory).Field("SkinColor").SetValue(skin.first);
                    Pawn.Notify_ColorChanged();
                }
            }
            if (Props.changeHead)
            {
                List<string> heads = new List<string>();
                if (!Props.crownType.NullOrEmpty())
                {
                    heads.Add(Props.crownType);
                }
                else
                {
                    heads.AddRange(alienPartGenerator.HeadTypes);
                }
                compAlien.crownType = null;

                if (Pawn.story.HeadGraphicPath.Contains("Average"))
                {
                    newStory.crownType = CrownType.Average;
                }
                else if (Pawn.story.HeadGraphicPath.Contains("Narrow"))
                {
                    newStory.crownType = CrownType.Narrow;
                }
            }
            pawn.story = newStory;
            Traverse.Create(newStory).Field("headGraphicPath").SetValue(alienRace.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head.NullOrEmpty() ? "" : alienPartGenerator.RandomAlienHead(alienRace.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage).head, Pawn));
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            Find.ColonistBar.drawer.Notify_RecachedEntries();
        }

        // Token: 0x06000FF2 RID: 4082 RVA: 0x0005B059 File Offset: 0x00059259
        public override void CompPostMake()
        {
            base.CompPostMake();
            this.ticksToDisappear = this.Props.disappearsAfterTicks.RandomInRange;
        }

        // Token: 0x06000FF3 RID: 4083 RVA: 0x0005B077 File Offset: 0x00059277
        public override void CompPostTick(ref float severityAdjustment)
        {
            this.ticksToDisappear--;
        }

        // Token: 0x06000FF4 RID: 4084 RVA: 0x0005B088 File Offset: 0x00059288
        public override void CompPostMerged(Hediff other)
        {
            base.CompPostMerged(other);
            HediffComp_ChangeRace hediffComp_Disappears = other.TryGetComp<HediffComp_ChangeRace>();
            if (hediffComp_Disappears != null && hediffComp_Disappears.ticksToDisappear > this.ticksToDisappear)
            {
                this.ticksToDisappear = hediffComp_Disappears.ticksToDisappear;
            }
        }

        // Token: 0x06000FF5 RID: 4085 RVA: 0x0005B0C0 File Offset: 0x000592C0
        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToDisappear, "ticksToDisappear", 0, false);
        }

        // Token: 0x06000FF6 RID: 4086 RVA: 0x0005B0D4 File Offset: 0x000592D4
        public override string CompDebugString()
        {
            return "ticksToDisappear: " + this.ticksToDisappear;
        }

        // Token: 0x04000BB2 RID: 2994
        public int ticksToDisappear;
        private static readonly FactionDef noHairFaction = new FactionDef
        {
            hairTags = new List<string>
            {
                "alienNoHair"
            }
        };
    }
    static class MoreTraitSlotsUtil
    {
        private static bool initialized = false;
        private static FieldInfo settingsFieldInfo = null;
        private static FieldInfo maxFieldInfo = null;

        public static bool TryGetMaxTraitSlots(out int max)
        {
            if (!initialized)
            {
                initialized = true;
                Initialized();
            }

            if (settingsFieldInfo != null && maxFieldInfo != null)
            {
                object settings = settingsFieldInfo.GetValue(null);
                if (settings != null)
                {
                    max = (int)(float)maxFieldInfo.GetValue(settings);
                    return true;
                }
            }
            max = 0;
            return false;
        }

        private static void Initialized()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.Name.IndexOf("More Trait Slots") != -1)
                {
                    foreach (Assembly assembly in p.assemblies.loadedAssemblies)
                    {
                        Type type = assembly.GetType("MoreTraitSlots.RMTS");
                        if (type != null)
                        {
                            settingsFieldInfo = type.GetField("Settings", BindingFlags.Public | BindingFlags.Static);
                            if (settingsFieldInfo != null)
                            {
                                Type st = settingsFieldInfo.GetValue(null).GetType();
                                maxFieldInfo = st.GetField("traitsMax", BindingFlags.Public | BindingFlags.Instance);
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
    // Token: 0x0200001C RID: 28
    internal static class GraphicPathsExtension
    {
        // Token: 0x060000AF RID: 175 RVA: 0x00009DC0 File Offset: 0x00007FC0
        public static GraphicPaths GetCurrentGraphicPath(this List<GraphicPaths> list, LifeStageDef lifeStageDef)
        {
            return list.FirstOrDefault(delegate (GraphicPaths gp)
            {
                List<LifeStageDef> lifeStageDefs = gp.lifeStageDefs;
                return lifeStageDefs != null && lifeStageDefs.Contains(lifeStageDef);
            }) ?? list.First<GraphicPaths>();
        }
    }
}
