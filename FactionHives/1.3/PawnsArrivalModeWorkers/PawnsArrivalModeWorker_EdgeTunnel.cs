using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ExtraHives
{
	// ExtraHives.PawnsArrivalModeWorker_EdgeTunnel
	public class PawnsArrivalModeWorker_EdgeTunnel : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			PawnsArrivalModeWorkerUtility.PlaceInTunnelsNearSpawnCenter(parms, pawns);
		}

		public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			IntVec3 near = DropCellFinder.FindRaidDropCenterDistant(map, false);
			TunnelsArrivalActionUtility.PlaceTravelingTunnelers(dropPods, near, map);
		}

		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!parms.spawnCenter.IsValid)
			{
				parms.spawnCenter = DropCellFinder.FindRaidDropCenterDistant(map, false);
			}
			parms.spawnRotation = Rot4.Random;
			return true;
		}
	}
}
