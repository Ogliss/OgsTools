using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ExtraHives
{
	// ExtraHives.PawnsArrivalModeWorker_CenterTunnel
	public class PawnsArrivalModeWorker_CenterTunnel : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			PawnsArrivalModeWorkerUtility.PlaceInTunnelsNearSpawnCenter(parms, pawns);
		}

		public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			IntVec3 near;
			if (!DropCellFinder.TryFindRaidDropCenterClose(out near, map, true, true, true, -1))
			{
				near = DropCellFinder.FindRaidDropCenterDistant(map, false);
			}
			TunnelsArrivalActionUtility.PlaceTravelingTunnelers(dropPods, near, map);
		}

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

		public const int PodOpenDelay = 520;
	}
}
