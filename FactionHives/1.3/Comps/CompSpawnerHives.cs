using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	public class CompProperties_SpawnerHives : RimWorld.CompProperties_SpawnerHives
	{
		public CompProperties_SpawnerHives()
		{
			this.compClass = typeof(CompSpawnerHives);
		}

		public ThingDef hiveDef;
		public ThingDef tunnelDef;
		public bool requireRoofed = true;
		public SimpleCurve radiusPerDayCurve;
	}

	public class CompSpawnerHives : RimWorld.CompSpawnerHives
	{
		public new CompProperties_SpawnerHives Props
		{
			get
			{
				return (CompProperties_SpawnerHives)this.props;
			}
		}

		public new bool CanSpawnChildHive
		{
			get
			{
				return this.canSpawnHives && HiveUtility.TotalSpawnedHivesCount(this.parent.Map, this.Props.hiveDef) < 30;
			}
		}

		public float AgeDays
		{
			get
			{
				return (float)this.plantHarmAge / 60000f;
			}
		}

		public float CurrentRadius
		{
			get
			{
				return  Mathf.Max(this.Props.radiusPerDayCurve?.Evaluate(this.AgeDays) ?? this.Props.HiveSpawnRadius, Props.HiveSpawnPreferredMinDist);
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.CalculateNextHiveSpawnTick();
			}
			this.initiatableComp = this.parent.GetComp<CompInitiatable>();
		}

		public override void CompTick()
		{
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

		public new void CalculateNextHiveSpawnTick()
		{
			Room room = this.parent.GetRoom(RegionType.Set_Passable);
			int num = 0;
			int num2 = GenRadial.NumCellsInRadius(CurrentRadius);
			for (int i = 0; i < num2; i++)
			{
				IntVec3 intVec = this.parent.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(this.parent.Map) && intVec.GetRoom(this.parent.Map) == room)
				{
					if (intVec.GetThingList(this.parent.Map).Any((Thing t) => t is Hive && t.def == this.parent.def))
					{
						num++;
					}
				}
			}
			float num3 = this.Props.ReproduceRateFactorFromNearbyHiveCountCurve.Evaluate((float)num);
			this.nextHiveSpawnTick = Find.TickManager.TicksGame + (int)(this.Props.HiveSpawnIntervalDays.RandomInRange * 60000f / (num3 * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

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

		private static new bool CanSpawnHiveAt(IntVec3 c, Map map, IntVec3 parentPos, ThingDef parentDef, float minDist, bool ignoreRoofedRequirement)
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

		public override void PostExposeData()
		{
			Scribe_Values.Look<int>(ref this.plantHarmAge, "plantHarmAge", 0, false);
			Scribe_Values.Look<int>(ref this.ticksToPlantHarm, "ticksToPlantHarm", 0, false);
		}

		private int plantHarmAge;
		private int ticksToPlantHarm;
		protected CompInitiatable initiatableComp;
	}
}
