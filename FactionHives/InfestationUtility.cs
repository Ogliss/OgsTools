// RimWorld.InfestationUtility
using RimWorld;
using Verse;

namespace ExtraHives
{
	public static class InfestationUtility
	{
		public static Thing SpawnTunnels(ThingDef hiveDef, int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null)
		{
			ThingDef HiveDef = hiveDef ?? RimWorld.ThingDefOf.Hive;
			HiveExtension HiveExt = HiveDef.GetModExtension<HiveExtension>();
			ThingDef TunnelDef = HiveExt?.TunnelDef ?? RimWorld.ThingDefOf.TunnelHiveSpawner;
			if (!InfestationCellFinder.TryFindCell(out IntVec3 cell, map, HiveExt))
			{
				if (!spawnAnywhereIfNoGoodCell)
				{
					return null;
				}
				if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(delegate (IntVec3 x)
				{
					if (!x.Standable(map) || x.Fogged(map))
					{
						return false;
					}
					bool flag = false;
					int num = GenRadial.NumCellsInRadius(3f);
					for (int j = 0; j < num; j++)
					{
						IntVec3 c = x + GenRadial.RadialPattern[j];
						if (c.InBounds(map))
						{
							RoofDef roof = c.GetRoof(map);
							if (roof != null && roof.isThickRoof)
							{
								flag = true;
								break;
							}
						}
					}
					return flag ? true : false;
				}, map, out cell))
				{
					return null;
				}
			}
			HiveExtension extension = HiveDef.GetModExtension<HiveExtension>();
			if (extension!=null && extension.TunnelDef!=null)
			{
				TunnelDef = extension.TunnelDef;
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(TunnelDef), cell, map, WipeMode.FullRefund);
			QuestUtility.AddQuestTag(thing, questTag);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				cell = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, HiveDef, HiveDef.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, allowUnreachable: true);
				if (cell.IsValid)
				{
					thing = GenSpawn.Spawn(ThingMaker.MakeThing(TunnelDef), cell, map, WipeMode.FullRefund);
					QuestUtility.AddQuestTag(thing, questTag);
				}
			}
			return thing;
		}
		public static Thing SpawnTunnels(ThingDef hiveDef, int hiveCount, Map map, IntVec3 cell, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null)
		{
			ThingDef HiveDef = hiveDef ?? RimWorld.ThingDefOf.Hive;
			HiveExtension HiveExt = HiveDef.GetModExtension<HiveExtension>();
			ThingDef TunnelDef = HiveExt?.TunnelDef ?? RimWorld.ThingDefOf.TunnelHiveSpawner;
			HiveExtension extension = HiveDef.GetModExtension<HiveExtension>();
			if (extension!=null && extension.TunnelDef!=null)
			{
				TunnelDef = extension.TunnelDef;
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(TunnelDef), cell, map, WipeMode.FullRefund);
			QuestUtility.AddQuestTag(thing, questTag);
			CompSpawnerHives spawnerHives = thing.TryGetComp<CompSpawnerHives>();
			if (spawnerHives?.Props.tunnelDef!=null)
			{
				TunnelDef = spawnerHives.Props.tunnelDef;
			}
			for (int i = 0; i < hiveCount - 1; i++)
			{
				cell = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, HiveDef, HiveDef.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, allowUnreachable: true);
				if (cell.IsValid)
				{
					thing = GenSpawn.Spawn(ThingMaker.MakeThing(TunnelDef), cell, map, WipeMode.FullRefund);
					QuestUtility.AddQuestTag(thing, questTag);
				}
			}
			return thing;
		}
	}

}