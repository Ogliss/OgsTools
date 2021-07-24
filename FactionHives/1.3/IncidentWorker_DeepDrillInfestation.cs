using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000A2D RID: 2605
	public class IncidentWorker_DeepDrillInfestation : IncidentWorker
	{
		// Token: 0x06003E45 RID: 15941 RVA: 0x00148B39 File Offset: 0x00146D39
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

		// Token: 0x06003E46 RID: 15942 RVA: 0x00148B70 File Offset: 0x00146D70
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

		// Token: 0x0400252D RID: 9517
		private static List<Thing> tmpDrills = new List<Thing>();

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
