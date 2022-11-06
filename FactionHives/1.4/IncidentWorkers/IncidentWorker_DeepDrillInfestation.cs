using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	public class IncidentWorker_DeepDrillInfestation : IncidentWorker
	{
		public override bool CanFireNowSub(IncidentParms parms)
		{
			if ( def.mechClusterBuilding == null)
			{
				return false;
			}
			if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
			{
				return false;
			}
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IncidentWorker_DeepDrillInfestation.tmpDrills.Clear();
			DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, IncidentWorker_DeepDrillInfestation.tmpDrills);
			return IncidentWorker_DeepDrillInfestation.tmpDrills.Any<Thing>();
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (def.mechClusterBuilding == null)
			{
				return false;
			}
			if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
			{
				return false;
			}
			ThingDef hiveDef = def.mechClusterBuilding;
			HiveDefExtension hive = hiveDef.GetModExtension<HiveDefExtension>();
			if (hive == null)
			{
				return false;
			}
			HiveDefExtension ext = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
			if (parms.faction == null)
			{
				try
				{
					parms.faction = Find.FactionManager.AllFactions.Where(x => x.def.defName.Contains(ext.Faction.defName))/*.Where(x => (float)GenDate.DaysPassed >= x.def.earliestRaidDays)*/.RandomElement();
				//	Log.Message(parms.faction.def.defName);
				}
				catch (System.Exception)
				{
					parms.faction = Find.FactionManager.FirstFactionOfDef(ext.Faction);
				}
			}
			ThingDef tunnelDef = hive.TunnelDef ?? RimWorld.ThingDefOf.TunnelHiveSpawner;
			IncidentWorker_DeepDrillInfestation.tmpDrills.Clear();
			DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, IncidentWorker_DeepDrillInfestation.tmpDrills);
			Thing deepDrill;
			if (!IncidentWorker_DeepDrillInfestation.tmpDrills.TryRandomElement(out deepDrill))
			{
				return false;
			}
			IntVec3 intVec = CellFinder.FindNoWipeSpawnLocNear(deepDrill.Position, map, tunnelDef, Rot4.North, 2, (IntVec3 x) => x.Walkable(map) && x.GetFirstThing(map, deepDrill.def) == null && x.GetFirstThingWithComp<ThingComp>(map) == null && x.GetFirstThing(map, RimWorld.ThingDefOf.Hive) == null && x.GetFirstThing(map, RimWorld.ThingDefOf.TunnelHiveSpawner) == null);
			if (intVec == deepDrill.Position)
			{
				return false;
			}
			TunnelHiveSpawner tunnelHiveSpawner = (TunnelHiveSpawner)ThingMaker.MakeThing(tunnelDef, null);
			tunnelHiveSpawner.spawnHive = false;
			Rand.PushState();
			tunnelHiveSpawner.initialPoints = Mathf.Clamp(parms.points * Rand.Range(0.3f, 0.6f), 200f, 1000f);
			Rand.PopState();
			tunnelHiveSpawner.spawnedByInfestationThingComp = true;
			GenSpawn.Spawn(tunnelHiveSpawner, intVec, map, WipeMode.FullRefund);
			deepDrill.TryGetComp<CompCreatesInfestations>().Notify_CreatedInfestation();
			base.SendStandardLetter(parms, new TargetInfo(tunnelHiveSpawner.Position, map, false), Array.Empty<NamedArgument>());
			return true;
		}

		private static List<Thing> tmpDrills = new List<Thing>();
		private const float MinPointsFactor = 0.3f;
		private const float MaxPointsFactor = 0.6f;
		private const float MinPoints = 200f;
		private const float MaxPoints = 1000f;
	}
}
