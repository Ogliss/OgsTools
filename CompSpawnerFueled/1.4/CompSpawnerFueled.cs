using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace CompSpawnerFueled
{
	// Token: 0x02000DD0 RID: 3536 CompSpawnerFueled.CompProperties_SpawnerFueled
	public class CompProperties_SpawnerFueled : CompProperties
	{
		// Token: 0x06005694 RID: 22164 RVA: 0x001CF7B7 File Offset: 0x001CD9B7
		public CompProperties_SpawnerFueled()
		{
			this.compClass = typeof(CompSpawnerFueled);
		}

		// Token: 0x04002FFA RID: 12282
		public ThingDef thingToSpawn;

		// Token: 0x04002FFB RID: 12283
		public int spawnCount = 1;

		// Token: 0x04002FFC RID: 12284
		public IntRange spawnIntervalRange = new IntRange(100, 100);

		// Token: 0x04002FFD RID: 12285
		public int spawnMaxAdjacent = -1;

		// Token: 0x04002FFE RID: 12286
		public bool spawnForbidden;

		// Token: 0x04002FFF RID: 12287
		public bool requiresPower;

		// Token: 0x04003000 RID: 12288
		public bool writeTimeLeftToSpawn;

		// Token: 0x04003001 RID: 12289
		public bool showMessageIfOwned;

		// Token: 0x04003002 RID: 12290
		public string saveKeysPrefix;

		// Token: 0x04003003 RID: 12291
		public bool inheritFaction;

		public float fuelUsedPerSpawn = 0f;
	}

	// Token: 0x02000DD1 RID: 3537
	public class CompSpawnerFueled : ThingComp
	{
		// Token: 0x17000F4F RID: 3919
		// (get) Token: 0x06005695 RID: 22165 RVA: 0x001CF7EC File Offset: 0x001CD9EC
		public CompProperties_SpawnerFueled PropsSpawner
		{
			get
			{
				return (CompProperties_SpawnerFueled)this.props;
			}
		}

		public CompRefuelable Fuel
		{
			get
			{
				return parent.TryGetComp<CompRefuelable>();
			}
		}
		// Token: 0x17000F50 RID: 3920
		// (get) Token: 0x06005696 RID: 22166 RVA: 0x001CF7FC File Offset: 0x001CD9FC
		private bool PowerOn
		{
			get
			{
				CompPowerTrader comp = this.parent.GetComp<CompPowerTrader>();
				return comp != null && comp.PowerOn;
			}
		}

		// Token: 0x06005697 RID: 22167 RVA: 0x001CF820 File Offset: 0x001CDA20
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!respawningAfterLoad)
			{
				this.ResetCountdown();
			}
		}

		// Token: 0x06005698 RID: 22168 RVA: 0x001CF82B File Offset: 0x001CDA2B
		public override void CompTick()
		{
			this.TickInterval(1);
		}

		// Token: 0x06005699 RID: 22169 RVA: 0x001CF834 File Offset: 0x001CDA34
		public override void CompTickRare()
		{
			this.TickInterval(250);
		}

		// Token: 0x0600569A RID: 22170 RVA: 0x001CF844 File Offset: 0x001CDA44
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

		// Token: 0x0600569B RID: 22171 RVA: 0x001CF8BF File Offset: 0x001CDABF
		private void CheckShouldSpawn()
		{
			bool spawn = true;
			if (Fuel!=null)
			{
				spawn = Fuel.Fuel > PropsSpawner.fuelUsedPerSpawn;
			}
			if (this.ticksUntilSpawn <= 0 && spawn)
			{
				this.TryDoSpawn();
				this.ResetCountdown();
			}
		}

		// Token: 0x0600569C RID: 22172 RVA: 0x001CF8D8 File Offset: 0x001CDAD8
		public bool TryDoSpawn()
		{
			if (!this.parent.Spawned)
			{
				return false;
			}
			if (this.PropsSpawner.spawnMaxAdjacent >= 0)
			{
				int num = 0;
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
			if (CompSpawner.TryFindSpawnCell(this.parent, this.PropsSpawner.thingToSpawn, this.PropsSpawner.spawnCount, out center))
			{
				if (Fuel != null)
				{
					Fuel.ConsumeFuel(PropsSpawner.fuelUsedPerSpawn);
				}
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

		// Token: 0x0600569D RID: 22173 RVA: 0x001CFADC File Offset: 0x001CDCDC
		public static bool TryFindSpawnCell(Thing parent, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
		{
			foreach (IntVec3 intVec in GenAdj.CellsAdjacent8Way(parent).InRandomOrder(null))
			{
				if (intVec.Walkable(parent.Map))
				{
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

		// Token: 0x0600569E RID: 22174 RVA: 0x001CFC18 File Offset: 0x001CDE18
		private void ResetCountdown()
		{
			this.ticksUntilSpawn = this.PropsSpawner.spawnIntervalRange.RandomInRange;
		}

		// Token: 0x0600569F RID: 22175 RVA: 0x001CFC30 File Offset: 0x001CDE30
		public override void PostExposeData()
		{
			string str = this.PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (this.PropsSpawner.saveKeysPrefix + "_");
			Scribe_Values.Look<int>(ref this.ticksUntilSpawn, str + "ticksUntilSpawn", 0, false);
		}

		// Token: 0x060056A0 RID: 22176 RVA: 0x001CFC80 File Offset: 0x001CDE80
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

		// Token: 0x060056A1 RID: 22177 RVA: 0x001CFC90 File Offset: 0x001CDE90
		public override string CompInspectStringExtra()
		{
			if (this.PropsSpawner.writeTimeLeftToSpawn && (!this.PropsSpawner.requiresPower || this.PowerOn))
			{
				return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(this.PropsSpawner.thingToSpawn, null, this.PropsSpawner.spawnCount)) + ": " + this.ticksUntilSpawn.ToStringTicksToPeriod(true, false, true, true);
			}
			return null;
		}

		// Token: 0x04003004 RID: 12292
		private int ticksUntilSpawn;
	}
}
