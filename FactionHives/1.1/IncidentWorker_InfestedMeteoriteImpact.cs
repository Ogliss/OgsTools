using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// Token: 0x020009E9 RID: 2537 ExtraHives.IncidentWorker_InfestedMeteoriteImpact
	public class IncidentWorker_InfestedMeteoriteImpact : IncidentWorker
	{
		// Token: 0x06003C69 RID: 15465 RVA: 0x0013F354 File Offset: 0x0013D554
		public override bool CanFireNowSub(IncidentParms parms)
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
		public override bool TryExecuteWorker(IncidentParms parms)
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
			Faction faction = null;
			if (parms.faction!=null)
			{
				faction = parms.faction;
			}
			else
			{
				if (hive.Faction != null)
				{
					IEnumerable<Faction> factions = CandidateFactions(map, hive.Faction.defName);
					if (!factions.EnumerableNullOrEmpty())
					{
						faction = factions.RandomElement();
					}
					else
					{
						faction = Find.FactionManager.FirstFactionOfDef(hive.Faction);
					}
				}
				else
				{
					return false;
				}

			}
			if (!this.TryFindCell(out intVec, map))
			{
				return false;
			}
			List<Thing> list = new List<Thing>();

		//	Log.Message("TunnelRaidSpawner");
			TunnelRaidSpawner tunnelHiveSpawner = (TunnelRaidSpawner)ThingMaker.MakeThing(ThingDefOf.Tunneler_ExtraHives, null);
			tunnelHiveSpawner.spawnHive = false;
			Rand.PushState();
			tunnelHiveSpawner.initialPoints = Mathf.Max(parms.points * Rand.Range(0.3f, 0.6f), 200f);
			Rand.PopState();
			tunnelHiveSpawner.spawnedByInfestationThingComp = true;
			tunnelHiveSpawner.ResultSpawnDelay = new FloatRange(0.1f,0.5f);
			tunnelHiveSpawner.spawnablePawnKinds = faction.def.pawnGroupMakers.Where(x=> x.kindDef == RimWorld.PawnGroupKindDefOf.Combat || x.kindDef == PawnGroupKindDefOf.Tunneler_ExtraHives).RandomElement().options;
			if (tunnelHiveSpawner.SpawnedFaction == null)
			{
				if (faction != null)
				{
				//	Log.Message(faction.Name);
					tunnelHiveSpawner.SpawnedFaction = faction;
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

		protected IEnumerable<Faction> CandidateFactions(Map map, string Contains, bool desperate = false)
		{
			return from f in Find.FactionManager.AllFactions
				   where this.FactionCanBeGroupSource(f, map, desperate) && (Contains.NullOrEmpty() || (f.def.defName.Contains(Contains)))
				   select f;
		}
		protected virtual bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			bool result = !f.IsPlayer && !f.defeated && !f.temporary && (desperate || (f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) && f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp) && (float)GenDate.DaysPassed >= f.def.earliestRaidDays));
		//	Log.Message(f.Name+" Allow: "+result);
			return result;
		}
	}
}
