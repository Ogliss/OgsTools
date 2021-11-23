using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
	// ExtraHives.PawnsArrivalModeWorker_RandomTunnel
	public class PawnsArrivalModeWorker_RandomTunnel : PawnsArrivalModeWorker
	{
		public override void Arrive(List<Pawn> pawns, IncidentParms parms)
		{
			Map map = (Map)parms.target;
			bool canRoofPunch = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
			for (int i = 0; i < pawns.Count; i++)
			{
				TunnelRaidUtility.DropThingsNear(DropCellFinder.RandomDropSpot(map), map, Gen.YieldSingle<Thing>(pawns[i]), parms.podOpenDelay, false, true, canRoofPunch);
			}
		}

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
