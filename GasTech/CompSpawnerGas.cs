using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasTech
{
	// Token: 0x02000DE0 RID: 3552
	public class CompProperties_SpawnerGas : CompProperties_Spawner
	{
		// Token: 0x0600574E RID: 22350 RVA: 0x001D4343 File Offset: 0x001D2543
		public CompProperties_SpawnerGas()
		{
			this.compClass = typeof(CompSpawnerGas);
		}
		public float spawnConcentration = 1f;

	}
	public class CompSpawnerGas : CompSpawner
	{
		public new CompProperties_SpawnerGas PropsSpawner => this.props as CompProperties_SpawnerGas;
		public new bool TryDoSpawn()
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
				GasCloud cloud = t as GasCloud;
                if (cloud != null)
				{
					cloud.ReceiveConcentration(this.PropsSpawner.spawnConcentration);
				}
				return true;
			}
			return false;
		}
	}
}
