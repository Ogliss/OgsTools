using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000D5C RID: 3420
	public class CompProperties_SpawnerHives : CompProperties
	{
		// Token: 0x0600533A RID: 21306 RVA: 0x001BD4A8 File Offset: 0x001BB6A8
		public CompProperties_SpawnerHives()
		{
			this.compClass = typeof(CompSpawnerHives);
		}

		public ThingDef hiveDef;
		public ThingDef tunnelDef;

		public bool requireRoofed = true;
		// Token: 0x04002DF0 RID: 11760
		public float HiveSpawnPreferredMinDist = 3.5f;

		// Token: 0x04002DF1 RID: 11761
		public float HiveSpawnRadius = 10f;

		// Token: 0x04002DF2 RID: 11762
		public FloatRange HiveSpawnIntervalDays = new FloatRange(2f, 3f);

		// Token: 0x04002DF3 RID: 11763
		public SimpleCurve ReproduceRateFactorFromNearbyHiveCountCurve = new SimpleCurve
		{
			{
				new CurvePoint(0f, 1f),
				true
			},
			{
				new CurvePoint(7f, 0.35f),
				true
			}
		};
		// Token: 0x04002F97 RID: 12183
		public SimpleCurve radiusPerDayCurve;
	}
	// Token: 0x02000D5B RID: 3419
	public class CompSpawnerHives : ThingComp
	{
		// Token: 0x17000EC9 RID: 3785
		// (get) Token: 0x0600532D RID: 21293 RVA: 0x001BCEC3 File Offset: 0x001BB0C3
		public CompProperties_SpawnerHives Props
		{
			get
			{
				return (CompProperties_SpawnerHives)this.props;
			}
		}

		// Token: 0x17000ECA RID: 3786
		// (get) Token: 0x0600532E RID: 21294 RVA: 0x001BCED0 File Offset: 0x001BB0D0
		private bool CanSpawnChildHive
		{
			get
			{
				return this.canSpawnHives && HiveUtility.TotalSpawnedHivesCount(this.parent.Map, this.Props.hiveDef) < 30;
			}
		}

		// Token: 0x17000F07 RID: 3847
		// (get) Token: 0x060055C2 RID: 21954 RVA: 0x001CB033 File Offset: 0x001C9233
		public float AgeDays
		{
			get
			{
				return (float)this.plantHarmAge / 60000f;
			}
		}

		// Token: 0x17000F08 RID: 3848
		// (get) Token: 0x060055C3 RID: 21955 RVA: 0x001CB042 File Offset: 0x001C9242
		public float CurrentRadius
		{
			get
			{
				return  Mathf.Max(this.Props.radiusPerDayCurve?.Evaluate(this.AgeDays) ?? this.Props.HiveSpawnRadius, Props.HiveSpawnPreferredMinDist);
			}
		}
		// Token: 0x0600532F RID: 21295 RVA: 0x001BCEF0 File Offset: 0x001BB0F0
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.CalculateNextHiveSpawnTick();
			}
			this.initiatableComp = this.parent.GetComp<CompInitiatable>();
		}

		// Token: 0x06005330 RID: 21296 RVA: 0x001BCEFC File Offset: 0x001BB0FC
		public override void CompTick()
		{
			base.CompTick();
			if (this.parent.Map == null)
			{
				return;
			}
			this.plantHarmAge++;
			CompCanBeDormant comp = this.parent.GetComp<CompCanBeDormant>();
			if ((comp == null || comp.Awake) && !this.wasActivated)
			{
				this.CalculateNextHiveSpawnTick();
				this.wasActivated = true;
			}
			if ((comp == null || comp.Awake) && Find.TickManager.TicksGame >= this.nextHiveSpawnTick)
			{
				Hive t;
				if (this.TrySpawnChildHive(!Props.requireRoofed, out t))
				{
					Messages.Message("MessageHiveReproduced".Translate(), t, MessageTypeDefOf.NegativeEvent, true);
					return;
				}
				this.CalculateNextHiveSpawnTick();
			}
		}

		public override string CompInspectStringExtra()
		{
			string str = !Props.radiusPerDayCurve.EnumerableNullOrEmpty() ? "FoliageKillRadius".Translate() + ": " + this.CurrentRadius.ToString("0.0") + "\n" + "RadiusExpandRate".Translate() + ": " + Math.Round((double)(this.Props.radiusPerDayCurve.Evaluate(this.AgeDays + 1f) - this.Props.radiusPerDayCurve.Evaluate(this.AgeDays))) + "/" + "day".Translate() + "\n": TaggedString.Empty;

			if (!this.canSpawnHives)
			{
				return str+"DormantHiveNotReproducing".Translate();
			}
			if (this.CanSpawnChildHive)
			{
				return str  + "HiveReproducesIn".Translate() + ": " + (this.nextHiveSpawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod(true, false, true, true);
			}
			return str;
		}

		// Token: 0x06005332 RID: 21298 RVA: 0x001BCFF8 File Offset: 0x001BB1F8
		public void CalculateNextHiveSpawnTick()
		{
			Room room = this.parent.GetRoom(RegionType.Set_Passable);
			int num = 0;
			int num2 = GenRadial.NumCellsInRadius(CurrentRadius);
			for (int i = 0; i < num2; i++)
			{
				IntVec3 intVec = this.parent.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(this.parent.Map) && intVec.GetRoom(this.parent.Map) == room)
				{
					if (intVec.GetThingList(this.parent.Map).Any((Thing t) => t is RimWorld.Hive))
					{
						num++;
					}
				}
			}
			float num3 = this.Props.ReproduceRateFactorFromNearbyHiveCountCurve.Evaluate((float)num);
			this.nextHiveSpawnTick = Find.TickManager.TicksGame + (int)(this.Props.HiveSpawnIntervalDays.RandomInRange * 60000f / (num3 * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		// Token: 0x06005333 RID: 21299 RVA: 0x001BD108 File Offset: 0x001BB308
		public bool TrySpawnChildHive(bool ignoreRoofedRequirement, out Hive newHive)
		{
			if (!this.CanSpawnChildHive)
			{
				newHive = null;
				return false;
			}
			HiveDefExtension ext = null;
			if (this.parent.def.HasModExtension<HiveDefExtension>())
			{
				ext = this.parent.def.GetModExtension<HiveDefExtension>();
			}
			ThingDef thingDef = Props.hiveDef ?? this.parent.def;
			IntVec3 loc = CompSpawnerHives.FindChildHiveLocation(this.parent.OccupiedRect().AdjacentCells.RandomElement(), this.parent.Map, thingDef, this.Props, ignoreRoofedRequirement, false, CurrentRadius);
			if (!loc.IsValid)
			{
				newHive = null;
				Log.Warning("this !loc.IsValid");
				return false;
			}
			newHive = (Hive)ThingMaker.MakeThing(thingDef, null);
			if (newHive.Faction != this.parent.Faction)
			{
				newHive.SetFaction(this.parent.Faction, null);
			}
			Hive hive = this.parent as Hive;
			if (hive != null)
			{
				if (hive.CompDormant.Awake)
				{
					newHive.CompDormant.WakeUp();
				}
				newHive.questTags = hive.questTags;
			}
			if (newHive.Ext?.TunnelDef!=null)
			{
				TunnelHiveSpawner tunnel = (TunnelHiveSpawner)ThingMaker.MakeThing(newHive.Ext.TunnelDef, null);
				tunnel.hive = newHive;
				GenSpawn.Spawn(tunnel, loc, this.parent.Map, WipeMode.FullRefund);
				this.CalculateNextHiveSpawnTick();
			}
			else
			{
				GenSpawn.Spawn(newHive, loc, this.parent.Map, WipeMode.FullRefund);
				this.CalculateNextHiveSpawnTick();
			}
			return true;
		}

		// Token: 0x06005334 RID: 21300 RVA: 0x001BD1F0 File Offset: 0x001BB3F0
		public static IntVec3 FindChildHiveLocation(IntVec3 pos, Map map, ThingDef parentDef, CompProperties_SpawnerHives props, bool ignoreRoofedRequirement, bool allowUnreachable, float radius = 0f)
		{
			IntVec3 intVec = IntVec3.Invalid;
			float Radius = radius > 0 ? radius : props.HiveSpawnRadius;
			for (int i = 0; i < 3; i++)
			{
				float minDist = props.HiveSpawnPreferredMinDist;
				bool flag;
				if (i < 2)
				{
					if (i == 1)
					{
						minDist = 0f;
					}
					flag = CellFinder.TryFindRandomReachableCellNear(pos, map, Radius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), (IntVec3 c) => CompSpawnerHives.CanSpawnHiveAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement), null, out intVec, 999999);

				}
				else
				{
					flag = (allowUnreachable && CellFinder.TryFindRandomCellNear(pos, map, (int)Radius, (IntVec3 c) => CompSpawnerHives.CanSpawnHiveAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement), out intVec, -1));
				}
				if (flag)
				{
					intVec = CellFinder.FindNoWipeSpawnLocNear(intVec, map, parentDef, Rot4.North, 2, (IntVec3 c) => CompSpawnerHives.CanSpawnHiveAt(c, map, pos, parentDef, minDist, ignoreRoofedRequirement));
					break;
				}
			}
			return intVec;
		}

		// Token: 0x06005335 RID: 21301 RVA: 0x001BD318 File Offset: 0x001BB518
		private static bool CanSpawnHiveAt(IntVec3 c, Map map, IntVec3 parentPos, ThingDef parentDef, float minDist, bool ignoreRoofedRequirement)
		{
		//	Log.Message("Checking "+ c + " for " + parentDef + " minDist "+minDist+ " need roofed: " + !ignoreRoofedRequirement);
			if ((!ignoreRoofedRequirement && !c.Roofed(map)) || (!c.Walkable(map) || (minDist != 0f && (float)c.DistanceToSquared(parentPos) < minDist * minDist)) || c.GetFirstThing(map, RimWorld.ThingDefOf.InsectJelly) != null || c.GetFirstThing(map, RimWorld.ThingDefOf.GlowPod) != null)
			{
			//	Log.Message(c+" failed due to lacking roof!!");
				return false;
			}
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
				if (c2.InBounds(map))
				{
					List<Thing> thingList = c2.GetThingList(map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j] is Hive || thingList[j] is TunnelHiveSpawner)
						{
							return false;
						}
					}
				}
			}
			List<Thing> thingList2 = c.GetThingList(map);
			for (int k = 0; k < thingList2.Count; k++)
			{
				Thing thing = thingList2[k];
				if (thing.def.category == ThingCategory.Building && thing.def.passability == Traversability.Impassable && GenSpawn.SpawningWipes(parentDef, thing.def))
				{
					return true;
				}
			}
			return true;
		}

		// Token: 0x06005336 RID: 21302 RVA: 0x001BD432 File Offset: 0x001BB632
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "Dev: Reproduce",
					icon = TexCommand.GatherSpotActive,
					action = delegate ()
					{
						Hive hive;
						if (this.TrySpawnChildHive(true, out hive))
						{

						}
						else
						{
							Log.Warning("Failed Spawning hive");
						}
					}
				};
			}
			yield break;
		}

		// Token: 0x06005337 RID: 21303 RVA: 0x001BD442 File Offset: 0x001BB642
		public override void PostExposeData()
		{
			Scribe_Values.Look<int>(ref this.nextHiveSpawnTick, "nextHiveSpawnTick", 0, false);
			Scribe_Values.Look<bool>(ref this.canSpawnHives, "canSpawnHives", true, false);
			Scribe_Values.Look<bool>(ref this.wasActivated, "wasActivated", true, false);
			Scribe_Values.Look<int>(ref this.plantHarmAge, "plantHarmAge", 0, false);
			Scribe_Values.Look<int>(ref this.ticksToPlantHarm, "ticksToPlantHarm", 0, false);
		}

		// Token: 0x04002DEC RID: 11756
		private int nextHiveSpawnTick = -1;

		// Token: 0x04002DED RID: 11757
		public bool canSpawnHives = true;

		// Token: 0x04002DEE RID: 11758
		private bool wasActivated;

		// Token: 0x04002DEF RID: 11759
		public const int MaxHivesPerMap = 30;

		// Token: 0x04002F98 RID: 12184
		private int plantHarmAge;

		// Token: 0x04002F99 RID: 12185
		private int ticksToPlantHarm;

		// Token: 0x04002F9A RID: 12186
		protected CompInitiatable initiatableComp;
	}
}
