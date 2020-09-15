using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;
using HunterMarkingSystem.Settings;
using HunterMarkingSystem.ExtensionMethods;

namespace HunterMarkingSystem
{
    [StaticConstructorOnStartup]
    public class HunterMarkingSystem
    {
        public static string Unbloodedkey = "Hediff_Unblooded";
        public static string Unmarkededkey = "Hediff_BloodedUM";
        public static string Markedkey = "Hediff_BloodedM";
        public static List<HediffDef> UnbloodedHediffList = DefDatabase<HediffDef>.AllDefs.Where(x => x.defName.Contains(Unbloodedkey)).ToList();
        public static List<HediffDef> BloodedUMHediffList = DefDatabase<HediffDef>.AllDefs.Where(x => x.defName.Contains(Unmarkededkey)).ToList();
        public static List<HediffDef> BloodedMHediffList = DefDatabase<HediffDef>.AllDefs.Where(x => x.defName.Contains(Markedkey)).ToList();
        public static List<HunterCultureDef> CultureDefList = DefDatabase<HunterCultureDef>.AllDefs.ToList();
        public static Dictionary<ThingDef, MarkData> RaceDefaultMarkDict = new Dictionary<ThingDef, MarkData>();
        public static List<ThingDef> MarkableRaceDict = DefDatabase<ThingDef>.AllDefs.Where(x => x.Markable()).ToList();
        //    public static List<HediffDef> HunterMarkList = UnbloodedHediffList.Concat(BloodedUMHediffList)
        static HunterMarkingSystem()
        {
            DefDatabase<ThingDef>.AllDefsListForReading.ForEach(action: td =>
            {
                if (td.IsCorpse)
                {
                    td.comps.Add(new CompProperties_UsableCorpse()
                    {
                        compClass = typeof(Comp_KillMarker),
                        useJob = HMSDefOf.HMS_Job_MarkSelf,
                        useLabel = "Use {0} to mark self as Blooded"
                    });
                    td.comps.Add(new CompProperties_UseEffect()
                    {
                        compClass = typeof(CompUseEffect_MarkSelf)
                        //     chance = 0.25f
                    });
                    //    td.tickerType = TickerType.Normal;
                }
                else
                if (td.race != null)
                {
                    if (td.race.Humanlike)
                    {
                        bool pawnflag = !((UtilChjAndroids.ChjAndroid && UtilChjAndroids.isChjAndroid(td)) || (UtilTieredAndroids.TieredAndroid && UtilTieredAndroids.isAtlasAndroid(td)) || (UtilAvPSynths.AvP && UtilAvPSynths.isAvPSynth(td)));
                        bool marker = DefDatabase<HunterCultureDef>.AllDefs.Any(X => X.markerRaceDefs.Contains(td) || X.markerRaceDefs.NullOrEmpty());
                        bool allowed = DefDatabase<HunterCultureDef>.AllDefs.Any(X => X.allowedRaceDefs.Contains(td) || X.allowedRaceDefs.NullOrEmpty());
                        bool disallowed = DefDatabase<HunterCultureDef>.AllDefs.Any(X => !X.disallowedRaceDefs.Contains(td));
                    //    Log.Message(string.Format("{0}, marker: {1}, allowed: {2}, disallowed: {3}", td.LabelCap, marker, allowed, disallowed));
                        if ((marker || allowed) && disallowed)
                        {
                            List<HunterCultureDef> releventculture = DefDatabase<HunterCultureDef>.AllDefs.Where(X => (X.markerRaceDefs.Contains(td) || X.markerRaceDefs.NullOrEmpty()) || (X.allowedRaceDefs.Contains(td) || X.allowedRaceDefs.NullOrEmpty()) && !X.disallowedRaceDefs.Contains(td)).ToList();
                            if (!releventculture.NullOrEmpty())
                            {
                                foreach (HunterCultureDef cultureDef in releventculture)
                                {
                                    if (cultureDef != null)
                                    {
                                        if (!td.HasComp(typeof(Comp_Markable)) && (pawnflag || cultureDef.AllowMechanicalMarking))
                                        {
                                            td.comps.Add(new CompProperties_Markable()
                                            {
                                                cultureDef = cultureDef,
                                                markerRaceDefs = cultureDef.markerRaceDefs,
                                                allowedRaceDefs = cultureDef.allowedRaceDefs,
                                                disallowedRaceDefs = cultureDef.disallowedRaceDefs
                                            });
                                            if (td.HasComp(typeof(Comp_Markable)))
                                            {
                                                CompProperties_Markable markable = td.GetCompProperties<CompProperties_Markable>();
                                                string markerRaceDefs = string.Empty, allowedRaceDefs = string.Empty, disallowedRaceDefs = string.Empty;
                                                markable.markerRaceDefs.ForEach(x => markerRaceDefs += (" " + x.LabelCap));
                                                markable.allowedRaceDefs.ForEach(x => allowedRaceDefs += (" " + x.LabelCap));
                                                markable.disallowedRaceDefs.ForEach(x => disallowedRaceDefs += (" " + x.LabelCap));
                                            //    Log.Message(string.Format("Added Comp_Markable to: {0}, cultureDef: {1}, markerRaceDefs: {2}, allowedRaceDefs: {3}, disallowedRaceDefs: {4}", td.label, markable.cultureDef.label, markerRaceDefs, allowedRaceDefs, disallowedRaceDefs));
                                            }
                                        }
                                        if (td.HasComp(typeof(Comp_Markable)) && !MarkableRaceDict.Contains(td))
                                        {
                                            MarkableRaceDict.Add(td);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (DefDatabase<PawnKindDef>.AllDefs.Any(x => x.race == td) && !RaceDefaultMarkDict.Keys.Contains(td))
                    {
                        MarkData data = new MarkData(td);
                        RaceDefaultMarkDict.Add(td, data);
                    }
                }
            });
            /*
            DefDatabase<ThingDef>.AllDefsListForReading.ForEach(action: td => 
            {
                if (td.race!=null && td.isPotentialHost())
                {
                    string text = string.Format("{0}'s possible Xenoforms", td.LabelCap);
                //    Log.Message(text);

                    foreach (var item in td.resultingXenomorph())
                    {
                        text = item.LabelCap;
                    //    Log.Message(text);
                    }
                }
            });
            */
        //    Log.Message(string.Format("Hunter Marking System Loaded\n{0} Unblooded Hediffs, {1} Unmarked Hediffs, {2} Marked Hediffs deteched", UnbloodedHediffList.Count, BloodedUMHediffList.Count, BloodedMHediffList.Count));
        }
    }

}