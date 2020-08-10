using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace ExtraHives.GenStuff
{
	// Token: 0x02000054 RID: 84
	internal class SymbolResolver_OutdoorLightingHivebase : SymbolResolver
	{
		// Token: 0x06000190 RID: 400 RVA: 0x0000F358 File Offset: 0x0000D558
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef glowPod = RimWorld.ThingDefOf.GlowPod;
			this.FindNearbyGlowers(rp.rect);
			for (int i = 0; i < rp.rect.Area / 4; i++)
			{
				IntVec3 randomCell = rp.rect.RandomCell;
				bool flag = randomCell.Standable(map) && randomCell.GetFirstItem(map) == null && randomCell.GetFirstPawn(map) == null && randomCell.GetFirstBuilding(map) == null;
				if (flag)
				{
					Region region = randomCell.GetRegion(map, RegionType.Set_Passable);
					bool flag2 = region != null && region.Room.PsychologicallyOutdoors && region.Room.UsesOutdoorTemperature && !this.AnyGlowerNearby(randomCell) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(randomCell, map);
					if (flag2)
					{
						bool flag3 = rp.spawnBridgeIfTerrainCantSupportThing == null || rp.spawnBridgeIfTerrainCantSupportThing.Value;
						if (flag3)
						{
							BaseGenUtility.CheckSpawnBridgeUnder(glowPod, randomCell, Rot4.North);
						}
						Thing thing = GenSpawn.Spawn(glowPod, randomCell, map, WipeMode.Vanish);
						bool flag4 = thing.def.CanHaveFaction && thing.Faction != rp.faction;
						if (flag4)
						{
							thing.SetFaction(rp.faction, null);
						}
						SymbolResolver_OutdoorLightingHivebase.nearbyGlowers.Add(thing.TryGetComp<CompGlower>());
					}
				}
			}
			SymbolResolver_OutdoorLightingHivebase.nearbyGlowers.Clear();
		}

		// Token: 0x06000191 RID: 401 RVA: 0x0000F4CC File Offset: 0x0000D6CC
		private void FindNearbyGlowers(CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			SymbolResolver_OutdoorLightingHivebase.nearbyGlowers.Clear();
			rect = rect.ExpandedBy(4);
			rect = rect.ClipInsideMap(map);
			foreach (IntVec3 intVec in rect)
			{
				Region region = intVec.GetRegion(map, RegionType.Set_Passable);
				bool flag = region != null && region.Room.PsychologicallyOutdoors;
				if (flag)
				{
					List<Thing> thingList = intVec.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						CompGlower compGlower = thingList[i].TryGetComp<CompGlower>();
						bool flag2 = compGlower != null;
						if (flag2)
						{
							SymbolResolver_OutdoorLightingHivebase.nearbyGlowers.Add(compGlower);
						}
					}
				}
			}
		}

		// Token: 0x06000192 RID: 402 RVA: 0x0000F5BC File Offset: 0x0000D7BC
		private bool AnyGlowerNearby(IntVec3 c)
		{
			for (int i = 0; i < SymbolResolver_OutdoorLightingHivebase.nearbyGlowers.Count; i++)
			{
				bool flag = c.InHorDistOf(SymbolResolver_OutdoorLightingHivebase.nearbyGlowers[i].parent.Position, SymbolResolver_OutdoorLightingHivebase.nearbyGlowers[i].Props.glowRadius + 2f);
				if (flag)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040000C6 RID: 198
		private static List<CompGlower> nearbyGlowers = new List<CompGlower>();

		// Token: 0x040000C7 RID: 199
		private const float Margin = 2f;
	}
}
