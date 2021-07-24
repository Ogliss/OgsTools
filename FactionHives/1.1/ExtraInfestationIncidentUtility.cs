using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000A2E RID: 2606
	public static class ExtraInfestationIncidentUtility
	{
		// Token: 0x06003E49 RID: 15945 RVA: 0x00148CA0 File Offset: 0x00146EA0
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
