using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000B94 RID: 2964 ExtraHives.PawnsArrivalModeWorker_EdgeTunnel
	public class PawnsArrivalModeWorker_EdgeTunnel : PawnsArrivalModeWorker
	{
		// Token: 0x060045D3 RID: 17875 RVA: 0x001780CB File Offset: 0x001762CB
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			PawnsArrivalModeWorkerUtility.PlaceInTunnelsNearSpawnCenter(parms, pawns);
		}

		// Token: 0x060045D4 RID: 17876 RVA: 0x0017824C File Offset: 0x0017644C
		public override void TravelingTransportPodsArrived(List<ActiveDropPodInfo> dropPods, Map map)
		{
			IntVec3 near = DropCellFinder.FindRaidDropCenterDistant(map, false);
			TunnelsArrivalActionUtility.PlaceTravelingTunnelers(dropPods, near, map);
		}

		// Token: 0x060045D5 RID: 17877 RVA: 0x0017826C File Offset: 0x0017646C
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
