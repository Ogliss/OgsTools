// RimWorld.IncidentWorker_Infestation
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives //ExtraHives.IncidentWorker_Infestation
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
		public const float HivePoints = 220f;

		protected override bool CanFireNowSub(IncidentParms parms)
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
			HiveDefExtension HiveExt = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
			IntVec3 cell;
			if (/*base.CanFireNowSub(parms) && */HiveUtility.TotalSpawnedHivesCount(map,def.mechClusterBuilding) < 30)
			{
				return InfestationCellFinder.TryFindCell(out cell, map, HiveExt);
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (def.mechClusterBuilding == null)
			{
				return false;
			}
			if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
			{
				return false;
			}
			HiveDefExtension ext = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
			if (parms.faction==null)
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
			CompProperties_SpawnerPawn spawnerPawn = def.mechClusterBuilding.GetCompProperties<CompProperties_SpawnerPawn>();
			float points = spawnerPawn?.initialPawnsPoints ?? 250f;
			Map map = (Map)parms.target;
			Thing t = InfestationUtility.SpawnTunnels(def.mechClusterBuilding, Mathf.Max(GenMath.RoundRandom( parms.points / points), 1), map, faction: parms.faction);
			SendStandardLetter(parms, t);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
	}

}