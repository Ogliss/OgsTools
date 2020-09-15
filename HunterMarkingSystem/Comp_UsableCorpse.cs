using HunterMarkingSystem.ExtensionMethods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace HunterMarkingSystem
{
    // Token: 0x0200025E RID: 606
    public class CompProperties_UsableCorpse : CompProperties
    {
        // Token: 0x06000ACC RID: 2764 RVA: 0x00056399 File Offset: 0x00054799
        public CompProperties_UsableCorpse()
        {
            this.compClass = typeof(Comp_UsableCorpse);
        }

        // Token: 0x040004D8 RID: 1240
        public JobDef useJob;

        // Token: 0x040004D9 RID: 1241
        [MustTranslate]
        public string useLabel;

        // Token: 0x040004DA RID: 1242
        public int useDuration = 100;
    }

    // Token: 0x02000774 RID: 1908
    public class Comp_UsableCorpse : ThingComp
    {
        // Token: 0x1700068E RID: 1678
        // (get) Token: 0x06002A48 RID: 10824 RVA: 0x00138F32 File Offset: 0x00137332
        public CompProperties_UsableCorpse Props
        {
            get
            {
                return (CompProperties_UsableCorpse)this.props;
            }
        }

        // Token: 0x1700068F RID: 1679
        // (get) Token: 0x06002A49 RID: 10825 RVA: 0x00138F3F File Offset: 0x0013733F
        protected virtual string FloatMenuOptionLabel
        {
            get
            {
                return this.Props.useLabel;
            }
        }

        // Token: 0x06002A4A RID: 10826 RVA: 0x00138F4C File Offset: 0x0013734C
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            if (!this.CanBeUsedBy(myPawn, out string failReason))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel + ((failReason == null) ? string.Empty : (" (" + failReason + ")")), null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.CanReserve(this.parent, 1, -1, null, false))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel + " (" + "Reserved".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                yield return new FloatMenuOption(this.FloatMenuOptionLabel + " (" + "Incapable".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
            {
                FloatMenuOption useopt = new FloatMenuOption(this.FloatMenuOptionLabel, delegate ()
                {
                    if (myPawn.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
                    {
                        foreach (CompUseEffect compUseEffect in this.parent.GetComps<CompUseEffect>())
                        {
                            if (compUseEffect.SelectedUseOption(myPawn))
                            {
                                return;
                            }
                        }
                        this.TryStartUseJob(myPawn);
                    }
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return useopt;
            }
            yield break;
        }

        // Token: 0x06002A4D RID: 10829 RVA: 0x00139094 File Offset: 0x00137494
        public bool CanBeUsedBy(Pawn p, out string failReason)
        {
            List<ThingComp> allComps = this.parent.AllComps;
            for (int i = 0; i < allComps.Count; i++)
            {
                if (allComps[i] is CompUseEffect compUseEffect && !compUseEffect.CanBeUsedBy(p, out failReason))
                {
                    return false;
                }
            }
            failReason = null;
            return true;
        }

        // Token: 0x06002A4B RID: 10827 RVA: 0x00138F78 File Offset: 0x00137378
        public void TryStartUseJob(Pawn user)
        {
            if (!user.CanReserveAndReach(this.parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, false))
            {
                return;
            }
            if (!this.CanBeUsedBy(user, out string text))
            {
                return;
            }
            Job job = new Job(this.Props.useJob, this.parent);
            user.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }

        // Token: 0x06002A4C RID: 10828 RVA: 0x00138FDC File Offset: 0x001373DC
        public void UsedBy(Pawn p)
        {
            if (!this.CanBeUsedBy(p, out string text))
            {
            //    Log.Message(text);
                return;
            }
            foreach (CompUseEffect compUseEffect in from x in this.parent.GetComps<CompUseEffect>()
                                                    orderby x.OrderPriority descending
                                                    select x)
            {
                try
                {
                    compUseEffect.DoEffect(p);
                }
                catch
                {
                    //    Log.Error("Error in CompUseEffect: " + arg, false);
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Any(x=> x.Markable(out Comp_Markable markable) && x.health.hediffSet.HasHediff(markable.Unmarkeddef) && markable.MarkableCorpse && markable.markcorpse == this.parent) &&!respawningAfterLoad)
            {
                this.parent.SetForbidden(true, false);
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Any(x => x.Markable(out Comp_Markable markable) && x.health.hediffSet.HasHediff(markable.Unmarkeddef)))
            {
                foreach (Pawn p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Where(x => x.Markable(out Comp_Markable markable) && x.health.hediffSet.HasHediff(markable.Unmarkeddef)))
                {
                    Hediff marked = null;
                    if (p.Markable(out Comp_Markable markable))
                    {
                        if (markable.markcorpse != null)
                        {
                            if (markable.markcorpse == this.parent && this.parent.DestroyedOrNull())
                            {

                                if (!p.Marked(out marked) && marked != null)
                                {
                                    p.health.RemoveHediff(marked);
                                    if (markable.MarkerRace)
                                    {
                                        p.health.AddHediff(markable.Props.cultureDef.UnbloodedHediff);
                                    }
                                }
                                else if (p.Marked(out marked, out Hediff unmarked) && unmarked != null)
                                {
                                    p.health.RemoveHediff(unmarked);
                                }
                            }
                        }
                    }
                }
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();

        }
    }
}
