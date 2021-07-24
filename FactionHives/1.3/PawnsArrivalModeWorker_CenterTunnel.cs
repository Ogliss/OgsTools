using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000B92 RID: 2962 ExtraHives.PawnsArrivalModeWorker_CenterTunnel
	public class PawnsArrivalModeWorker_CenterTunnel : PawnsArrivalModeWorker
	{
		// Token: 0x060045CB RID: 17867 RVA: 0x001780CB File Offset: 0x001762CB
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			PawnsArrivalModeWorkerUtility.PlaceInTunnelsNearSpawnCenter(parms, pawns);
		}

		// Token: 0x060045CC RID: 17868 RVA: 0x001780D4 File Offset: 0x001762D4
		public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			IntVec3 near;
			if (!DropCellFinder.TryFindRaidDropCenterClose(out near, map, true, true, true, -1))
			{
				near = DropCellFinder.FindRaidDropCenterDistant(map, false);
			}
			TunnelsArrivalActionUtility.PlaceTravelingTunnelers(dropPods, near, map);
		}

		// Token: 0x060045CD RID: 17869 RVA: 0x00178100 File Offset: 0x00176300
		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!parms.raidArrivalModeForQuickMilitaryAid)
			{
				parms.podOpenDelay = 520;
			}
			parms.spawnRotation = Rot4.Random;
			if (!parms.spawnCenter.IsValid)
			{
				bool flag = parms.faction == Faction.OfMechanoids;
				bool flag2 = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
				Rand.PushState();
				if (Rand.Chance(0.4f) && !flag && map.listerBuildings.ColonistsHaveBuildingWithPowerOn(RimWorld.ThingDefOf.OrbitalTradeBeacon))
				{
					parms.spawnCenter = DropCellFinder.TradeDropSpot(map);
				}
				else if (!DropCellFinder.TryFindRaidDropCenterClose(out parms.spawnCenter, map, !flag && flag2, !flag, true, -1))
				{
					
					parms.raidArrivalMode = Rand.Chance(0.75f) ? PawnsArrivalModeDefOf.EdgeTunnelIn_ExtraHives : PawnsArrivalModeDefOf.EdgeTunnelInGroups_ExtraHives;
					return parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms);
				}
				Rand.PopState();
			}
			return true;
		}

		// Token: 0x04002875 RID: 10357
		public const int PodOpenDelay = 520;
	}
}
