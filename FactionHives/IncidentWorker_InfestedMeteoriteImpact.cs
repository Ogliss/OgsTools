using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// Token: 0x020009E9 RID: 2537 ExtraHives.IncidentWorker_InfestedMeteoriteImpact
	public class IncidentWorker_InfestedMeteoriteImpact : IncidentWorker
	{
		// Token: 0x06003C69 RID: 15465 RVA: 0x0013F354 File Offset: 0x0013D554
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (def.mechClusterBuilding == null)
			{
				return false;
			}
			if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
			{
				return false;
			}
			ThingDef hiveDef = def.mechClusterBuilding;
			HiveDefExtension hive = hiveDef.GetModExtension<HiveDefExtension>();
			return this.TryFindCell(out intVec, map);
		}

		// Token: 0x06003C6A RID: 15466 RVA: 0x0013F378 File Offset: 0x0013D578
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 intVec;
			if (def.mechClusterBuilding == null)
			{
				return false;
			}
			if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
			{
				return false;
			}
			ThingDef hiveDef = def.mechClusterBuilding;
			HiveDefExtension hive = hiveDef.GetModExtension<HiveDefExtension>();
			if (!this.TryFindCell(out intVec, map))
			{
				return false;
			}
			List<Thing> list = new List<Thing>();

		//	Log.Message("TunnelRaidSpawner");
			TunnelRaidSpawner tunnelHiveSpawner = (TunnelRaidSpawner)ThingMaker.MakeThing(ThingDefOf.Tunneler_ExtraHives, null);
			tunnelHiveSpawner.spawnHive = false;
			tunnelHiveSpawner.initialPoints = Mathf.Max(parms.points * Rand.Range(0.3f, 0.6f), 200f);
			tunnelHiveSpawner.spawnedByInfestationThingComp = true;
			tunnelHiveSpawner.ResultSpawnDelay = new FloatRange(0.1f,0.5f);
			tunnelHiveSpawner.spawnablePawnKinds = hiveDef.GetCompProperties<CompProperties_SpawnerPawn>().spawnablePawnKinds;
			if (tunnelHiveSpawner.Faction == null)
			{
				if (hive.Faction != null)
				{
					tunnelHiveSpawner.Faction = Find.FactionManager.FirstFactionOfDef(hive.Faction);
				}
			}
		//	Log.Message("TunnelRaidSpawner "+ tunnelHiveSpawner.Faction);
			list.Add(tunnelHiveSpawner);
			List<Thing> outThings;
			Generate(out outThings);
			list.AddRange(outThings);
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.InfestedMeteoriteIncoming_ExtraHives, list, intVec, map);
			LetterDef baseLetterDef = list[list.Count-1].def.building.isResourceRock ? LetterDefOf.PositiveEvent : LetterDefOf.NeutralEvent;
			string str = string.Format(this.def.letterText, list[list.Count - 1].def.label).CapitalizeFirst();
			base.SendStandardLetter(this.def.letterLabel + ": " + list[list.Count - 1].def.LabelCap, str, baseLetterDef, parms, new TargetInfo(intVec, map, false), Array.Empty<NamedArgument>());
			return true;
		}

		protected void Generate(out List<Thing> outThings)
		{
			outThings = new List<Thing>();
			int randomInRange = (ThingSetMaker_Meteorite.MineablesCountRange).RandomInRange;
			ThingDef def = RimWorld.ThingDefOf.MineableComponentsIndustrial;
			for (int i = 0; i < randomInRange; i++)
			{
				Building building = (Building)ThingMaker.MakeThing(def, null);
				building.canChangeTerrainOnDestroyed = false;
				outThings.Add(building);
			}
		}
		// Token: 0x06003C6B RID: 15467 RVA: 0x0013F458 File Offset: 0x0013D658
		private bool TryFindCell(out IntVec3 cell, Map map)
		{
			int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max;
			return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.InfestedMeteoriteIncoming_ExtraHives, map, out cell, 10, default(IntVec3), -1, true, false, false, false, true, true, delegate (IntVec3 x)
			{
				int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
				CellRect cellRect = CellRect.CenteredOn(x, num, num);
				int num2 = 0;
				foreach (IntVec3 c in cellRect)
				{
					if (c.InBounds(map) && c.Standable(map))
					{
						num2++;
					}
				}
				return num2 >= maxMineables;
			});
		}
	}
}
