using AlienRace;
using HunterMarkingSystem.ExtensionMethods;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem
{

    public class CompProperties_Markable : CompProperties
    {
        public CompProperties_Markable() => this.compClass = typeof(Comp_Markable);
        public HunterCultureDef cultureDef = null;
        public HediffDef Unblooded = null;
        public HediffDef Unmarked = null;
        public HediffDef MarkedBase = null;
        public List<ThingDef> markerRaceDefs = new List<ThingDef>();
        public List<ThingDef> allowedRaceDefs = new List<ThingDef>();
        public List<ThingDef> disallowedRaceDefs = new List<ThingDef>();
    }

    public class Comp_Markable : ThingComp
    {
        public CompProperties_Markable Props => (CompProperties_Markable)this.props;
    //    public HediffDef Unbloodeddef => Props.Unblooded ?? DefDatabase<HediffDef>.AllDefs.First(x=> x.defName.Contains("Hediff_Unblooded"));
        public HediffDef Unmarkeddef => Props.Unmarked ?? DefDatabase<HediffDef>.AllDefs.First(x=> x.defName.Contains("Hediff_BloodedUM"));
        public HediffDef Markeddef => Props.Unblooded ?? DefDatabase<HediffDef>.AllDefs.First(x=> x.defName.Contains("Hediff_BloodedM"));
        public Pawn pawn => (Pawn)parent;
        public bool alienRace => pawn.def is ThingDef_AlienRace;
        public List<ThingDef> markerRaces => Props.markerRaceDefs ?? new List<ThingDef>();
        public List<ThingDef> allowedRaces => Props.allowedRaceDefs ?? new List<ThingDef>();
        public List<ThingDef> disallowedRaces => Props.disallowedRaceDefs ?? new List<ThingDef>();
        public bool UseMarkerRace => !markerRaces.NullOrEmpty();
        public bool UseAllowedRaces => !allowedRaces.NullOrEmpty();
        public bool UseDisallowedRaces => !disallowedRaces.NullOrEmpty();
        public bool MarkerRace => markerRaces.Contains(pawn.def);
        public Pawn Mark;
        public int MarkId;
        public Corpse markcorpse;
        public int? CorpseId;
        public Corpse Markcorpse
        {
            get
            {
                if (Mark!=null)
                {
                    if (Mark.Dead && markcorpse == null)
                    {
                        if (Mark.Corpse!=null)
                        {
                            markcorpse = Mark.Corpse;
                            CorpseId = markcorpse.thingIDNumber;
                        }
                    }
                }
                else
                {
                    if (CorpseId!=null)
                    {
                        markcorpse = PawnsFinder.All_AliveOrDead.First(x => x.Corpse.thingIDNumber == CorpseId).Corpse;
                    }
                }
                return markcorpse;
            }
        }
        public Hediff markHediff;
        public MarkData markDataKill;
        public MarkData markDataKillNew;
        public bool blooded;
        public bool inducted => BloodStatus == BloodStatusMode.Marked;
        public bool Inducted => MarkerRace || inducted;

        public bool inductable
        {
            get
            {
                bool result = !Inducted && (allowedRaces.NullOrEmpty() || allowedRaces.Contains(pawn.def)) && (disallowedRaces.NullOrEmpty() || !disallowedRaces.Contains(pawn.def)) && blood >= BloodStatusMode.Unblooded;

            //    Log.Message(string.Format(pawn.Name.ToStringShort + " inductable: {0} = {1} && {2} && {3} && {4} && {5}", result, !Inducted, (allowedRaces.NullOrEmpty() || allowedRaces.Contains(pawn.def)), (disallowedRaces.NullOrEmpty() || !disallowedRaces.Contains(pawn.def)), blood == BloodStatusMode.Unblooded, MarkableCorpse));
                return result;
            }
        }

        public bool MarkableCorpse => (Markcorpse != null && !Markcorpse.Destroyed);
        private BloodStatusMode blood = BloodStatusMode.NoComp;
        public BloodStatusMode BloodStatus

        {
            get
            {
                if (this.blood == BloodStatusMode.NoComp)
                {
                    if (pawn.health.hediffSet.hediffs.Any(x=> HunterMarkingSystem.UnbloodedHediffList.Contains(x.def)))
                    {
                        this.blood = (BloodStatusMode.Unblooded);
                    }
                    else
                    if (pawn.health.hediffSet.hediffs.Any(x => HunterMarkingSystem.BloodedUMHediffList.Contains(x.def)))
                    {
                        this.blood = (BloodStatusMode.Unmarked);
                    }
                    else
                    if (pawn.health.hediffSet.hediffs.Any(x => HunterMarkingSystem.BloodedMHediffList.Contains(x.def)))
                    {
                        this.blood = (BloodStatusMode.Marked);
                    }
                    else
                    {
                        this.blood = (BloodStatusMode.None);
                    }

                }
                else
                {
                    if (pawn.health.hediffSet.hediffs.Any(x => HunterMarkingSystem.BloodedMHediffList.Contains(x.def)))
                    {
                        this.blood = (BloodStatusMode.Marked);
                    }
                    else
                    if (pawn.health.hediffSet.hediffs.Any(x => HunterMarkingSystem.BloodedUMHediffList.Contains(x.def)))
                    {
                        this.blood = (BloodStatusMode.Unmarked);
                    }
                    else
                    if (pawn.health.hediffSet.hediffs.Any(x => HunterMarkingSystem.UnbloodedHediffList.Contains(x.def)))
                    {
                        this.blood = (BloodStatusMode.Unblooded);
                    }
                }
                /*
                if (blood <= (BloodStatusMode)1)
                {
                //    Log.Message("Markable missing hediff, restting to unblooded");
                    pawn.health.AddHediff(Unbloodeddef, partRecord);
                    blood = BloodStatusMode.Unblooded;
                }
                */
                return this.blood;
            }
            set
            {
                blood = value;
            }
        }
        public BodyPartRecord partRecord
        {
            get
            {
                foreach (var part in ((Pawn)parent).RaceProps.body.AllParts.Where(x => x.def.defName.Contains( "Head") && x.GetChildParts(BodyPartTagDefOf.ConsciousnessSource) != null))
                {
                    return part;
                }
                return null;
            }
        }


        public float MarkScore => markDataKill!=null? markDataKill.MarkScore : (MyScore*Settings.SettingsHelper.latest.MinWorthyKill);
        public float MyScore => pawn.GetStatValue(StatDef.Named("HMS_MarkScore"));

        public override string CompInspectStringExtra()
        {
            return base.CompInspectStringExtra();
        }

        public override string GetDescriptionPart()
        {
            return base.GetDescriptionPart() + string.Format("\n{0}", BloodStatus.GetLabel(pawn));
        }
        /*
        public override string TransformLabel(string label)
        {
            label = label + BloodStatus.ToString() + " " + markDataKill.raceDef.LabelCap;
            return base.TransformLabel(label); 
        }
        */
        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look<Corpse>(ref this.markcorpse, "markcorpse");
            Scribe_References.Look<Pawn>(ref this.Mark, "Mark");
            Scribe_Deep.Look(ref this.markDataKill, "killedData");
            Scribe_Deep.Look(ref this.markDataKillNew, "killedDataNew");
            Scribe_Values.Look(ref this.blooded, "thisblooded");
        //    Scribe_Values.Look(ref this.inducted, "inducted");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            /*
            if (blood <= (BloodStatusMode)1)
            {
            //    Log.Message("Markable missing hediff, restting to unblooded");
                pawn.health.AddHediff(Unbloodeddef, partRecord);
                blood = BloodStatusMode.Unblooded;
            }
            */
            if (!respawningAfterLoad)
            {
                if (markDataKill == null)
                {
                    if (BloodStatus == BloodStatusMode.Marked)
                    {
                        Rand.PushState();
                        int rand = Rand.RangeInclusive(0, HunterMarkingSystem.RaceDefaultMarkDict.Count - 1);
                        Rand.PopState();
                        markDataKill = HunterMarkingSystem.RaceDefaultMarkDict.ElementAt(rand).Value;
                        if (markDataKill == null)
                        {
                            Log.Warning(string.Format("markData null for {0} pawn {1}", BloodStatus.ToString(), pawn.Name.ToStringShort));
                        }
                    }
                }
                else
                {
                    //    Log.Message(string.Format("markData exists for {0} pawn {1}, {2}", BloodStatus.ToString(), pawn.Name.ToStringShort, markDataKill.Label));
                }
            }
        }
        
        public JobDef useJob = HMSDefOf.HMS_Job_MarkOther;
        public string useLabel = "HMS_MarkOtherLabel".Translate();
        // (get) Token: 0x06002942 RID: 10562 RVA: 0x001394F0 File Offset: 0x001378F0
        protected string FloatMenuOptionLabel
        {
            get
            {
                return string.Format(useLabel, this.Markcorpse.LabelShortCap, this.parent.LabelShortCap);
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if ((Props.markerRaceDefs.NullOrEmpty() || Props.markerRaceDefs.Contains(selPawn.def) || (selPawn.Markable(out Comp_Markable Markable) && Markable.MarkerRace)) && inductable && !this.Markcorpse.DestroyedOrNull())
            {
                FloatMenuOption useopt = new FloatMenuOption(this.FloatMenuOptionLabel, delegate ()
                {
                    if (selPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                    {
                        this.TryStartUseJob(selPawn);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return useopt;
            }
            foreach (var item in base.CompFloatMenuOptions(selPawn))
            {
                yield return item;
            }
            yield break;
        }

        // Token: 0x06002A4B RID: 10827 RVA: 0x00138F78 File Offset: 0x00137378
        public void TryStartUseJob(Pawn user)
        {
            if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                return;
            }
            if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                return;
            }
            Job job = new Job(this.useJob,this.Markcorpse, this.parent);
            user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        public override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);
        }

        public override void Notify_SignalReceived(Signal signal)
        {
            base.Notify_SignalReceived(signal);
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
        }


    }

}
