using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace HunterMarkingSystem
{
    public class MarkData : IExposable
    {
        public MarkData()
        {
            this.kindDef = null;
            this.raceDef = null;
        }

        public MarkData(Pawn p)
        {
            this.kindDef = p.kindDef;
            this.faction = p.Faction;
            this.raceDef = p.def;
            if (Humanlike)
            {
                this.Name = p.Name.ToStringShort;
            }
            if (this.faction != null)
            {
                this.factionDef = this.faction.def;
            }
            this.MarkScore = p.GetStatValue(StatDef.Named("HMS_MarkScore"));
            /*
            this.Label = p.kindDef.LabelCap;
            this.Humanlike = p.RaceProps.Humanlike;
            this.MarkDef = HMSUtility.GetMark(p);
            this.MarkDefs = HMSUtility.GetMarks(p);
            */
        }

        public MarkData(PawnKindDef p)
        {
            this.kindDef = p;
            if (p.defaultFactionType != null)
            {
                this.factionDef = p.defaultFactionType;
            }
            this.raceDef = p.race;
            this.MarkScore = p.race.GetStatValueAbstract(StatDef.Named("HMS_MarkScore"));
            /*
            this.Label = p.LabelCap;
            this.Humanlike = p.RaceProps.Humanlike;
            this.MarkDef = HMSUtility.GetMark(p);
            this.MarkDefs = HMSUtility.GetMarks(p);
            */
        }
        public MarkData(ThingDef p)
        {
            if (DefDatabase<PawnKindDef>.AllDefs.Any(x => x.race == p))
            {
                this.kindDef = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race == p).Any() ? DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race == p).RandomElement() : null;
            }
            else
            {
                Log.Warning(string.Format("Warning, no pawnkinds found for {0} Race def: {1}", p.LabelCap, p.defName));
            }
            this.raceDef = p;
            this.MarkScore = p.GetStatValueAbstract(StatDef.Named("HMS_MarkScore"));
            /*
            this.Label = p.LabelCap;
            this.Humanlike = p.race.Humanlike;
            this.MarkDef = HMSUtility.GetMark(p);
            this.MarkDefs = HMSUtility.GetMarks(p);
            */
        }

        public HediffDef MarkDef => raceDef !=null ? HMSUtility.GetMark(raceDef) : null;
        public List<HediffDef> MarkDefs => raceDef != null ? HMSUtility.GetMarks(raceDef) : null;
        public PawnKindDef kindDef = null;
        public ThingDef raceDef = null;
        public FactionDef factionDef = null;
        public Faction faction = null;
        public bool Humanlike => raceDef != null ? raceDef.race.Humanlike :false;
        public string Label => raceDef != null ? raceDef.LabelCap : TaggedString.Empty;
        public string Name;
        public float MarkScore;

        public void ExposeData()
        {
            /*
            Scribe_Values.Look(ref this.Label, "Label", string.Empty);
            Scribe_Defs.Look(ref this.MarkDef, "MarkDef");
            Scribe_Collections.Look(ref this.MarkDefs, "MarkDefs", LookMode.Def);
            */
            Scribe_Values.Look(ref this.Name, "Name", string.Empty);
            Scribe_Defs.Look(ref this.kindDef, "kindDef");
            Scribe_Defs.Look(ref this.raceDef, "raceDef");
            Scribe_Defs.Look(ref this.factionDef, "factionDef");
            Scribe_References.Look(ref this.faction, "faction");
            Scribe_Values.Look(ref this.MarkScore, "MarkScore", 0f);
        }
    }

}
