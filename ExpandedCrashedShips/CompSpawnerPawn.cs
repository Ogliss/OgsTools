using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace CrashedShipsExtension
{
	// Token: 0x02000DE7 RID: 3559
	public class CompProperties_SpawnerPawn : CompProperties
	{
		// Token: 0x06005730 RID: 22320 RVA: 0x001D2EB0 File Offset: 0x001D10B0
		public CompProperties_SpawnerPawn()
		{
			this.compClass = typeof(CompSpawnerPawn);
		}

		public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();
		public List<PawnKindDef> AlwaysSpawnWith = new List<PawnKindDef>();
		public SoundDef spawnSound;
		public string spawnMessageKey;
		public string noPawnsLeftToSpawnKey;
		public string pawnsLeftToSpawnKey;
		public bool showNextSpawnInInspect;
		public bool shouldJoinParentLord;
		public Type lordJob;
		public float defendRadius = 21f;
		public int initialSpawnDelay = 120;
		public int initialPawnsCount;
		public float initialPawnsPoints;
		public float maxSpawnedPawnsPoints = -1f;
		public FloatRange pawnSpawnIntervalDays = new FloatRange(0.85f, 1.15f);
		public int pawnSpawnRadius = 2;
		public IntRange maxPawnsToSpawn = IntRange.zero;
		public bool chooseSingleTypeToSpawn;
		public string nextSpawnInspectStringKey;
		public string nextSpawnInspectStringKeyDormant;
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
	// Token: 0x02000DE8 RID: 3560
	public class CompSpawnerPawn : ThingComp
	{
		// Token: 0x17000F66 RID: 3942
		// (get) Token: 0x06005731 RID: 22321 RVA: 0x001D2F10 File Offset: 0x001D1110
		private CompProperties_SpawnerPawn Props
		{
			get
			{
				return (CompProperties_SpawnerPawn)this.props;
			}
		}

		// Token: 0x17000F67 RID: 3943
		// (get) Token: 0x06005732 RID: 22322 RVA: 0x001D2F1D File Offset: 0x001D111D
		public Lord Lord
		{
			get
			{
                if (lord == null)
                {
					lord = CompSpawnerPawn.FindLordToJoin(this.parent, this.Props.lordJob, this.Props.shouldJoinParentLord, null);
				}
				return lord;
			}
		}

		// Token: 0x17000F68 RID: 3944
		// (get) Token: 0x06005733 RID: 22323 RVA: 0x001D2F44 File Offset: 0x001D1144
		private float SpawnedPawnsPoints
		{
			get
			{
				this.FilterOutUnspawnedPawns();
				float num = 0f;
				for (int i = 0; i < this.spawnedPawns.Count; i++)
				{
					num += this.spawnedPawns[i].kindDef.combatPower;
				}
				return num;
			}
		}

		// Token: 0x17000F69 RID: 3945
		// (get) Token: 0x06005734 RID: 22324 RVA: 0x001D2F8D File Offset: 0x001D118D
		public bool Active
		{
			get
			{
				return this.pawnsLeftToSpawn != 0 && !this.Dormant;
			}
		}

		// Token: 0x17000F6A RID: 3946
		// (get) Token: 0x06005735 RID: 22325 RVA: 0x001D2FA4 File Offset: 0x001D11A4
		public CompCanBeDormant DormancyComp
		{
			get
			{
				CompCanBeDormant result;
				if ((result = this.dormancyCompCached) == null)
				{
					result = (this.dormancyCompCached = this.parent.TryGetComp<CompCanBeDormant>());
				}
				return result;
			}
		}

		// Token: 0x17000F6B RID: 3947
		// (get) Token: 0x06005736 RID: 22326 RVA: 0x001D2FCF File Offset: 0x001D11CF
		public bool Dormant
		{
			get
			{
				return this.DormancyComp != null && !this.DormancyComp.Awake;
			}
		}

		// Token: 0x06005737 RID: 22327 RVA: 0x001D2FEC File Offset: 0x001D11EC
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			if (this.chosenKind == null)
			{
				this.chosenKind = this.RandomPawnKindDef();
			}
			if (this.Props.maxPawnsToSpawn != IntRange.zero)
			{
				this.pawnsLeftToSpawn = this.Props.maxPawnsToSpawn.RandomInRange;
			}
		}

		// Token: 0x06005738 RID: 22328 RVA: 0x001D3044 File Offset: 0x001D1244
		public static Lord FindLordToJoin(Thing spawner, Type lordJobType, bool shouldTryJoinParentLord, Func<Thing, List<Pawn>> spawnedPawnSelector = null)
		{
			if (spawner.Spawned)
			{
				if (shouldTryJoinParentLord)
				{
					Building building = spawner as Building;
					Lord lord = (building != null) ? building.GetLord() : null;
					if (lord != null)
					{
						return lord;
					}
				}
				if (spawnedPawnSelector == null)
				{
					spawnedPawnSelector = delegate (Thing s)
					{
						CompSpawnerPawn compSpawnerPawn = s.TryGetComp<CompSpawnerPawn>();
						if (compSpawnerPawn != null)
						{
							return compSpawnerPawn.spawnedPawns;
						}
						return null;
					};
				}
				Predicate<Pawn> hasJob = delegate (Pawn x)
				{
					Lord lord2 = x.GetLord();
					return lord2 != null && lord2.LordJob.GetType() == lordJobType;
				};
				Pawn foundPawn = null;
				RegionTraverser.BreadthFirstTraverse(spawner.OccupiedRect().AdjacentCells.RandomElement().GetRegion(spawner.Map), (Region from, Region to) => true, delegate (Region r)
				{
					List<Thing> list = r.ListerThings.ThingsOfDef(spawner.def);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].Faction == spawner.Faction)
						{
							List<Pawn> list2 = spawnedPawnSelector(list[i]);
							if (list2 != null)
							{
								foundPawn = list2.Find(hasJob);
							}
							if (foundPawn != null)
							{
								return true;
							}
						}
					}
					return false;
				}, 40, RegionType.Set_Passable);
				if (foundPawn != null)
				{
					return foundPawn.GetLord();
				}
			}
			return null;
		}

		// Token: 0x06005739 RID: 22329 RVA: 0x001D315C File Offset: 0x001D135C
		public static Lord CreateNewLord(Thing byThing, bool aggressive, float defendRadius, Type lordJobType)
		{
		//	Log.Message("CreateNewLord 0");
			IntVec3 invalid;
		//	Log.Message("CreateNewLord 1");
			if (!CellFinder.TryFindRandomCellNear(byThing.Position, byThing.Map, 5, (IntVec3 c) => c.Standable(byThing.Map) && byThing.Map.reachability.CanReach(c, byThing, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out invalid, -1))
			{
				Log.Error("Found no place for pawns to defend " + byThing, false);
				invalid = IntVec3.Invalid;
			}
		//	Log.Message("CreateNewLord 2");
		//	Log.Message(byThing.Faction + ", " + aggressive + ", " + defendRadius + ", " + lordJobType);
			return LordMaker.MakeNewLord(byThing.Faction, Activator.CreateInstance(lordJobType, new object[]
			{
				new SpawnedPawnParams
				{
					aggressive = aggressive,
					defendRadius = defendRadius,
					defSpot = invalid,
					spawnerThing = byThing
				}
			}) as LordJob, byThing.Map, null);
		}

		// Token: 0x0600573A RID: 22330 RVA: 0x001D3218 File Offset: 0x001D1418
		private void SpawnInitialPawns()
		{
			int num = 0;
			Pawn pawn;
			while (num < this.Props.initialPawnsCount && this.TrySpawnPawn(out pawn))
			{
				num++;
			}
			this.SpawnPawnsUntilPoints(this.Props.initialPawnsPoints);
			if (!this.Props.AlwaysSpawnWith.NullOrEmpty())
			{
				foreach (var item in this.Props.AlwaysSpawnWith)
				{
					this.TrySpawnPawn(out pawn);
				}
			}
			this.CalculateNextPawnSpawnTick();
		}

		// Token: 0x0600573B RID: 22331 RVA: 0x001D325C File Offset: 0x001D145C
		public void SpawnPawnsUntilPoints(float points)
		{
			int num = 0;
			while (this.SpawnedPawnsPoints < points)
			{
				num++;
				if (num > 1000)
				{
					Log.Error("Too many iterations.", false);
					break;
				}
				Pawn pawn;
				if (!this.TrySpawnPawn(out pawn))
				{
					break;
				}
			}
			this.CalculateNextPawnSpawnTick();
		}

		// Token: 0x0600573C RID: 22332 RVA: 0x001D329F File Offset: 0x001D149F
		private void CalculateNextPawnSpawnTick()
		{
			this.CalculateNextPawnSpawnTick(this.Props.pawnSpawnIntervalDays.RandomInRange * 60000f);
		}

		// Token: 0x0600573D RID: 22333 RVA: 0x001D32C0 File Offset: 0x001D14C0
		public void CalculateNextPawnSpawnTick(float delayTicks)
		{
			float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)this.spawnedPawns.Count);
			this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(delayTicks / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		// Token: 0x0600573E RID: 22334 RVA: 0x001D3318 File Offset: 0x001D1518
		private void FilterOutUnspawnedPawns()
		{
			for (int i = this.spawnedPawns.Count - 1; i >= 0; i--)
			{
				if (!this.spawnedPawns[i].Spawned)
				{
					this.spawnedPawns.RemoveAt(i);
				}
			}
		}
		public static float Inverse(float val) => 1f / val;

		// Token: 0x0600573F RID: 22335 RVA: 0x001D335C File Offset: 0x001D155CC
		private PawnKindDef RandomPawnKindDef()
		{
			float curPoints = this.SpawnedPawnsPoints;

			if (spawnablePawnKinds.NullOrEmpty())
			{
				if (!this.Props.spawnablePawnKinds.NullOrEmpty())
				{
					spawnablePawnKinds = this.Props.spawnablePawnKinds;
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
			IEnumerable<PawnGenOption> source = spawnablePawnKinds;
			if (this.Props.maxSpawnedPawnsPoints > -1f)
			{
				source = from x in source
						 where curPoints + x.kind.combatPower <= this.Props.maxSpawnedPawnsPoints
						 select x;
			}
			PawnGenOption result;
			if (source.TryRandomElementByWeight(x => x.selectionWeight, out result))
			{
				return result.kind;
			}
			return null;
		}

		// Token: 0x06005740 RID: 22336 RVA: 0x001D33C0 File Offset: 0x001D15C0
		private bool TrySpawnPawn(out Pawn pawn)
		{
			if (!this.canSpawnPawns)
			{
				pawn = null;
				return false;
			}
			if (!this.Props.chooseSingleTypeToSpawn)
			{
				this.chosenKind = this.RandomPawnKindDef();
			}
			if (this.chosenKind == null)
			{
				pawn = null;
				return false;
			}
			pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(this.chosenKind, this.parent.Faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, null, new float?(this.chosenKind.race.race.lifeStageAges.Last().minAge), null, null, null, null, null, null));
			this.spawnedPawns.Add(pawn);
			GenSpawn.Spawn(pawn, this.parent.OccupiedRect().AdjacentCells.RandomElement()/*CellFinder.RandomClosewalkCellNear(this.parent.Position, this.parent.Map, this.Props.pawnSpawnRadius, null) */, this.parent.Map, WipeMode.Vanish);
			Lord lord = this.Lord;
			if (lord == null)
			{
				lord = CompSpawnerPawn.CreateNewLord(this.parent, this.aggressive, this.Props.defendRadius, this.Props.lordJob);
			}
			lord.AddPawn(pawn);
			if (this.Props.spawnSound != null)
			{
				this.Props.spawnSound.PlayOneShot(this.parent);
			}
			if (this.pawnsLeftToSpawn > 0)
			{
				this.pawnsLeftToSpawn--;
			}
			this.SendMessage();
			return true;
		}

		// Token: 0x06005741 RID: 22337 RVA: 0x001D3568 File Offset: 0x001D1768
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad && this.Active && this.nextPawnSpawnTick == -1)
			{
			//	initialSpawnDelay = Props.initialSpawnDelay;
				//	this.SpawnInitialPawns();
			}
		}
		public override void PostPostMake()
		{
			base.PostPostMake();
			initialSpawnDelay = Props.initialSpawnDelay;
		//	Log.Message("SpawnInitialPawns delay " + initialSpawnDelay);
		}

		private void FeedSpawnedPawns()
		{
			this.FilterOutUnspawnedPawns();
			for (int i = 0; i < this.spawnedPawns.Count; i++)
			{
				Pawn p = this.spawnedPawns[i];

				if (p.def.race.EatsFood)
				{
					if (p.RaceProps.Humanlike)
					{
						if (p.needs.food.Starving)
						{
						//	Log.Message("try feed " + p);
							Thing food = ThingMaker.MakeThing(RimWorld.ThingDefOf.MealNutrientPaste);
							food.stackCount = 3;
							p.inventory.TryAddItemNotForSale(food);
						}
					}
				}
			}
		}
		// Token: 0x06005742 RID: 22338 RVA: 0x001D358C File Offset: 0x001D178C
		public override void CompTick()
		{
			if (this.parent.Map == null)
			{
				return;
			}
			if (initialSpawnDelay > -1)
			{
				initialSpawnDelay--;
			}
			if (this.Active && this.parent.Spawned && this.initialSpawnDelay == 0)
			{
				this.SpawnInitialPawns();
			}
			if (this.parent.Spawned && this.initialSpawnDelay == -1)
			{
				this.FilterOutUnspawnedPawns();
				if (Find.TickManager.TicksGame % 30000 == 0)
				{
					FeedSpawnedPawns();
				}
				if (this.Active && Find.TickManager.TicksGame >= this.nextPawnSpawnTick)
				{
					Pawn pawn;
					if ((this.Props.maxSpawnedPawnsPoints < 0f || this.SpawnedPawnsPoints < this.Props.maxSpawnedPawnsPoints) && this.TrySpawnPawn(out pawn) && pawn.caller != null)
					{
						pawn.caller.DoCall();
					}
					this.CalculateNextPawnSpawnTick();
				}
			}
		}

		// Token: 0x06005743 RID: 22339 RVA: 0x001D3634 File Offset: 0x001D1834
		public void SendMessage()
		{
			if (!this.Props.spawnMessageKey.NullOrEmpty() && MessagesRepeatAvoider.MessageShowAllowed(this.Props.spawnMessageKey, 0.1f))
			{
				Messages.Message(this.Props.spawnMessageKey.Translate(), this.parent, MessageTypeDefOf.NegativeEvent, true);
			}
		}

		// Token: 0x06005744 RID: 22340 RVA: 0x001D3695 File Offset: 0x001D1895
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Spawn pawn",
					icon = TexCommand.ReleaseAnimals,
					action = delegate ()
					{
						Pawn pawn;
						this.TrySpawnPawn(out pawn);
					}
				};
				if (this.spawnedPawns.Any(x => x.def.race.Humanlike))
				{
					yield return new Command_Action
					{
						defaultLabel = "DEBUG: Feed pawns",
						icon = TexCommand.ReleaseAnimals,
						action = delegate ()
						{
							this.FeedSpawnedPawns();
						}
					};
				}
			}
			yield break;
		}

		// Token: 0x06005745 RID: 22341 RVA: 0x001D36A8 File Offset: 0x001D18A8
		public override string CompInspectStringExtra()
		{
			if (!this.Props.showNextSpawnInInspect || this.nextPawnSpawnTick <= 0 || this.chosenKind == null)
			{
				return null;
			}
			if (this.pawnsLeftToSpawn == 0 && !this.Props.noPawnsLeftToSpawnKey.NullOrEmpty())
			{
				return this.Props.noPawnsLeftToSpawnKey.Translate();
			}
			string text;
			if (!this.Dormant)
			{
				text = (this.Props.nextSpawnInspectStringKey ?? "SpawningNextPawnIn").Translate(this.chosenKind.LabelCap, (this.nextPawnSpawnTick - Find.TickManager.TicksGame).ToStringTicksToDays("F1"));
			}
			else
			{
				if (this.Props.nextSpawnInspectStringKeyDormant == null)
				{
					return null;
				}
				text = this.Props.nextSpawnInspectStringKeyDormant.Translate() + ": " + this.chosenKind.LabelCap;
			}
			if (this.pawnsLeftToSpawn > 0 && !this.Props.pawnsLeftToSpawnKey.NullOrEmpty())
			{
				text = text + ("\n" + this.Props.pawnsLeftToSpawnKey.Translate() + ": ") + this.pawnsLeftToSpawn;
			}
			return text;
		}

		// Token: 0x06005746 RID: 22342 RVA: 0x001D37F8 File Offset: 0x001D19F8
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick", 0, false);
			Scribe_Values.Look<int>(ref this.initialSpawnDelay, "initialSpawnDelay", 0, false);
			Scribe_Values.Look<int>(ref this.pawnsLeftToSpawn, "pawnsLeftToSpawn", -1, false);
			Scribe_Collections.Look<Pawn>(ref this.spawnedPawns, "spawnedPawns", LookMode.Reference, Array.Empty<object>());
			Scribe_Values.Look<bool>(ref this.aggressive, "aggressive", false, false);
			Scribe_Values.Look<bool>(ref this.canSpawnPawns, "canSpawnPawns", true, false);
			Scribe_Defs.Look<PawnKindDef>(ref this.chosenKind, "chosenKind");
			Scribe_References.Look<Lord>(ref this.lord, "lord");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.spawnedPawns.RemoveAll((Pawn x) => x == null);
				if (this.pawnsLeftToSpawn == -1 && this.Props.maxPawnsToSpawn != IntRange.zero)
				{
					this.pawnsLeftToSpawn = this.Props.maxPawnsToSpawn.RandomInRange;
				}
			}
		}

		private Lord lord;
		// Token: 0x0400307D RID: 12413
		public int nextPawnSpawnTick = -1;
		public int initialSpawnDelay = -1;

		// Token: 0x0400307E RID: 12414
		public int pawnsLeftToSpawn = -1;

		// Token: 0x0400307F RID: 12415
		public List<Pawn> spawnedPawns = new List<Pawn>();
		public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();

		// Token: 0x04003080 RID: 12416
		public bool aggressive = true;

		// Token: 0x04003081 RID: 12417
		public bool canSpawnPawns = true;

		// Token: 0x04003082 RID: 12418
		private PawnKindDef chosenKind;

		// Token: 0x04003083 RID: 12419
		private CompCanBeDormant dormancyCompCached;
	}
}
