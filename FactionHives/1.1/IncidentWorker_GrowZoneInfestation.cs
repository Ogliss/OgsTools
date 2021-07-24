using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000A2D RID: 2605
	public class IncidentWorker_GrowZoneInfestation : IncidentWorker
	{
		// Token: 0x06003E45 RID: 15941 RVA: 0x00148B39 File Offset: 0x00146D39
		public override bool CanFireNowSub(IncidentParms parms)
		{
			if ( def.mechClusterBuilding == null)
			{
				Log.Error("Hivedef (def.mechClusterBuilding) not set");
				return false;
			}
			if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
			{
				Log.Error("Hivedef (def.mechClusterBuilding) missing HiveExtension");
				return false;
			}
			if (!base.CanFireNowSub(parms))
			{
				Log.Error("!base.CanFireNowSub");
				return false;
			}
			Map map = (Map)parms.target;
			IncidentWorker_GrowZoneInfestation.tmpZones.Clear();
			ExtraInfestationIncidentUtility.GetUsableGrowZones(map, IncidentWorker_GrowZoneInfestation.tmpZones);
			return true;
		}

		// Token: 0x06003E46 RID: 15942 RVA: 0x00148B70 File Offset: 0x00146D70
		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (def.mechClusterBuilding == null)
			{
				Log.Error("Hivedef (def.mechClusterBuilding) not set");
				return false;
			}
			if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
			{
				Log.Error("Hivedef (def.mechClusterBuilding) missing HiveExtension");
				return false;
			}
			ThingDef hiveDef = def.mechClusterBuilding;
			HiveDefExtension hive = hiveDef.GetModExtension<HiveDefExtension>();
			if (hive == null)
			{
				return false;
			}
			if (parms.faction == null)
			{
				try
				{
					parms.faction = Find.FactionManager.AllFactions.Where(x => x.def.defName.Contains(hive.Faction.defName))/*.Where(x => (float)GenDate.DaysPassed >= x.def.earliestRaidDays)*/.RandomElement();
				//	Log.Message(parms.faction.def.defName);
				}
				catch (System.Exception)
				{
					parms.faction = Find.FactionManager.FirstFactionOfDef(hive.Faction);
				}
			}
			ThingDef tunnelDef = hive.TunnelDef ?? RimWorld.ThingDefOf.TunnelHiveSpawner;
			IncidentWorker_GrowZoneInfestation.tmpZones.Clear();
			ExtraInfestationIncidentUtility.GetUsableGrowZones(map, IncidentWorker_GrowZoneInfestation.tmpZones);
			Zone_Growing growZone;
			IntVec3 intVec = IntVec3.Invalid;
			if (IncidentWorker_GrowZoneInfestation.tmpZones.TryRandomElementByWeight( x=> x.Cells.Count, out growZone))
			{
				intVec = CellFinder.FindNoWipeSpawnLocNear(growZone.Cells.RandomElement(), map, tunnelDef, Rot4.North, 2, (IntVec3 x) => x.Walkable(map) && x.GetFirstThing(map, hiveDef) == null && x.GetFirstThingWithComp<ThingComp>(map) == null && x.GetFirstThing(map, RimWorld.ThingDefOf.Hive) == null && x.GetFirstThing(map, RimWorld.ThingDefOf.TunnelHiveSpawner) == null && !x.Roofed(map) && x.UsesOutdoorTemperature(map));
				if (intVec == growZone.Position)
				{
					Log.Error("intVec == growZone.Position");
					return false;
				}
			}
			else
			{
				RCellFinder.TryFindRandomPawnEntryCell(out intVec, map, 0);
				if (RCellFinder.TryFindRandomSpotJustOutsideColony(intVec, map, out intVec))
				{
					Log.Warning("Found spot outside colony");
				}
				else
				{
					Log.Warning("failed to find interesting location, use map edge");
				}
			//	intVec = CellFinder.FindNoWipeSpawnLocNear(intVec, map, hiveDef, Rot4.North, 10, (IntVec3 x) => x.Walkable(map) && x.GetFirstThing(map, hiveDef) == null && x.GetFirstThingWithComp<ThingComp>(map) == null && x.GetFirstThing(map, ThingDefOf.Hive) == null && x.GetFirstThing(map, ThingDefOf.TunnelHiveSpawner) == null && !x.Roofed(map) && x.UsesOutdoorTemperature(map));
			}
			if (intVec == IntVec3.Invalid)
			{
				Log.Error("intVec == IntVec3.Invalid");
				return false;
			}
			CompProperties_SpawnerPawn spawnerPawn = def.mechClusterBuilding.GetCompProperties<CompProperties_SpawnerPawn>();
			float points = spawnerPawn?.initialPawnsPoints ?? 250f;
			Thing t = InfestationUtility.SpawnTunnels(def.mechClusterBuilding, Mathf.Max(GenMath.RoundRandom(parms.points / points), 1), map, intVec, true, true, faction: parms.faction);
			
			/*
			TunnelHiveSpawner tunnelHiveSpawner = (TunnelHiveSpawner)ThingMaker.MakeThing(tunnelDef, null);
			tunnelHiveSpawner.spawnHive = true;
			tunnelHiveSpawner.insectsPoints = Mathf.Clamp(parms.points * Rand.Range(0.3f, 0.6f), 200f, 1000f);
			tunnelHiveSpawner.spawnedByInfestationThingComp = true;
			GenSpawn.Spawn(tunnelHiveSpawner, intVec, map, WipeMode.FullRefund);
			*/
			base.SendStandardLetter(parms, new TargetInfo(intVec, map, false), Array.Empty<NamedArgument>());
			return true;
		}

		// Token: 0x0400252D RID: 9517
		private static List<Zone_Growing> tmpZones = new List<Zone_Growing>();

		// Token: 0x0400252E RID: 9518
		private const float MinPointsFactor = 0.3f;

		// Token: 0x0400252F RID: 9519
		private const float MaxPointsFactor = 0.6f;

		// Token: 0x04002530 RID: 9520
		private const float MinPoints = 200f;

		// Token: 0x04002531 RID: 9521
		private const float MaxPoints = 1000f;
	}
}
