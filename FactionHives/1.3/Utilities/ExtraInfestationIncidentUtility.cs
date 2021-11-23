using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
	public static class ExtraInfestationIncidentUtility
	{
		public static void GetUsableDeepDrills(Map map, List<Thing> outDrills)
		{
			outDrills.Clear();
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.CreatesInfestations);
			Faction ofPlayer = Faction.OfPlayer;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Faction == ofPlayer && list[i].TryGetComp<CompCreatesInfestations>().CanCreateInfestationNow)
				{
					outDrills.Add(list[i]);
				}
			}
		}

		public static void GetUsableGrowZones(Map map, List<Zone_Growing> outGrowZones)
		{
			outGrowZones.Clear();
			List<Zone> list = map.zoneManager.AllZones;
			Faction ofPlayer = Faction.OfPlayer;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is Zone_Growing && list[i].Cells.Any(x=> x.UsesOutdoorTemperature(map) || !x.Roofed(map)))
				{
					outGrowZones.Add(list[i] as Zone_Growing);
				}
			}
		}
	}
}
