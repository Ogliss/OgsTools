using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000B98 RID: 2968 ExtraHives.PawnsArrivalModeWorker_RandomTunnel
	public class PawnsArrivalModeWorker_RandomTunnel : PawnsArrivalModeWorker
	{
		// Token: 0x060045E0 RID: 17888 RVA: 0x00178498 File Offset: 0x00176698
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			Map map = (Map)parms.target;
			bool canRoofPunch = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
			for (int i = 0; i < pawns.Count; i++)
			{
				TunnelRaidUtility.DropThingsNear(DropCellFinder.RandomDropSpot(map), map, Gen.YieldSingle<Thing>(pawns[i]), parms.podOpenDelay, false, true, canRoofPunch);
			}
		}

		// Token: 0x060045E1 RID: 17889 RVA: 0x00178500 File Offset: 0x00176700
		public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
		{
			if (!parms.raidArrivalModeForQuickMilitaryAid)
			{
				parms.podOpenDelay = 520;
			}
			parms.spawnRotation = Rot4.Random;
			return true;
		}
	}
}
