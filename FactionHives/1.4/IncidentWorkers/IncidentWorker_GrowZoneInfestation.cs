﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	public class IncidentWorker_GrowZoneInfestation : IncidentWorker
	{
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

		private static List<Zone_Growing> tmpZones = new List<Zone_Growing>();
		private const float MinPointsFactor = 0.3f;
		private const float MaxPointsFactor = 0.6f;
		private const float MinPoints = 200f;
		private const float MaxPoints = 1000f;
	}
}
