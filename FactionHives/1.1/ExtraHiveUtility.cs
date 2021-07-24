using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000CA2 RID: 3234
	public static class HiveUtility
	{
		// Token: 0x06004E30 RID: 20016 RVA: 0x001A4409 File Offset: 0x001A2609
		public static int TotalSpawnedHivesCount(Map map, ThingDef thingDef = null)
		{
			ThingDef def = thingDef ?? RimWorld.ThingDefOf.Hive;
			return map.listerThings.ThingsOfDef(def).Count;
		}

		// Token: 0x06004E31 RID: 20017 RVA: 0x001A4420 File Offset: 0x001A2620
		public static bool AnyHivePreventsClaiming(Thing thing)
		{
			if (!thing.Spawned)
			{
				return false;
			}
			int num = GenRadial.NumCellsInRadius(2f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = thing.Position + GenRadial.RadialPattern[i];
				if (c.InBounds(thing.Map) && c.GetFirstThing<Thing>(thing.Map) != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06004E32 RID: 20018 RVA: 0x001A4484 File Offset: 0x001A2684
		public static void Notify_HiveDespawned(Hive hive, Map map)
		{
			int num = GenRadial.NumCellsInRadius(2f);
			for (int i = 0; i < num; i++)
			{
				IntVec3 c = hive.Position + GenRadial.RadialPattern[i];
				if (c.InBounds(map))
				{
					List<Thing> thingList = c.GetThingList(map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j].Faction == Faction.OfInsects && !HiveUtility.AnyHivePreventsClaiming(thingList[j]) && !(thingList[j] is Pawn))
						{
							thingList[j].SetFaction(null, null);
						}
					}
				}
			}
		}

		// Token: 0x04002BE4 RID: 11236
		private const float HivePreventsClaimingInRadius = 2f;
	}
}
