﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ExtraHives
{
    class TunnelsArrivalActionUtility 
	{
		public static void PlaceTravelingTunnelers(List<ActiveDropPodInfo> dropPods, IntVec3 near, Map map)
		{
			TransportPodsArrivalActionUtility.RemovePawnsFromWorldPawns(dropPods);
			for (int i = 0; i < dropPods.Count; i++)
			{
				IntVec3 c;
				DropCellFinder.TryFindDropSpotNear(near, map, out c, false, true, true, null);
				TunnelRaidUtility.MakeTunnelAt(c, map, dropPods[i]);
			}
		}
	}
}
