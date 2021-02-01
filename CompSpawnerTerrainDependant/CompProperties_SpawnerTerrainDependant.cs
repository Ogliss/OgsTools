using System;
using System.Collections.Generic;
using Verse;

namespace CompSpawnerTerrainDependant
{
	// CompSpawnerTerrainDependant.CompProperties_SpawnerTerrainDependant
	public class CompProperties_SpawnerTerrainDependant : CompProperties
	{
		// Token: 0x0600574E RID: 22350 RVA: 0x001D4343 File Offset: 0x001D2543
		public CompProperties_SpawnerTerrainDependant()
		{
			this.compClass = typeof(CompSpawnerTerrainDependant);
		}
		public List<TerrainDef> allowedTerrain = new List<TerrainDef>();
		public List<TerrainAffordanceDef> allowedAffordances = new List<TerrainAffordanceDef>();

		// Token: 0x0400307A RID: 12410
		public ThingDef thingToSpawn;

		// Token: 0x0400307B RID: 12411
		public int spawnCount = 1;

		// Token: 0x0400307C RID: 12412
		public IntRange spawnIntervalRange = new IntRange(100, 100);

		// Token: 0x0400307D RID: 12413
		public int spawnMaxAdjacent = -1;

		// Token: 0x0400307E RID: 12414
		public bool spawnForbidden;

		// Token: 0x0400307F RID: 12415
		public bool requiresPower;

		// Token: 0x04003080 RID: 12416
		public bool writeTimeLeftToSpawn;

		// Token: 0x04003081 RID: 12417
		public bool showMessageIfOwned;

		// Token: 0x04003082 RID: 12418
		public string saveKeysPrefix;

		// Token: 0x04003083 RID: 12419
		public bool inheritFaction;
	}
}
