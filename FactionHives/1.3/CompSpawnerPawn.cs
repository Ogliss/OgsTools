using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace ExtraHives
{
	// Token: 0x02000D60 RID: 3424
	public class CompProperties_SpawnerPawn : CompProperties
	{
		// Token: 0x06005349 RID: 21321 RVA: 0x001BD780 File Offset: 0x001BB980
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
		public bool assaultOnSpawn;
		public bool shouldJoinParentLord;
		public Type lordJob;
		public float defendRadius = 21f;
		public int initialSpawnDelay = 0;
		public int initialPawnsCount = 10;
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
				factionGroupKindDef = PawnGroupKindDefOf.Hive_ExtraHives;
			}
		}
	}
	// Token: 0x02000D61 RID: 3425
	public class CompSpawnerPawn : ThingComp
	{
		// Token: 0x17000ECE RID: 3790
		// (get) Token: 0x0600534A RID: 21322 RVA: 0x001BD7E0 File Offset: 0x001BB9E0
		private CompProperties_SpawnerPawn Props
		{
			get
			{
				return (CompProperties_SpawnerPawn)this.props;
			}
		}

		public HiveDefExtension HiveExtension
		{
			get
			{
				HiveDefExtension hiveExtension = null;
				hiveExtension = this.parent.def.GetModExtension<HiveDefExtension>();
				return hiveExtension;
			}
		}
		// Token: 0x17000ECF RID: 3791
		// (get) Token: 0x0600534B RID: 21323 RVA: 0x001BD7ED File Offset: 0x001BB9ED
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

		// Token: 0x17000ED0 RID: 3792
		// (get) Token: 0x0600534C RID: 21324 RVA: 0x001BD814 File Offset: 0x001BBA14
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
						//	Log.Message("try feed "+p);
							Thing food = ThingMaker.MakeThing(RimWorld.ThingDefOf.MealNutrientPaste);
							food.stackCount = 3;
							p.inventory.TryAddItemNotForSale(food);
                        }
                    }
                }
			}
		}
		// Token: 0x17000ED1 RID: 3793
		// (get) Token: 0x0600534D RID: 21325 RVA: 0x001BD85D File Offset: 0x001BBA5D
		public bool Active
		{
			get
			{
				return this.pawnsLeftToSpawn != 0 && !this.Dormant;
			}
		}

		// Token: 0x17000ED2 RID: 3794
		// (get) Token: 0x0600534E RID: 21326 RVA: 0x001BD874 File Offset: 0x001BBA74
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

		// Token: 0x17000ED3 RID: 3795
		// (get) Token: 0x0600534F RID: 21327 RVA: 0x001BD89F File Offset: 0x001BBA9F
		public bool Dormant
		{
			get
			{
				return this.DormancyComp != null && !this.DormancyComp.Awake;
			}
		}

		// Token: 0x06005350 RID: 21328 RVA: 0x001BD8BC File Offset: 0x001BBABC
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

		// Token: 0x06005351 RID: 21329 RVA: 0x001BD914 File Offset: 0x001BBB14
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
			//	Log.Message("for map " + spawner.Map);
			//	Log.Message("try get region at "+ spawner.OccupiedRect().AdjacentCells.Where(x=> x.Walkable(spawner.Map)).RandomElement());
				RegionTraverser.BreadthFirstTraverse(spawner.OccupiedRect().AdjacentCells.Where(x => x.Walkable(spawner.Map)).RandomElement().GetRegion(spawner.Map), (Region from, Region to) => true, delegate (Region r)
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

		// Token: 0x06005352 RID: 21330 RVA: 0x001BDA2C File Offset: 0x001BBC2C
		public Lord CreateNewLord(Thing byThing, bool aggressive, float defendRadius, Type lordJobType)
		{
			IntVec3 invalid;
			if (!CellFinder.TryFindRandomCellNear(byThing.Position, byThing.Map, 5, (IntVec3 c) => c.Standable(byThing.Map) && byThing.Map.reachability.CanReach(c, byThing, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out invalid, -1))
			{
				Log.Error("Found no place for mechanoids to defend " + byThing, false);
				invalid = IntVec3.Invalid;
			}
			Lord lord =  LordMaker.MakeNewLord(byThing.Faction, Activator.CreateInstance(lordJobType, new object[]
			{
				new SpawnedPawnParams
				{
					aggressive = aggressive,
					defendRadius = defendRadius,
					defSpot = invalid,
					spawnerThing = byThing
				}
			}) as LordJob, byThing.Map, null);
			return lord;
		}

		// Token: 0x06005353 RID: 21331 RVA: 0x001BDAE8 File Offset: 0x001BBCE8
		private void SpawnInitialPawns()
		{
		//	Log.Message("SpawnInitialPawns");
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
					this.TrySpawnPawn(out pawn, item);
				}
			}
			this.CalculateNextPawnSpawnTick();
		}

		// Token: 0x06005354 RID: 21332 RVA: 0x001BDB2C File Offset: 0x001BBD2C
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

		// Token: 0x06005355 RID: 21333 RVA: 0x001BDB6F File Offset: 0x001BBD6F
		private void CalculateNextPawnSpawnTick()
		{
			this.CalculateNextPawnSpawnTick(this.Props.pawnSpawnIntervalDays.RandomInRange * 60000f);
		}

		// Token: 0x06005356 RID: 21334 RVA: 0x001BDB90 File Offset: 0x001BBD90
		public void CalculateNextPawnSpawnTick(float delayTicks)
		{
			float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)this.spawnedPawns.Count);
			this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(delayTicks / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		// Token: 0x06005357 RID: 21335 RVA: 0x001BDBE8 File Offset: 0x001BBDE8
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

		// Token: 0x06005358 RID: 21336 RVA: 0x001BDC2C File Offset: 0x001BBE2C
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
						if (parent.Faction.def.pawnGroupMakers.Any(x=> x.kindDef == this.Props.factionGroupKindDef))
						{
							spawnablePawnKinds = parent.Faction.def.pawnGroupMakers.Where(x => x.kindDef == this.Props.factionGroupKindDef).RandomElementByWeight(x=> x.commonality).options;
						}
						else
						{
							spawnablePawnKinds = parent.Faction.def.pawnGroupMakers.Where(x => x.kindDef == RimWorld.PawnGroupKindDefOf.Combat || x.kindDef == RimWorld.PawnGroupKindDefOf.Settlement).RandomElementByWeight(x => x.commonality).options;
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
			if (source.TryRandomElementByWeight( x=> x.selectionWeight ,out result))
			{
				return result.kind;
			}
			return null;
		}

		// Token: 0x06005359 RID: 21337 RVA: 0x001BDC90 File Offset: 0x001BBE90
		private bool TrySpawnPawn(out Pawn pawn, PawnKindDef kindDef = null)
		{
			if (!this.canSpawnPawns)
			{
				pawn = null;
				return false;
			}
			if (!this.Props.chooseSingleTypeToSpawn)
			{
				this.chosenKind = kindDef !=null ? kindDef : this.RandomPawnKindDef();
			}
			if (this.chosenKind == null)
			{
			//	Log.Message("TrySpawnPawn chosenKind == null return false");
				pawn = null;
				return false;
			}
			Faction faction = null;


			if (parent.Faction != null)
			{
				faction = parent.Faction;
			}
			else
			{
				Log.Warning("Warning faction not found");
				if (HiveExtension?.Faction != null)
				{
					faction = Find.FactionManager.FirstFactionOfDef(HiveExtension.Faction);
				}
			}
			if (parent.Faction == null)
			{
				parent.SetFaction(faction);
			}
			pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(this.chosenKind, faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, new float?(this.chosenKind.race.race.lifeStageAges.Last().minAge), null, null, null, null, null, null));

			this.spawnedPawns.Add(pawn);
			GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(this.parent.OccupiedRect().AdjacentCells.RandomElement(), this.parent.Map, this.Props.pawnSpawnRadius, null), this.parent.Map, WipeMode.Vanish);
			Lord lord = this.Lord;
			if (lord == null)
			{
			//	Log.Message("make new lord of "+ this.parent.Faction +" for "+this.parent+" agro "+aggressive+" radius "+Props.defendRadius+" Job: "+this.Props.lordJob);
				lord = CreateNewLord(this.parent, this.aggressive, this.Props.defendRadius, this.Props.lordJob);
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

		// Token: 0x0600535A RID: 21338 RVA: 0x001BDE38 File Offset: 0x001BC038
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad && this.Active && this.nextPawnSpawnTick == -1)
			{
				initialSpawnDelay = Props.initialSpawnDelay;
				//	this.SpawnInitialPawns();
			}
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
		//	Log.Message("SpawnInitialPawns delay " + initialSpawnDelay);
		}

		// Token: 0x0600535B RID: 21339 RVA: 0x001BDE5C File Offset: 0x001BC05C
		public override void CompTick()
		{
			if (this.parent.Map == null)
			{
				if (this.initialSpawnDelay == -1)
				{
					initialSpawnDelay = Props.initialSpawnDelay;
				}
				return;
			}
			if (initialSpawnDelay>-1)
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

		// Token: 0x0600535C RID: 21340 RVA: 0x001BDF04 File Offset: 0x001BC104
		public void SendMessage()
		{
			if (!this.Props.spawnMessageKey.NullOrEmpty() && MessagesRepeatAvoider.MessageShowAllowed(this.Props.spawnMessageKey, 0.1f))
			{
				Messages.Message(this.Props.spawnMessageKey.Translate(), this.parent, MessageTypeDefOf.NegativeEvent, true);
			}
		}

		// Token: 0x0600535D RID: 21341 RVA: 0x001BDF65 File Offset: 0x001BC165
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
                if (this.spawnedPawns.Any(x=> x.def.race.Humanlike))
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

		// Token: 0x0600535E RID: 21342 RVA: 0x001BDF78 File Offset: 0x001BC178
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

		// Token: 0x0600535F RID: 21343 RVA: 0x001BE0C8 File Offset: 0x001BC2C8
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
		// Token: 0x04002E0F RID: 11791
		public int nextPawnSpawnTick = -1;
		public int initialSpawnDelay = -1;

		// Token: 0x04002E10 RID: 11792
		public int pawnsLeftToSpawn = -1;

		// Token: 0x04002E11 RID: 11793
		public List<Pawn> spawnedPawns = new List<Pawn>();
		public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();

		// Token: 0x04002E12 RID: 11794
		public bool aggressive = true;

		// Token: 0x04002E13 RID: 11795
		public bool canSpawnPawns = true;

		// Token: 0x04002E14 RID: 11796
		private PawnKindDef chosenKind;

		// Token: 0x04002E15 RID: 11797
		private CompCanBeDormant dormancyCompCached;
	}
}
