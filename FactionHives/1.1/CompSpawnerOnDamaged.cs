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
    // Token: 0x0200025A RID: 602
    public class CompProperties_SpawnerOnDamaged : CompProperties
    {
        // Token: 0x06000AC8 RID: 2760 RVA: 0x000562D4 File Offset: 0x000546D4
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

    // Token: 0x02000769 RID: 1897
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

        // Token: 0x060029EB RID: 10731 RVA: 0x0013D92F File Offset: 0x0013BD2F
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
        // Token: 0x060029EC RID: 10732 RVA: 0x0013D960 File Offset: 0x0013BD60
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

        // Token: 0x060029EE RID: 10734 RVA: 0x0013DA2C File Offset: 0x0013BE2C
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

        // Token: 0x060029EF RID: 10735 RVA: 0x0013DC44 File Offset: 0x0013C044
        private bool CanSpawnPawnAt(IntVec3 c)
        {
            return c.Walkable(this.parent.Map);
        }

        // Token: 0x04001746 RID: 5958
        public float pointsLeft;

        // Token: 0x04001747 RID: 5959
        private Lord lord;

        // Token: 0x04001749 RID: 5961
        public static readonly string MemoDamaged = "ShipPartDamaged";

        // Token: 0x04000FB7 RID: 4023
        private List<Faction> allFactions = new List<Faction>();
        public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();
    }
}
