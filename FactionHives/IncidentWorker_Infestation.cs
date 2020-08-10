// RimWorld.IncidentWorker_Infestation
using RimWorld;
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
			Map map = (Map)parms.target;
			Thing t = InfestationUtility.SpawnTunnels(def.mechClusterBuilding, Mathf.Max(GenMath.RoundRandom( parms.points / 250f), 1), map);
			SendStandardLetter(parms, t);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
	}

}