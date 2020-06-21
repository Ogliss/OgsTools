using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;

namespace CompPawnGrower
{

    public class CompProperties_PawnGrower : CompProperties
    {
        public CompProperties_PawnGrower()
        {
            this.compClass = typeof(Comp_PawnGrower);
        }

        public bool canspawn = true;
        public bool spawnwild = true;
        public float spawnChance = 1f;
        public List<RaceGenOption> Races = new List<RaceGenOption>();
        public List<PawnGenOption> Pawnkinds = new List<PawnGenOption>();
    }

    public class Comp_PawnGrower : ThingComp
    {
        public CompProperties_PawnGrower Props
        {
            get
            {
                return (CompProperties_PawnGrower)this.props;
            }
        }

        public Plant plant => base.parent as Plant;

        public bool canspawn => plant.HarvestableNow && Props.canspawn;

        public bool spawnwild => Props.spawnwild;
        public float spawnChance => Props.spawnChance;

        private float age = 0;
        public float Age
        {
            get
            {
                if (plant != null)
                {
                    age = plant.Age;
                }
                return age;
            }
        }
        private float fertility = 0;
        public float Fertility
        {
            get
            {
                if (plant != null)
                {
                    if (plant.Map != null)
                    {
                        fertility = plant.GrowthRateFactor_Fertility;
                    }
                }
                return fertility;
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (canspawn)
            {
                var spawnRoll = Rand.Value;
                if (spawnRoll < (spawnChance * plant.Growth))
                {
                    if (Props.Pawnkinds.NullOrEmpty())
                    {
                        if (Props.Races.NullOrEmpty())
                        {
                            Log.Error("Comp_PawnGrower: No race or kinddefs set in "+ parent.def.defName);
                            return;
                        }
                        ThingDef race = Props.Races.RandomElementByWeight(x => x.selectionWeight).thingdef;
                        try
                        {
                            pawnKindDef = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race == race).RandomElement();
                        }
                        catch (Exception)
                        {
                            Log.Error("Comp_PawnGrower: No kinddefs found for " + race.defName);
                            return;
                        }
                    }
                    else
                    {
                        pawnKindDef = Props.Pawnkinds.RandomElementByWeight(x => x.selectionWeight).kind;
                    }
                    faction = spawnwild ? null : Faction.OfPlayer;
                    generationContext = spawnwild ? PawnGenerationContext.NonPlayer : PawnGenerationContext.NonPlayer;
                    PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest(pawnKindDef, faction, generationContext, -1, true, true, false, false, true, true, 0f, fixedGender: Gender.None, fixedBiologicalAge: Age, fixedChronologicalAge: Age);

                    Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);

                    if (pawnKindDef.RaceProps.Humanlike)
                    {
                        if (!spawnwild && Faction.OfPlayer.def.basicMemberKind != null)
                        {
                            pawn.ChangeKind(Faction.OfPlayer.def.basicMemberKind);
                        }
                        else
                        {
                            pawn.ChangeKind(PawnKindDefOf.WildMan);
                        }
                        pawn.story.bodyType = pawn.story.childhood.BodyTypeFor(pawn.gender);
                    }
                    if (Fertility < 1f)
                    {
                        foreach (Need need in pawn.needs.AllNeeds)
                        {
                            need.CurLevel = 0f;
                        }
                        Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.Malnutrition, pawn);
                        hediff.Severity = Math.Min(1f - Fertility, 0.75f);
                        pawn.health.AddHediff(hediff);
                    }
                    else
                    {
                        foreach (Need need in pawn.needs.AllNeeds)
                        {
                            need.CurLevel = Fertility - 1f;
                        }
                    }
                    GenSpawn.Spawn(pawn, base.parent.Position, map, 0);
                }
            }
            base.PostDeSpawn(map);
        }

        public PawnKindDef pawnKindDef;

        public Faction faction;

        public PawnGenerationContext generationContext;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.age, "PlantAge");
            Scribe_Values.Look(ref this.fertility, "PlantFertility");
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (age == 0)
            {
                age = Age;
            }
            if (fertility == 0)
            {
                fertility = Fertility;
            }
        }
    }

}
