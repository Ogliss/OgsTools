using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace ExtraHives.GenStuff
{
	// Token: 0x020010B0 RID: 4272 ExtraHives.GenStuff.SymbolResolver_HiveMountMaker
	public class SymbolResolver_HiveBaseMoundMaker : SymbolResolver
	{

		List<IntVec3> cells = new List<IntVec3>();
		Faction Faction = null;
		// Token: 0x0600650C RID: 25868 RVA: 0x00233C80 File Offset: 0x00231E80
		public override void Resolve(ResolveParams rp)
		{
			Faction = rp.faction;
			HiveFactionExtension hiveFaction = Faction.def.GetModExtension<HiveFactionExtension>();
			cells.Clear();
			Map map = BaseGen.globalSettings.map;
			IntVec3 CenterCell = rp.rect.CenterCell;
			float dist = rp.rect.TopRight.DistanceTo(rp.rect.BottomLeft);
		//	Log.Message(Faction+" hive radius "+ dist);
			cells = map.AllCells.Where(x=> x.DistanceTo(CenterCell)<= dist).ToList();
			RoofGrid roofGrid = BaseGen.globalSettings.map.roofGrid;
			RoofDef def = rp.roofDef ?? RoofDefOf.RoofRockThick;
			List<IntVec3> cellst = cells.Where(x => x.DistanceTo(CenterCell) < dist - 10).ToList();
			List<IntVec3> celle = cells.Where(x => x.DistanceTo(CenterCell) > dist - 5 && x.DistanceTo(CenterCell) < dist).ToList();
			for (int i = 0; i < dist/5; i++)
			{
				IntVec3 ce = celle.RandomElement();
				Rand.PushState();
				float size = Rand.Range(5, 10);
				Rand.PopState();
				cells.RemoveAll(x=> ce.DistanceTo(x) < size);
			}
			foreach (IntVec3 c in cells)
			{
				roofGrid.SetRoof(c, def);
				map.terrainGrid.SetTerrain(c, TerrainDefOf.Gravel);
				this.TrySpawnWall(c, rp);
			}
		}

		private Thing TrySpawnWall(IntVec3 c, ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{

				if (!thingList[i].def.destroyable)
				{
					return null;
				}
				/*
				if (thingList[i] is Building_Door)
				{
					return null;
				}
				*/
			}
			if (rp.rect.CenterCell.DistanceTo(c)<10)
			{
				return null;
			}
			for (int j = thingList.Count - 1; j >= 0; j--)
			{
				thingList[j].Destroy(DestroyMode.Vanish);
			}

			Rand.PushState();
			bool f = Rand.Chance(rp.chanceToSkipWallBlock.Value);
			Rand.PopState();
			if (rp.chanceToSkipWallBlock != null && f)
			{
				return null;
			}
			
			Thing thing = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("ExtraHive_Hive_Wall"), null);
			thing.SetFaction(rp.faction, null);
			return GenSpawn.Spawn(thing, c, map, WipeMode.Vanish);
		}

	}
}
