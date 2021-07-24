// RimWorld.InfestationUtility
using RimWorld;
using Verse;

namespace ExtraHives
{
	public static class InfestationUtility
    {
		public static Thing SpawnTunnels(ThingDef hiveDef, int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null, Faction faction = null)
		{
			ThingDef HiveDef = hiveDef ?? RimWorld.ThingDefOf.Hive;
			HiveDefExtension HiveExt = HiveDef.GetModExtension<HiveDefExtension>();
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
			HiveDefExtension extension = HiveDef.GetModExtension<HiveDefExtension>();
			if (extension!=null && extension.TunnelDef!=null)
			{
				TunnelDef = extension.TunnelDef;
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(TunnelDef), cell, map, WipeMode.FullRefund);
			TunnelHiveSpawner hiveSpawner = thing as TunnelHiveSpawner;
			if (hiveSpawner!=null)
			{
				if (hiveSpawner.faction == null && faction!=null)
				{
					hiveSpawner.faction = faction;
				}
			}
			if (hiveSpawner.SpawnedFaction != null)
			{
			//	Log.Message(hiveSpawner.Faction.def.defName + ": " + hiveSpawner.faction);
			}
			QuestUtility.AddQuestTag(thing, questTag);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				cell = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, HiveDef, HiveDef.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement, allowUnreachable: true);
				if (cell.IsValid)
				{
					thing = GenSpawn.Spawn(ThingMaker.MakeThing(TunnelDef), cell, map, WipeMode.FullRefund);
					hiveSpawner = thing as TunnelHiveSpawner;
					if (hiveSpawner != null)
					{
						if (hiveSpawner.faction == null && faction != null)
						{
							hiveSpawner.faction = faction;
						}
					}
					if (hiveSpawner.SpawnedFaction!=null)
					{
					//	Log.Message(hiveSpawner.Faction.def.defName+": "+ hiveSpawner.faction);
					}
					QuestUtility.AddQuestTag(thing, questTag);
				}
			}
			return thing;
		}
		public static Thing SpawnTunnels(ThingDef hiveDef, int hiveCount, Map map, IntVec3 cell, bool spawnAnywhereIfNoGoodCell = false, bool ignoreRoofedRequirement = false, string questTag = null, Faction faction = null)
		{
			ThingDef HiveDef = hiveDef ?? RimWorld.ThingDefOf.Hive;
			HiveDefExtension HiveExt = HiveDef.GetModExtension<HiveDefExtension>();
			ThingDef TunnelDef = HiveExt?.TunnelDef ?? RimWorld.ThingDefOf.TunnelHiveSpawner;
			HiveDefExtension extension = HiveDef.GetModExtension<HiveDefExtension>();
			if (extension!=null && extension.TunnelDef!=null)
			{
				TunnelDef = extension.TunnelDef;
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(TunnelDef), cell, map, WipeMode.FullRefund);
			TunnelHiveSpawner hiveSpawner = thing as TunnelHiveSpawner;
			if (hiveSpawner != null)
			{
				if (hiveSpawner.faction == null && faction != null)
				{
					hiveSpawner.faction = faction;
				}
			}
			if (hiveSpawner.SpawnedFaction != null)
			{
			//	Log.Message(hiveSpawner.Faction.def.defName + ": " + hiveSpawner.faction);
			}
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
					hiveSpawner = thing as TunnelHiveSpawner;
					if (hiveSpawner != null)
					{
						if (hiveSpawner.faction == null && faction != null)
						{
							hiveSpawner.faction = faction;
						}
					}
					if (hiveSpawner.SpawnedFaction != null)
					{
					//	Log.Message(hiveSpawner.Faction.def.defName + ": " + hiveSpawner.faction);
					}
					QuestUtility.AddQuestTag(thing, questTag);
				}
			}
			return thing;
		}
	}

}