using HunterMarkingSystem.ExtensionMethods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace HunterMarkingSystem
{
    // Token: 0x020007BB RID: 1979
    public class Alert_PawnCanMark : Alert
    {
        // Token: 0x170006D7 RID: 1751
        // (get) Token: 0x06002BF4 RID: 11252 RVA: 0x001499E8 File Offset: 0x00147DE8
        private IEnumerable<Thing> SickPawns
        {
            get
            {
                foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep)
                {
                    if (p.Markable(out Comp_Markable Markable))
                    {
                        if (p.health.hediffSet.hediffs.Any(x=> HunterMarkingSystem.BloodedUMHediffList.Contains(x.def)))
                        {
                            Hediff hediff = p.health.hediffSet.hediffs.Find(x=> HunterMarkingSystem.BloodedUMHediffList.Contains(x.def));
                            if (hediff != null)
                            {
                                if (Markable.MarkableCorpse)
                                {
                                    yield return p;
                                    if (Find.Selector.SingleSelectedThing == p)
                                    {
                                        yield return Markable.Markcorpse;
                                        
                                        if (!(Markable.MarkerRace || Markable.Inducted))
                                        {
                                            List<string> vs = new List<string>();
                                            foreach (ThingDef race in Markable.markerRaces)
                                            {
                                                IEnumerable<Thing> pawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Where(y => y.def == race && y.Map == p.Map);
                                                if (!pawns.EnumerableNullOrEmpty())
                                                {
                                                    foreach (Thing item in pawns)
                                                    {
                                                        yield return item;
                                                    }
                                                }
                                            }
                                        }
                                        
                                    }
                                    else
                                    {
                                    //    yield return Markable.Markcorpse;
                                    }
                                }
                                else
                                {
                                    if (Markable.Markcorpse != null)
                                    {
                                        if (Markable.Markcorpse.Destroyed)
                                        {
                                            //    Log.Message(string.Format("{0}'s target {1}, is destroyed", p.LabelShortCap, Markable.Markcorpse));
                                            p.health.RemoveHediff(hediff);
                                            if (Markable.MarkerRace)
                                            {
                                                p.health.AddHediff(Markable.Unmarkeddef, Markable.partRecord);
                                            }
                                            else
                                            {

                                            }
                                        }
                                        else
                                        {
                                        //    Log.Message(string.Format("{0}'s target {1}, is NOT destroyed", p.LabelShortCap, Markable.Markcorpse));
                                        }

                                    }
                                    else
                                    {
                                        if (Markable.Mark!=null)
                                        {
                                            if (Markable.Mark.Downed)
                                            {
                                            //    Log.Message(string.Format("{0}'s Mark is Downed", p.LabelShortCap));
                                            }
                                            else
                                            {
                                            //    Log.Message(string.Format("{0}'s Mark is Not Downed", p.LabelShortCap));
                                            }
                                        }
                                        else
                                        {
                                        //    Log.Message(string.Format("{0}'s Mark is null", p.LabelShortCap));
                                            p.health.RemoveHediff(hediff);
                                            if (Markable.MarkerRace)
                                            {
                                                p.health.AddHediff(Markable.Unmarkeddef, Markable.partRecord);
                                            }
                                            else
                                            {
                                                
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                        //    Log.Message(string.Format("{0}'s target corpse, is null", p.LabelShortCap));
                        }
                    }
                    /*
                    List<Hediff> hediffs = p.health.hediffSet.hediffs.FindAll(x => x.TryGetComp<HediffComp_BloodedYautja>() != null);
                    foreach (var x in hediffs)
                    {
                        if (x.TryGetComp<HediffComp_BloodedYautja>() != null && x.TryGetComp<HediffComp_BloodedYautja>() is HediffComp_BloodedYautja _Blooded)
                        {

                        }
                    }
                    */
                }
                yield break;
            }
        }

        // Token: 0x06002BF5 RID: 11253 RVA: 0x00149A04 File Offset: 0x00147E04
        public override string GetLabel()
        {
            return "AvP_CanMarkSelf".Translate();
        }

        // Token: 0x06002BF6 RID: 11254 RVA: 0x00149A10 File Offset: 0x00147E10
        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Thing thing in this.SickPawns.Where(x=> x.GetType() == typeof(Pawn)))
            {
                Pawn pawn = thing as Pawn;
                if (pawn!=null)
                {
                    Comp_Markable _Markable = pawn.TryGetComp<Comp_Markable>();
                    if (_Markable != null)
                    {
                        if (pawn.health.hediffSet.HasHediff(_Markable.Props.cultureDef.UnmarkedHediff))
                        {
                            if ((_Markable.MarkerRace || _Markable.Inducted))
                            {
                                stringBuilder.AppendLine("    " + pawn.LabelShort + " " + "HMS_CanMarkThemself".Translate() + ":" + _Markable.Markcorpse.InnerPawn.NameShortColored);
                            }
                            else
                            {
                                List<string> vs = new List<string>();
                                foreach (ThingDef race in _Markable.markerRaces)
                                {
                                    string rl = race.LabelCap + ": ";
                                    string rc = string.Empty;
                                    PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Where(y => y.def == race).ToList().ForEach(z => rc += (rc == string.Empty ? z.NameShortColored : " ," + z.NameShortColored));
                                    vs.Add(rc.NullOrEmpty() ? rl : rc);

                                }
                                stringBuilder.AppendLine("    " + pawn.LabelShort + " " + "HMS_CanbeMarked".Translate() + ":" + _Markable.Markcorpse.InnerPawn.NameShortColored + " By: " + vs.ToCommaList());
                            }
                        }
                    }
                }
            }
            return "AvP_CanMarkSelfDesc".Translate(stringBuilder.ToString());
        }

        // Token: 0x06002BF7 RID: 11255 RVA: 0x00149B50 File Offset: 0x00147F50
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(this.SickPawns.ToList());
        }

        protected override void OnClick()
        {
            base.OnClick();

        }

        protected override Color BGColor => base.BGColor;
    }
}
