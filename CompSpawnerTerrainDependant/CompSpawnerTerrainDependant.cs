using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace CompSpawnerTerrainDependant
{
    // Token: 0x02000DE1 RID: 3553
    public class CompSpawnerTerrainDependant : ThingComp
	{
		// Token: 0x17000F73 RID: 3955
		// (get) Token: 0x0600574F RID: 22351 RVA: 0x001D4378 File Offset: 0x001D2578
		public CompProperties_SpawnerTerrainDependant PropsSpawner
		{
			get
			{
				return (CompProperties_SpawnerTerrainDependant)this.props;
			}
		}

		// Token: 0x17000F74 RID: 3956
		// (get) Token: 0x06005750 RID: 22352 RVA: 0x001D4388 File Offset: 0x001D2588
		private bool PowerOn
		{
			get
			{
				CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
				return comp != null && comp.PowerOn;
			}
		}

		// Token: 0x06005751 RID: 22353 RVA: 0x001D43AC File Offset: 0x001D25AC
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.ResetCountdown();
			}
		}

		// Token: 0x06005752 RID: 22354 RVA: 0x001D43B7 File Offset: 0x001D25B7
		public override void CompTick()
		{
			this.TickInterval(1);
		}

		// Token: 0x06005753 RID: 22355 RVA: 0x001D43C0 File Offset: 0x001D25C0
		public override void CompTickRare()
		{
			this.TickInterval(250);
		}

		// Token: 0x06005754 RID: 22356 RVA: 0x001D43D0 File Offset: 0x001D25D0
		private void TickInterval(int interval)
		{
			if (!this.parent.Spawned)
			{
				return;
			}
			CompCanBeDormant comp = this.parent.GetComp<CompCanBeDormant>();
			if (comp != null)
			{
				if (!comp.Awake)
				{
					return;
				}
			}
			else if (this.parent.Position.Fogged(this.parent.Map))
			{
				return;
			}
			if (this.PropsSpawner.requiresPower && !this.PowerOn)
			{
				return;
			}
			this.ticksUntilSpawn -= interval;
			this.CheckShouldSpawn();
		}

		// Token: 0x06005755 RID: 22357 RVA: 0x001D444B File Offset: 0x001D264B
		private void CheckShouldSpawn()
		{
			if (this.ticksUntilSpawn <= 0)
			{
				this.TryDoSpawn();
				this.ResetCountdown();
			}
		}

		// Token: 0x06005756 RID: 22358 RVA: 0x001D4464 File Offset: 0x001D2664
		public bool TryDoSpawn()
		{
			if (!this.parent.Spawned)
			{
				return false;
			}
			if (this.PropsSpawner.spawnMaxAdjacent >= 0)
			{
				int num = 0;
				bool foundloc = false;
				for (int i = 0; i < 9; i++)
				{
					IntVec3 c = this.parent.Position + GenAdj.AdjacentCellsAndInside[i];
					if (c.InBounds(this.parent.Map))
					{
						List<Thing> thingList = c.GetThingList(this.parent.Map);
						for (int j = 0; j < thingList.Count; j++)
						{
							if (thingList[j].def == this.PropsSpawner.thingToSpawn)
							{
								num += thingList[j].stackCount;
								if (num >= this.PropsSpawner.spawnMaxAdjacent)
								{
									return false;
								}
							}
						}
					}
				}
			}
			IntVec3 center;
			if (CompSpawnerTerrainDependant.TryFindSpawnCell(this.parent, this.PropsSpawner, out center))
			{
				Thing thing = ThingMaker.MakeThing(this.PropsSpawner.thingToSpawn, null);
				thing.stackCount = this.PropsSpawner.spawnCount;
				if (thing == null)
				{
					Log.Error("Could not spawn anything for " + this.parent, false);
				}
				if (this.PropsSpawner.inheritFaction && thing.Faction != this.parent.Faction)
				{
					thing.SetFaction(this.parent.Faction, null);
				}
				Thing t;
				GenPlace.TryPlaceThing(thing, center, this.parent.Map, ThingPlaceMode.Direct, out t, null, null, default(Rot4));
				if (this.PropsSpawner.spawnForbidden)
				{
					t.SetForbidden(true, true);
				}
				if (this.PropsSpawner.showMessageIfOwned && this.parent.Faction == Faction.OfPlayer)
				{
					Messages.Message("MessageCompSpawnerSpawnedItem".Translate(this.PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent, true);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06005757 RID: 22359 RVA: 0x001D4668 File Offset: 0x001D2868
		public static bool TryFindSpawnCell(Thing parent, CompProperties_SpawnerTerrainDependant PropsSpawner, out IntVec3 result)
		{
			ThingDef thingToSpawn = PropsSpawner.thingToSpawn;
			int spawnCount = PropsSpawner.spawnCount;
			foreach (IntVec3 intVec in GenAdj.CellsAdjacent8Way(parent).InRandomOrder(null))
			{
				if (intVec.Walkable(parent.Map))
				{
					if (!PropsSpawner.allowedTerrain.NullOrEmpty())
					{
						if (!PropsSpawner.allowedTerrain.Contains(intVec.GetTerrain(parent.Map)))
						{
							continue;
						}
					}
					if (!PropsSpawner.allowedAffordances.NullOrEmpty())
					{
						if (!PropsSpawner.allowedAffordances.Any(x => intVec.GetTerrain(parent.Map).affordances.Contains(x)))
						{
							continue;
						}
					}
					Building edifice = intVec.GetEdifice(parent.Map);
					if (edifice == null || !thingToSpawn.IsEdifice())
					{
						Building_Door building_Door = edifice as Building_Door;
						if ((building_Door == null || building_Door.FreePassage) && (parent.def.passability == Traversability.Impassable || GenSight.LineOfSight(parent.Position, intVec, parent.Map, false, null, 0, 0)))
						{
							bool flag = false;
							List<Thing> thingList = intVec.GetThingList(parent.Map);
							for (int i = 0; i < thingList.Count; i++)
							{
								Thing thing = thingList[i];
								if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
								{
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								result = intVec;
								return true;
							}
						}
					}
				}
			}
			result = IntVec3.Invalid;
			return false;
		}

		// Token: 0x06005758 RID: 22360 RVA: 0x001D47A4 File Offset: 0x001D29A4
		private void ResetCountdown()
		{
			this.ticksUntilSpawn = this.PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		// Token: 0x06005759 RID: 22361 RVA: 0x001D47BC File Offset: 0x001D29BC
		public override void PostExposeData()
		{
			string str = this.PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (this.PropsSpawner.saveKeysPrefix + "_");
			Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
		}

		// Token: 0x0600575A RID: 22362 RVA: 0x001D480C File Offset: 0x001D2A0C
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (Prefs.DevMode)
			{
				yield return new Command_Action
				{
					defaultLabel = "DEBUG: Spawn " + this.PropsSpawner.thingToSpawn.label,
					icon = TexCommand.DesirePower,
					action = delegate ()
					{
						this.TryDoSpawn();
						this.ResetCountdown();
					}
				};
			}
			yield break;
		}

		// Token: 0x0600575B RID: 22363 RVA: 0x001D481C File Offset: 0x001D2A1C
		public override string CompInspectStringExtra()
		{
			if (this.PropsSpawner.writeTimeLeftToSpawn && (!this.PropsSpawner.requiresPower || this.PowerOn))
			{
				return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(this.PropsSpawner.thingToSpawn, null, this.PropsSpawner.spawnCount)) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod(true, false, true, true);
			}
			return null;
		}

		// Token: 0x04003084 RID: 12420
		private int ticksUntilSpawn;
	}
}
