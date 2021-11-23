using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace ExtraHives
{
    public class CompProperties_SpawnerOnDamaged : CompProperties
    {
        public CompProperties_SpawnerOnDamaged()
        {
            this.compClass = typeof(CompSpawnerOnDamaged);
        }
        public float defaultPoints = 550f;
        public float minPoints = 300f;
        public float minTimeBetween = -1f;
        public PawnGroupKindDef factionGroupKindDef;
        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (factionGroupKindDef == null)
            {
                factionGroupKindDef = RimWorld.PawnGroupKindDefOf.Combat;
            }
        }

    }

    public class CompSpawnerOnDamaged : ThingComp
    {
        public CompProperties_SpawnerOnDamaged Props => (CompProperties_SpawnerOnDamaged)props;
        public FactionDef factionDef;
        public int lastSpawnTick = -1;
        public Faction faction = null;
        public Lord Lord => this.lord;

        public Faction OfFaction
        {
            get
            {
                if (faction == null)
                {
                    if (parent.Faction != null)
                    {
                        faction = parent.Faction;
                    }
                }
                return faction;
            }
            set
            {
                faction = value;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Lord>(ref this.lord, "defenseLord", false);
            Scribe_References.Look<Faction>(ref this.faction, "defenseFaction", false);
            Scribe_Values.Look<float>(ref this.pointsLeft, "PawnPointsLeft", 0f, false);
            Scribe_Values.Look<int>(ref this.lastSpawnTick, "lastSpawnTick", 0, false);
        }
        public override void CompTick()
        {
            base.CompTick();
            if (this.lastSpawnTick>0)
            {
                this.lastSpawnTick--;
            }
        }
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (absorbed)
            {
                return;
            }
            if (dinfo.Def.harmsHealth)
            {
                if (this.lord != null)
                {
                    this.lord.ReceiveMemo(CompSpawnerOnDamaged.MemoDamaged);
                }
                float num = (float)this.parent.HitPoints - dinfo.Amount;
                if ((num < (float)this.parent.MaxHitPoints * 0.98f && dinfo.Instigator != null && dinfo.Instigator.Faction != null) || num < (float)this.parent.MaxHitPoints * 0.9f)
                {
                    this.TrySpawnPawns();
                }
            }
            absorbed = false;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (parent.Faction == null)
                {
                    parent.SetFaction(OfFaction);
                    //    Log.Message("set parent faction to "+ this.parent.Faction);
                }
                if (this.pointsLeft == 0f)
                {
                    this.pointsLeft = Mathf.Max(Props.defaultPoints * 0.9f, Props.minPoints);
                    //    Log.Message("set pointsLeft to " + this.pointsLeft);
                }
                if (spawnablePawnKinds.NullOrEmpty())
                {
                    if (parent.Faction != null)
                    {
                        if (parent.Faction.def.pawnGroupMakers.Any(x => x.kindDef == this.Props.factionGroupKindDef))
                        {
                            spawnablePawnKinds = parent.Faction.def.pawnGroupMakers.Where(x => x.kindDef == this.Props.factionGroupKindDef).RandomElementByWeight(x => x.commonality).options;
                        }
                        else
                        {
                            spawnablePawnKinds = parent.Faction.def.pawnGroupMakers.Where(x => x.kindDef == RimWorld.PawnGroupKindDefOf.Combat || x.kindDef == RimWorld.PawnGroupKindDefOf.Settlement).RandomElementByWeight(x => x.commonality).options;
                        }
                    }
                }
            }
        }

        private void TrySpawnPawns()
        {
            if (this.lastSpawnTick>0)
            {
                return;
            }
            if (this.pointsLeft <= 0f)
            {
                return;
            }
            if (!this.parent.Spawned)
            {
                return;
            }
            if (this.lord == null)
            {
                LordJob_AssaultColony lordJob = new LordJob_AssaultColony(this.parent.Faction, false, false, false, false, false);
                this.lord = LordMaker.MakeNewLord(OfFaction, lordJob, this.parent.Map, null);
            }
            try
            {
                while (this.pointsLeft > 0f)
                {
                    if (!(from def in spawnablePawnKinds select def).TryRandomElementByWeight(x => x.selectionWeight, out PawnGenOption kind))
                    {
                        //    Log.Message(string.Format("kindDef: {0}", kind));
                        break;
                    }
                    if (!(from cell in GenAdj.CellsAdjacent8Way(this.parent)
                          where this.CanSpawnPawnAt(cell)
                          select cell).TryRandomElement(out IntVec3 center))
                    {
                        break;
                    }
                    //    Log.Message(string.Format("kindDef: {0}", kind));
                    PawnGenerationRequest request = new PawnGenerationRequest(kind.kind, faction, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, false, 1f, false, true, true, false, false, false, false);
                    Pawn pawn = PawnGenerator.GeneratePawn(request);
                    if (!GenPlace.TryPlaceThing(pawn, center, this.parent.Map, ThingPlaceMode.Near, null, null))
                    {
                        Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                        break;
                    }
                    this.lord.AddPawn(pawn);
                    this.pointsLeft -= pawn.kindDef.combatPower;
                }
            }
            finally
            {
                this.pointsLeft = 0f;
            }
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(this.parent.Map);
            if (Props.minTimeBetween>0)
            {
                this.lastSpawnTick = (int)(this.Props.minTimeBetween * 60000f / Find.Storyteller.difficulty.enemyReproductionRateFactor);
            }
        }

        private bool CanSpawnPawnAt(IntVec3 c)
        {
            return c.Walkable(this.parent.Map);
        }

        public float pointsLeft;
        private Lord lord;
        public static readonly string MemoDamaged = "ShipPartDamaged";
        private List<Faction> allFactions = new List<Faction>();
        public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();
    }
}
