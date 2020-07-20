using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Metamorphasis
{
    // Token: 0x02000241 RID: 577
    public class HediffCompProperties_MetroidEvoloution : HediffCompProperties
    {
        public HediffCompProperties_MetroidEvoloution()
        {
            this.compClass = typeof(HediffComp_MetroidEvoloution);
        }

        public List<MetroidWhitelistDef> whitelists = new List<MetroidWhitelistDef>();
        public ThingDef huskDef = null;
        public List<MetroidEvolutionPath> PossibleEvolutionPaths = new List<MetroidEvolutionPath>();

    }

    public class HediffComp_MetroidEvoloution : HediffComp
    {
        public HediffCompProperties_MetroidEvoloution Props
        {
            get
            {
                return (HediffCompProperties_MetroidEvoloution)this.props;
            }
        }

        private void TransformPawn(PawnKindDef kindDef, bool changeDef = true, bool keep = false)
        {
            //sets position, faction and map
            IntVec3 intv = parent.pawn.Position;
            Faction faction = parent.pawn.Faction;
            Map map = parent.pawn.Map;
            RegionListersUpdater.DeregisterInRegions(parent.pawn, map);

            //Change Race to Props.raceDef
            if (changeDef && kindDef != null && kindDef != parent.pawn.kindDef)
            {
                parent.pawn.def = kindDef.race;
                parent.pawn.kindDef = kindDef;
                long ageB = Pawn.ageTracker.AgeBiologicalTicks;
                long ageC = Pawn.ageTracker.AgeChronologicalTicks;
                Pawn.ageTracker = new Pawn_AgeTracker(Pawn);
                Pawn.ageTracker.AgeBiologicalTicks = ageB;
                Pawn.ageTracker.AgeChronologicalTicks = ageC;

            }
            RegionListersUpdater.RegisterInRegions(parent.pawn, map);
            map.mapPawns.UpdateRegistryForPawn(parent.pawn);

            //decache graphics
            parent.pawn.Drawer.renderer.graphics.ResolveAllGraphics();

            // remove non whitelisted hediffs
            if (!Pawn.health.hediffSet.hediffs.NullOrEmpty())
            {
                if (!Props.whitelists.NullOrEmpty())
                {
                    foreach (MetroidWhitelistDef list in Props.whitelists)
                    {
                        if (parent.pawn.health.hediffSet.hediffs.Any(x => !list.whitelist.Contains(x.def) && x != this.parent))
                        {
                            List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs.Where(x => !list.whitelist.Contains(x.def) && x != this.parent).ToList();
                            foreach (Hediff item in removeable)
                            {
                                if (item != this.parent)
                                {
                                    Pawn.health.RemoveHediff(item);
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<Hediff> removeable = parent.pawn.health.hediffSet.hediffs;
                    foreach (Hediff item in removeable)
                    {
                        if (item != this.parent)
                        {
                            Pawn.health.RemoveHediff(item);
                        }
                    }
                }
            }

            //save the pawn
            parent.pawn.ExposeData();
            if (parent.pawn.Faction != faction)
            {
                parent.pawn.SetFaction(faction);
            }
            //spawn Husk if set
            if (Props.huskDef != null)
            {
                GenSpawn.Spawn(ThingMaker.MakeThing(Props.huskDef), parent.pawn.Position, parent.pawn.Map);
            }

        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            MetroidEvolutionPath path = null;
            if (Props.PossibleEvolutionPaths.Any(x => x.triggerDef != null && Pawn.health.hediffSet.HasHediff(x.triggerDef)))
            {
                path = Props.PossibleEvolutionPaths.First(x => x.triggerDef != null && Pawn.health.hediffSet.HasHediff(x.triggerDef));
            }
            else if (Props.PossibleEvolutionPaths.Any(x => x.triggerDef == null))
            {
                path = Props.PossibleEvolutionPaths.FindAll(x => x.triggerDef == null).RandomElement();
            }
            if (path != null)
            {
                if (Pawn.ageTracker.AgeBiologicalYearsFloat > path.Age)
                {
                    TransformPawn(path.Kind);
                }
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look<int>(ref this.ticksToDisappear, "ticksToDisappear", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToDisappear: " + this.ticksToDisappear;
        }

        public int ticksToDisappear = 60;
    }

    public class MetroidEvolutionPath
    {
        public HediffDef triggerDef;
        public float Age = 0f;
        public PawnKindDef Kind;
    }
    public class MetroidWhitelistDef : Def
    {
        public List<HediffDef> whitelist = new List<HediffDef>();
    }
}
