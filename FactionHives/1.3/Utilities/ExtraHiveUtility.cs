using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
	public static class HiveUtility
	{
		public static int TotalSpawnedHivesCount(Map map, ThingDef thingDef = null)
		{
			ThingDef def = thingDef ?? RimWorld.ThingDefOf.Hive;
			return map.listerThings.ThingsOfDef(def).Count;
		}

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

		private const float HivePreventsClaimingInRadius = 2f;
	}
}
