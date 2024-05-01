using System;
using System.Collections.Generic;
using RimWorld;
using System.Text;
using Verse;
using HunterMarkingSystem.ExtensionMethods;

namespace HunterMarkingSystem
{
    public static class HMSUtility
    {

        // Token: 0x02000D68 RID: 3432
        public enum BloodStatusMode
        {
            NoComp,
            None,
            Unblooded,
            Unmarked,
            Marked
        }

        // Token: 0x06004C44 RID: 19524 RVA: 0x0023802D File Offset: 0x0023642D
        public static string GetLabel(this BloodStatusMode m, Pawn pawn)
        {
            if (pawn.Markable(out Comp_Markable Markable))
            {

                switch (m)
                {
                    case BloodStatusMode.Unblooded:
                        return "HMS_BloodStatus_Unblooded".Translate(pawn.LabelShortCap, Markable.MyScore);
                    case BloodStatusMode.Unmarked:
                        return "HMS_BloodStatus_Unmarked".Translate(pawn.LabelShortCap, Markable.markDataKillNew.Label, Markable.markDataKillNew.raceDef.LabelCap, Markable.markDataKillNew.MarkScore);
                    case BloodStatusMode.Marked:
                        return "HMS_BloodStatus_Marked".Translate(pawn.LabelShortCap, Markable.markDataKill.MarkDef, Markable.markDataKill.raceDef.LabelCap, Markable.markDataKill.MarkScore);
                    default:
                        return "HMS_BloodStatus_Uninitalised".Translate(pawn.LabelShortCap);
                }
            }
            return "HMS_BloodStatus_Uninitalised".Translate(pawn.LabelShortCap);
        }

        public static HediffDef GetMark(Pawn x)
        {
            List<HediffDef> list = new List<HediffDef>();
            list = GetMarks(x.def);
            if (x.kindDef.factionLeader && list.Any(y => y.defName.Contains("Worthy")))
            {
                return list.Find(y => y.defName.Contains("Worthy"));
            }
            return list[0];
        }
        public static List<HediffDef> GetMarks(Pawn x)
        {
            return GetMarks(x.def);
        }

        public static HediffDef GetMark(PawnKindDef x)
        {
            List<HediffDef> list = new List<HediffDef>();
            list = GetMarks(x.race);
            if (x.factionLeader && list.Any(y => y.defName.Contains("Worthy")))
            {
                return list.Find(y => y.defName.Contains("Worthy"));
            }
            return list[0];
        }
        public static List<HediffDef> GetMarks(PawnKindDef x)
        {
            return GetMarks(x.race);
        }

        public static HediffDef GetMark(ThingDef x)
        {
            List<HediffDef> list = new List<HediffDef>();
            list = GetMarks(x);
            return list[0];
        }

        public static List<HediffDef> GetMarks(ThingDef x)
        {
            List<HediffDef> Marks = new List<HediffDef>();
            if (x.race==null)
            {
                return Marks;
            }
            bool ArtificalPawn = x.race.FleshType.defName.Contains("Flesh_Construct") || x.race.FleshType.defName.Contains("Deamon") || x.race.body.defName.Contains("AIRobot") || x.race.IsMechanoid || (UtilChjAndroids.ChjAndroid && UtilChjAndroids.isChjAndroid(x)) || (UtilTieredAndroids.TieredAndroid && UtilTieredAndroids.isAtlasAndroid(x));
            if (x.HasModExtension<MarkDefExtension>())
            {
                if (!x.GetModExtension<MarkDefExtension>().hediffs.NullOrEmpty())
                {
                    return x.GetModExtension<MarkDefExtension>().hediffs;
                }
                else
                {
                    Log.Warning(string.Format("MarkDefExtension found for {0} but no Hediffs found", x.defName));
                }
            }
            else
            if (ArtificalPawn)
            {
                Marks.Add(HMSDefOf.HMS_Hediff_BloodedMMechanoid);
            }
            else
            if (x.race.Humanlike)
            {
                if (x.defName.Contains("Human") || x == ThingDefOf.Human || x.race.meatDef == ThingDefOf.Meat_Human)
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMHuman);
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMWorthyHuman);
                }
                else
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMHumanlike);
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMWorthyHumanlike);
                }
            }
            else
            {

                if (x.defName.Contains("Xenomorph"))
                {
                    if (x.defName.Contains("Queen"))
                    {
                        Marks.Add(HMSDefOf.HMS_Hediff_BloodedMXenomorphQueen);
                    }
                    else if (x.defName.Contains("Predalien"))
                    {
                        Marks.Add(HMSDefOf.HMS_Hediff_BloodedMPredalien);
                    }
                    else if (x.defName.Contains("Thrumbo"))
                    {
                        Marks.Add(HMSDefOf.HMS_Hediff_BloodedMXenomorphThrumbo);
                    }
                    else Marks.Add(HMSDefOf.HMS_Hediff_BloodedMXenomorph);
                }
                else if (x.defName.Contains("GroTye") || x.defName.Contains("Megasloth"))
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMGroTye);
                }
                else if ((x.defName.Contains("Rhinoceros") || x.defName.Contains("Elephant")))
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMCrusher);
                }
                else if (x.defName.Contains("Thrumbo"))
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMThrumbo);
                }
                else if (((x.defName.Contains("Wolf") || x.description.Contains("Wolf") || x.description.Contains("wolf") || x.description.Contains("wolves")) || (x.defName.Contains("Hound") || x.defName.Contains("hound") || x.description.Contains("Hound") || x.description.Contains("hound") || x.description.Contains("hounds")) || (x.defName.Contains("Dog") || x.description.Contains("Dog") || x.description.Contains("dog") || x.description.Contains("dogs"))) && x.race.body.defName.Contains("Quad") && ((x.race.predator == true)))
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMHound);
                }
                else if (x.race.Animal)
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedMBeast);
                }
                else
                {
                    Marks.Add(HMSDefOf.HMS_Hediff_BloodedM);
                }
            }
            return Marks;
        }
    }
}
