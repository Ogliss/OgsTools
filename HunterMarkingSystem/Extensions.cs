using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using HunterMarkingSystem;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem.ExtensionMethods
{
    [StaticConstructorOnStartup]
    public static class Extensions
    {
        public static bool Marked(this Pawn p, out Hediff MarkedHD, out Hediff UnmarkedHD)
        {
            MarkedHD = null;
            UnmarkedHD = null;
            if (!p.Markable(out Comp_Markable Markable))
            {
                return false;
            }
            HediffSet hediffSet = p.health.hediffSet;
            bool hasbloodedM = hediffSet.hediffs.Any(x => x.def == Markable.Markeddef || x.def.defName.Contains(HunterMarkingSystem.Markedkey));
            if (hasbloodedM) MarkedHD = hediffSet.hediffs.Find(x => x.def == Markable.Markeddef || x.def.defName.Contains(HunterMarkingSystem.Markedkey));
            bool hasbloodedUM = hediffSet.hediffs.Any(x => x.def == Markable.Unmarkeddef || x.def.defName.Contains(HunterMarkingSystem.Unmarkededkey));
            if (hasbloodedUM) UnmarkedHD = hediffSet.hediffs.Find(x => x.def == Markable.Unmarkeddef || x.def.defName.Contains(HunterMarkingSystem.Unmarkededkey));
            return hasbloodedM;
        }

        public static bool Marked(this Pawn p, out Hediff MarkedHD)
        {
            MarkedHD = null;
            if (!p.Markable(out Comp_Markable Markable))
            {
                return false;
            }
            HediffSet hediffSet = p.health.hediffSet;
            bool hasbloodedM = hediffSet.hediffs.Any(x => x.def == Markable.Markeddef || x.def.defName.Contains(HunterMarkingSystem.Markedkey));
            if (hasbloodedM) MarkedHD = hediffSet.hediffs.Find(x => x.def == Markable.Markeddef || x.def.defName.Contains(HunterMarkingSystem.Markedkey));
            bool hasbloodedUM = hediffSet.hediffs.Any(x => x.def == Markable.Unmarkeddef || x.def.defName.Contains(HunterMarkingSystem.Unmarkededkey));
            if (hasbloodedUM) MarkedHD = hediffSet.hediffs.Find(x => x.def == Markable.Unmarkeddef || x.def.defName.Contains(HunterMarkingSystem.Unmarkededkey));
            return hasbloodedM;
        }

        public static bool Marked(this Pawn p)
        {
            HediffSet hediffSet = p.health.hediffSet;
            Comp_Markable Markable = p.TryGetComp<Comp_Markable>();
            if (Markable==null)
            {
                return false;
            }
            bool hasbloodedM = hediffSet.hediffs.Any(x => x.def == Markable.Markeddef || x.def.defName.Contains("Hediff_BloodedM"));
            return hasbloodedM;
        }
        
        public static bool isBloodable(this Pawn p)
        {
            return p.TryGetComp<Comp_Markable>() != null;
        }

        public static bool Markable(this Pawn p)
        {
            return p.Markable(out Comp_Markable comp);
        }

        public static bool Markable(this Pawn p, out Comp_Markable comp)
        {
            comp = p.TryGetComp<Comp_Markable>();
            return comp != null;
        }

        public static bool Markable(this Thing p)
        {
            return p.Markable(out Comp_Markable comp);
        }

        public static bool Markable(this Thing p, out Comp_Markable comp)
        {
            comp = p.TryGetComp<Comp_Markable>();
            return comp != null;
        }

        public static bool Markable(this ThingDef p)
        {
            return p.HasComp(typeof(Comp_Markable));
        }
        public static bool Markable(this ThingDef p, out CompProperties_Markable props)
        {
            props = null;
            if (p.HasComp(typeof(Comp_Markable)))
            {
                props = p.GetCompProperties<CompProperties_Markable>();
                return true;
            }
            return false;
        }

        public static bool isWorthyKillFor(this Pawn x, Pawn y)
        {
            bool result = false;
            if (x != null)
            {
                MarkData markDatax = new MarkData(x);
                MarkData markDatay = new MarkData(y);
                if (markDatax.MarkScore > (markDatay.MarkScore * Settings.SettingsHelper.latest.MinWorthyKill))
                {
                    result = true;
                }
            //    Log.Message(string.Format("{0} Worthy Kill for {1} : {4}, Score: \n{0}: {2} Vs {1}: {3},", x.LabelShortCap, y.LabelShortCap, markDatax.MarkScore, markDatay.MarkScore, result));
            }
            else
            {
            //    Log.Message("X == null");
            }
            return result;
        }

        public static bool isWorthyKillFor(this Pawn x, Pawn y, out MarkData markData)
        {
            bool result = false;
            markData = null;
            if (x != null)
            {
                MarkData markDatax = new MarkData(x);
                MarkData markDatay = new MarkData(y);
                if (markDatax.MarkScore > (markDatay.MarkScore * Settings.SettingsHelper.latest.MinWorthyKill))
                {
                    markData = markDatax;
                    result = true;
                }
            //    Log.Message(string.Format("{0} Worthy Kill for {1} : {4}, Score: \n{0}: {2} Vs {1}: {3},", x.LabelShortCap, y.LabelShortCap, markDatax.MarkScore, markDatay.MarkScore, result));
            }
            else
            {
            //    Log.Message("X == null");
            }
            return result;
        }
        /*
        public static MarkData markDataFor(this Pawn x)
        {
            MarkData data = null;
            if (x.Markable(out Comp_Markable Markable))
            {
                if (Markable.markDataSelf!=null)
                {
                    return Markable.markDataSelf;
                }
            }
            return data;
        }
        */
    }
}
