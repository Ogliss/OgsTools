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
    public class Comp_KillMarker : Comp_UsableCorpse
    {

        protected override string FloatMenuOptionLabel
        {
            get
            {
                return string.Format(base.Props.useLabel, this.parent.LabelCap);
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
        {
            HediffSet hediffSet = myPawn.health.hediffSet;
            Comp_Markable markable = null;
            if (!myPawn.Markable(out markable))
            {
            //    Log.Message(string.Format("myPawn.Markable: {0}", !myPawn.Markable(out markable)));
                yield break;
            }
            if (!hediffSet.hediffs.Any(x => x.def.defName.Contains(HunterMarkingSystem.Unmarkededkey)) || markable == null)
            {
            //    Log.Message(string.Format("HasHediff: {0}, markable: {1}", hediffSet.hediffs.Any(x => x.def.defName.Contains(HunterMarkingSystem.Unmarkededkey)), markable != null));
                yield break;
            }
            HediffDef UnmarkedDef = hediffSet.hediffs.Find(x => x.def.defName.Contains(HunterMarkingSystem.Unmarkededkey)).def ?? null;
            if (!markable.MarkerRace && !markable.Inducted)
            {
            //    Log.Message(string.Format("MarkerRace: {0}, Inducted: {1}", markable.MarkerRace, markable.Inducted));
                yield break;
            }
            if (!this.CanBeUsedBy(myPawn, out string failReason))
            {
            //    yield break;
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

        private new bool CanBeUsedBy(Pawn p, out string failReason)
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

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.skill = DefDatabase<SkillDef>.GetRandom();
        }
        /*
        public override string TransformLabel(string label)
        {
            return this.skill.LabelCap + " " + label;
        }
        */

        public override bool AllowStackWith(Thing other)
        {
            return false;
        }

        public SkillDef skill;
    }

}
