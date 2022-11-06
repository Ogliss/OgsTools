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
	public class CompProperties_SpawnerPawn : CompProperties
	{
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
		public int initialPawnsPoints;
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

	public class CompSpawnerPawn : ThingComp
	{
		public CompProperties_SpawnerPawn Props => (CompProperties_SpawnerPawn)this.props;

		public bool Active => this.pawnsLeftToSpawn != 0 && !this.Dormant;

		public bool Dormant => this.DormancyComp != null && !this.DormancyComp.Awake;

		public HiveDefExtension HiveExtension => cachedHiveExtension ??= this.parent.def.GetModExtension<HiveDefExtension>();

		public Lord Lord => cachedlord ??= FindLordToJoin(this.parent, this.Props.lordJob, this.Props.shouldJoinParentLord, null);

		public float SpawnedPawnsPoints
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

		public int SpawnedPawnsCount
		{
			get
			{
				this.FilterOutUnspawnedPawns();
				return this.spawnedPawns.Count;
			}
		}

		public void FeedSpawnedPawns()
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

		public Lord FindLordToJoin(Thing spawner, Type lordJobType, bool shouldTryJoinParentLord, Func<Thing, List<Pawn>> spawnedPawnSelector = null)
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

		public Lord CreateNewLord(Thing byThing, bool aggressive, float defendRadius, Type lordJobType)
		{
			IntVec3 invalid;
			if (!CellFinder.TryFindRandomCellNear(byThing.Position, byThing.Map, 5, (IntVec3 c) => c.Standable(byThing.Map) && byThing.Map.reachability.CanReach(c, byThing, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false)), out invalid, -1))
			{
				Log.Error("Found no place for mechanoids to defend " + byThing);
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

		public void SpawnInitialPawns()
		{
			string s = $"{this.parent.LabelCap}({this.parent.Position.x},{this.parent.Position.z}) SpawnInitialPawns\n    initialPawnsCount: {this.Props.initialPawnsCount}";

			Pawn pawn;
			/*			
			int num = 0;
			while (num < this.Props.initialPawnsCount)
			{
                if (!this.TrySpawnPawn(out pawn))
                {
					break;
				}
				if (Prefs.DevMode) s += $"\n    initialPawn: {pawn.NameShortColored}";
				num++;
			}
			*/
			int points = initialPawnsPoints > -1 ? initialPawnsPoints : this.Props.initialPawnsPoints;
			if (Prefs.DevMode) s += $"\n    SpawnInitialPawns points: {points}";
			this.SpawnPawnsUntilPoints(points, true);
			if (!this.Props.AlwaysSpawnWith.NullOrEmpty())
			{
				foreach (var item in this.Props.AlwaysSpawnWith)
				{
                    if (this.TrySpawnPawn(out pawn, item))
					{
						if (Prefs.DevMode) s += $"\n    AlwaysSpawnWith: {pawn.NameShortColored}";
					}
				}
			}
		//	if (Prefs.DevMode) Log.Message(s);
			this.CalculateNextPawnSpawnTick();
		}

		public void SpawnPawnsUntilPoints(float points, bool spawningInital = false, bool spawningWave = false, int extraPoints = 0)
		{
			int num = 0;
			int wavePoints = 0;
			string s = $"SpawnPawnsUntilXPoints {(points + extraPoints) - (spawningWave ? wavePoints : this.SpawnedPawnsPoints)}";
			while ((spawningWave ? wavePoints : this.SpawnedPawnsPoints) < points + extraPoints)
			{
				num++;
				if (num > 1000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				Pawn pawn;
				float max = (points + extraPoints) - (spawningWave ? wavePoints : this.SpawnedPawnsPoints);
				if (!this.TrySpawnPawn(out pawn, maxPower: (int)max))
				{
					break;
				}
				wavePoints += (int)pawn.kindDef.combatPower;

				if (spawningInital)
				{
					if (Prefs.DevMode) s += ($"\n    spawningInital: {pawn.NameShortColored}:({pawn.kindDef.combatPower})");
					if (SpawnedPawnsCount >= this.Props.initialPawnsCount)
					{
						if (Prefs.DevMode) s += ($"\n    spawningInital Finished: {SpawnedPawnsCount}");
						break;
					}
				}
			}
			if (Prefs.DevMode) Log.Message(s);
			this.CalculateNextPawnSpawnTick();
		}

		public bool TrySpawnPawn(out Pawn pawn, PawnKindDef kindDef = null, int? maxPower = null)
		{
			if (!this.canSpawnPawns)
			{
				pawn = null;
				return false;
			}
			if (!this.Props.chooseSingleTypeToSpawn)
			{
				this.chosenKind = kindDef != null ? kindDef : this.RandomPawnKindDef(maxPower);
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
			pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(this.chosenKind, faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, 1f, false, true, true, true, false, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, new float?(this.chosenKind.race.race.lifeStageAges.Last().minAge), null, null, null, null, null, null));

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

		public PawnKindDef RandomPawnKindDef(int? maxPower = null)
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
            if (maxPower.HasValue)
			{
				source = from x in source
						 where x.kind.combatPower <= maxPower.Value
						 select x;
			}
			else
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
						Log.Message("PawnSpawner CompTick Spawned "+pawn.NameShortColored);
						pawn.caller.DoCall();
					}
					this.CalculateNextPawnSpawnTick();
				}
			}
		}

		public void CalculateNextPawnSpawnTick()
		{
			this.CalculateNextPawnSpawnTick(this.Props.pawnSpawnIntervalDays.RandomInRange * 60000f);
		}

		public void CalculateNextPawnSpawnTick(float delayTicks)
		{
			float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)this.SpawnedPawnsCount);
			num = (delayTicks / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
			//	Log.Message($"CalculateNextPawnSpawnTick with Delay: {delayTicks} -- {num} next Spawn tick = {Find.TickManager.TicksGame + (int)num}");
			this.nextPawnSpawnTick = Find.TickManager.TicksGame + (int)num;
		}

		public void FilterOutUnspawnedPawns()
		{
			for (int i = this.spawnedPawns.Count - 1; i >= 0; i--)
			{
				if (!this.spawnedPawns[i].Spawned)
				{
					this.spawnedPawns.RemoveAt(i);
				}
			}
		}

		public void SendMessage()
		{
			if (!this.Props.spawnMessageKey.NullOrEmpty() && MessagesRepeatAvoider.MessageShowAllowed(this.Props.spawnMessageKey, 0.1f))
			{
				Messages.Message(this.Props.spawnMessageKey.Translate(), this.parent, MessageTypeDefOf.NegativeEvent, true);
			}
		}

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
                if (this.spawnedPawns.Any(x=> x.def.race.EatsFood))
                {
					yield return new Command_Action
					{
						defaultLabel = "DEBUG: Feed pawns",
						icon = TexCommand.DesirePower,
						action = delegate ()
						{
							this.FeedSpawnedPawns();
						}
					};
				}
			}
			yield break;
		}

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

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.nextPawnSpawnTick, "nextPawnSpawnTick", 0, false);
			Scribe_Values.Look<int>(ref this.initialSpawnDelay, "initialSpawnDelay", 0, false);
			Scribe_Values.Look<int>(ref this.initialPawnsPoints, "initialPawnsPoints", -1, false);
			Scribe_Values.Look<int>(ref this.pawnsLeftToSpawn, "pawnsLeftToSpawn", -1, false);
			Scribe_Collections.Look<Pawn>(ref this.spawnedPawns, "spawnedPawns", LookMode.Reference, Array.Empty<object>());
			Scribe_Values.Look<bool>(ref this.aggressive, "aggressive", false, false);
			Scribe_Values.Look<bool>(ref this.canSpawnPawns, "canSpawnPawns", true, false);
			Scribe_Defs.Look<PawnKindDef>(ref this.chosenKind, "chosenKind");
			Scribe_References.Look<Lord>(ref this.cachedlord, "lord");
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.spawnedPawns.RemoveAll((Pawn x) => x == null);
				if (this.pawnsLeftToSpawn == -1 && this.Props.maxPawnsToSpawn != IntRange.zero)
				{
					this.pawnsLeftToSpawn = this.Props.maxPawnsToSpawn.RandomInRange;
				}
			}
		}

		public int nextPawnSpawnTick = -1;
		public int initialSpawnDelay = -1;
		public int pawnsLeftToSpawn = -1;
		public int initialPawnsPoints = -1;
		public List<Pawn> spawnedPawns = new List<Pawn>();
		public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();
		public bool aggressive = true;
		public bool canSpawnPawns = true;
		public PawnKindDef chosenKind;
		private HiveDefExtension cachedHiveExtension;
		private Lord cachedlord;
		private CompCanBeDormant dormancyCompCached;
	}
}
