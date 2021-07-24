using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000B95 RID: 2965 ExtraHives.PawnsArrivalModeWorker_EdgeTunnelGroups
	public class PawnsArrivalModeWorker_EdgeTunnelGroups : PawnsArrivalModeWorker
	{
		// Token: 0x060045D7 RID: 17879 RVA: 0x001782AC File Offset: 0x001764AC
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			Map map = (Map)parms.target;
			bool canRoofPunch = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
			List<Pair<List<Pawn>, IntVec3>> list = PawnsArrivalModeWorkerUtility.SplitIntoRandomGroupsNearMapEdge(pawns, map, true);
			PawnsArrivalModeWorkerUtility.SetPawnGroupsInfo(parms, list);
			for (int i = 0; i < list.Count; i++)
			{
				TunnelRaidUtility.DropThingsNear(list[i].Second, map, list[i].First.Cast<Thing>(), parms.podOpenDelay, false, true, canRoofPunch);
			}
		}

		// Token: 0x060045D8 RID: 17880 RVA: 0x00178337 File Offset: 0x00176537
		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			parms.spawnRotation = Rot4.Random;
			return true;
		}
	}
}
