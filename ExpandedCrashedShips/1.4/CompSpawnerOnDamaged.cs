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

namespace CrashedShipsExtension
{
    // Token: 0x0200025A RID: 602
    public class CompProperties_SpawnerOnDamaged : CompProperties
    {
        // Token: 0x06000AC8 RID: 2760 RVA: 0x000562D4 File Offset: 0x000546D4
        public CompProperties_SpawnerOnDamaged()
        {
            this.compClass = typeof(CompSpawnerOnDamaged);
        }
        public FactionDef Faction;
        public Faction faction;
        public List<PawnGenOption> allowedKinddefs = new List<PawnGenOption>();
        public List<PawnKindDef> disallowedKinddefs = new List<PawnKindDef>();
        public List<FactionDef> Factions = new List<FactionDef>();
        public List<FactionDef> disallowedFactions = new List<FactionDef>();
        public String techLevel;
        public bool allowHidden = true;
        public bool allowNonHumanlike = false;
        public bool allowDefeated = true;
        public ThingDef skyFaller;
        public float defaultPoints = 550f;
        public float minPoints = 300f;

        public PawnGroupKindDef factionGroupKindDef;
        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (factionGroupKindDef == null)
            {
                factionGroupKindDef = PawnGroupKindDefOf.Combat;
            }
        }
    }

    // Token: 0x02000769 RID: 1897
    public class CompSpawnerOnDamaged : ThingComp
    {
        public CompProperties_SpawnerOnDamaged Props => (CompProperties_SpawnerOnDamaged)props;
        public FactionDef factionDef;
        public Faction faction = null;
        public Lord Lord => this.lord;
        public List<Faction> AllFactions
        {
            get
            {
                List<Faction> allfactions = Find.FactionManager.AllFactionsListForReading;
                List<Faction> factions = Find.FactionManager.AllFactionsListForReading;
                if (Props.disallowedFactions != null)
                {
                    foreach (var i in allfactions)
                    {
                        if (!Props.disallowedFactions.Contains(i.def))
                        {
                            factions.Remove(i);
                        }
                    }
                }
                return factions;
            }
        }

        public TechLevel techLevel
        {
            get
            {
                if (Props.techLevel != null)
                {
                    if (Props.techLevel == "Animal")
                    {
                        return TechLevel.Animal;
                    }
                    if (Props.techLevel == "Archotech")
                    {
                        return TechLevel.Archotech;
                    }
                    if (Props.techLevel == "Industrial")
                    {
                        return TechLevel.Industrial;
                    }
                    if (Props.techLevel == "Medieval")
                    {
                        return TechLevel.Medieval;
                    }
                    if (Props.techLevel == "Neolithic")
                    {
                        return TechLevel.Neolithic;
                    }
                    if (Props.techLevel == "Spacer")
                    {
                        return TechLevel.Spacer;
                    }
                    if (Props.techLevel == "Ultra")
                    {
                        return TechLevel.Ultra;
                    }
                    else return TechLevel.Undefined;
                }
                else return TechLevel.Undefined;
            }
        }

        public Faction OfFaction
        {
            get
            {
                if (faction == null)
                {
                    if (parent.Faction != null)
                    {
                        faction = parent.Faction;
                        return faction;
                    }
                    if (Props.Faction != null)
                    {
                        //    Log.Message(string.Format("Loading Faction Def from CompProps"));
                        factionDef = Props.Faction;
                        Faction f = Find.FactionManager.FirstFactionOfDef(factionDef);
                        if (f != null)
                        {
                            faction = Find.FactionManager.FirstFactionOfDef(factionDef);
                            return faction;
                        }
                        //    Log.Message(string.Format("Owner: {0} Def of:{1}", Find.FactionManager.FirstFactionOfDef(factionDef), factionDef));
                    }

                    if (Props.Factions.Count > 0)
                    {
                        //   Log.Message(string.Format("Loading Faction List from CompProps"));
                        factionDef = Props.Factions.Where(x=> Find.FactionManager.FirstFactionOfDef(x) != null).RandomElement<FactionDef>();
                        if (factionDef != null)
                        {
                            faction = Find.FactionManager.FirstFactionOfDef(factionDef);
                        }
                        //    Log.Message(string.Format("Owner: {0} Def of:{1}", Find.FactionManager.FirstFactionOfDef(factionDef), factionDef));
                    }
                    if (faction == null)
                    {
                        //    Log.Message(string.Format("Getting Random Enemy Faction"));
                        faction = RandomEnemyFaction(Props.allowHidden, Props.allowDefeated, Props.allowNonHumanlike, techLevel);
                        factionDef = faction.def;
                        Props.faction = faction;
                        //    Log.Message(string.Format("Owner: {0} Def of:{1}", faction.Name, factionDef));
                    }
                }
                return faction;
            }
            set
            {
                faction = value;
                this.parent.SetFactionDirect(value);
            }
        }

        public Faction RandomEnemyFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
        {
            Faction result;
            if ((from x in Find.FactionManager.GetFactions(allowHidden, allowDefeated, allowNonHumanlike, minTechLevel, false)
                 where x.HostileTo(Faction.OfPlayer) && x.RandomPawnKind() != null
                 select x).TryRandomElement(out result))
            {
                return result;
            }
            return null;
        }
        // Token: 0x060029EB RID: 10731 RVA: 0x0013D92F File Offset: 0x0013BD2F
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<Lord>(ref this.lord, "defenseLord", false);
            Scribe_References.Look<Faction>(ref this.faction, "defenseFaction", false);
            Scribe_Values.Look<float>(ref this.pointsLeft, "PawnPointsLeft", 0f, false);
        }
        public static float Inverse(float val) => 1f / val;

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
                if (parent.Faction == null)
                {
                    parent.SetFactionDirect(OfFaction);
                //    Log.Message("set parent faction to "+ this.parent.Faction+" of "+ parent.Faction.def.LabelCap);
                    if (spawnablePawnKinds.NullOrEmpty())
                    {
                        if (!this.Props.allowedKinddefs.NullOrEmpty())
                        {
                            spawnablePawnKinds = this.Props.allowedKinddefs;
                        }
                        else
                        {
                            if (parent.Faction != null)
                            {
                                if (parent.Faction.def.pawnGroupMakers.NullOrEmpty())
                                {
                                    List<PawnKindDef> kinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(x => x.isFighter && x.defaultFactionType != null && x.defaultFactionType == parent.Faction.def).ToList();

                                    for (int i = 0; i < kinds.Count(); i++)
                                    {
                                        spawnablePawnKinds.Add(new PawnGenOption(kinds[i], Inverse(kinds[i].combatPower)));
                                    }
                                }
                                else
                                {
                                    List<RimWorld.PawnGenOption> opts = new List<RimWorld.PawnGenOption>();

                                    if (parent.Faction.def.pawnGroupMakers.Any(x => x.kindDef == this.Props.factionGroupKindDef))
                                    {
                                        opts = parent.Faction.def.pawnGroupMakers.Where(x => x.kindDef == this.Props.factionGroupKindDef).RandomElementByWeight(x => x.commonality).options;
                                    }
                                    else
                                    {
                                        opts = parent.Faction.def.pawnGroupMakers.Where(x => x.kindDef == RimWorld.PawnGroupKindDefOf.Combat || x.kindDef == RimWorld.PawnGroupKindDefOf.Settlement).RandomElementByWeight(x => x.commonality).options;
                                    }
                                    for (int i = 0; i < opts.Count(); i++)
                                    {
                                        spawnablePawnKinds.Add(new PawnGenOption(opts[i]));
                                    }
                                }
                            }
                        }
                    }

                }
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
                if (this.pointsLeft == 0f)
                {
                    this.pointsLeft = Mathf.Max(Props.defaultPoints * 0.9f, Props.minPoints);
                    //    Log.Message("set pointsLeft to " + this.pointsLeft);
                }
            }
        }

        // Token: 0x060029EE RID: 10734 RVA: 0x0013DA2C File Offset: 0x0013BE2C
        private void TrySpawnPawns()
        {

            IEnumerable<PawnGenOption> source = spawnablePawnKinds;
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
                if (!CellFinder.TryFindRandomCellNear(this.parent.Position, this.parent.Map, 5, (IntVec3 c) => c.Standable(this.parent.Map) && this.parent.Map.reachability.CanReach(c, this.parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out IntVec3 invalid, -1))
                {
                    Log.Error("Found no place for Pawns to defend " + this);
                    invalid = IntVec3.Invalid;
                }
                LordJob_PawnsDefendShip lordJob = new LordJob_PawnsDefendShip(this.parent, this.parent.Faction, 21f, invalid);
                this.lord = LordMaker.MakeNewLord(OfFaction, lordJob, this.parent.Map, null);
            }
            try
            {
                while (this.pointsLeft > 0f)
                {
                    if (!(from def in source select def).TryRandomElementByWeight( x=> x.selectionWeight, out PawnGenOption kind))
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
                    PawnGenerationRequest request = new PawnGenerationRequest(kind.kind, faction, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, 1f, false, true, true, false, false, false, false);
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

        // Token: 0x04001748 RID: 5960
        private const float PawnsDefendRadius = 21f;

        // Token: 0x04001749 RID: 5961
        public static readonly string MemoDamaged = "ShipPartDamaged";

        // Token: 0x04000FB7 RID: 4023
        private List<Faction> allFactions = new List<Faction>();
        public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();
    }
}
