// RimWorld.InfestationCellFinder
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
namespace ExtraHives
{
	public static class InfestationCellFinder
	{
		private struct LocationCandidate
		{
			public IntVec3 cell;

			public float score;

			public LocationCandidate(IntVec3 cell, float score)
			{
				this.cell = cell;
				this.score = score;
			}
		}

		private static List<LocationCandidate> locationCandidates = new List<LocationCandidate>();


		public static bool TryFindCell(out IntVec3 cell, Map map, HiveDefExtension HiveExt)
		{
			CalculateLocationCandidates(map, HiveExt);
			if (!locationCandidates.TryRandomElementByWeight((LocationCandidate x) => x.score, out LocationCandidate result))
			{
				cell = IntVec3.Invalid;
				return false;
			}
			cell = CellFinder.FindNoWipeSpawnLocNear(result.cell, map, RimWorld.ThingDefOf.Hive, Rot4.North, 2, (IntVec3 x) => GetScoreAt(x, map, HiveExt) > 0f && x.GetFirstThing(map, RimWorld.ThingDefOf.Hive) == null && x.GetFirstThing(map, RimWorld.ThingDefOf.TunnelHiveSpawner) == null);
			return true;
		}

		public static bool TryFindCell(out IntVec3 cell, Map map, HivelikeIncidentDef HiveExt)
		{
			CalculateLocationCandidates(map, HiveExt);
			if (!locationCandidates.TryRandomElementByWeight((LocationCandidate x) => x.score, out LocationCandidate result))
			{
				cell = IntVec3.Invalid;
				return false;
			}
			cell = CellFinder.FindNoWipeSpawnLocNear(result.cell, map, RimWorld.ThingDefOf.Hive, Rot4.North, 2, (IntVec3 x) => GetScoreAt(x, map, HiveExt) > 0f && x.GetFirstThing(map, RimWorld.ThingDefOf.Hive) == null && x.GetFirstThing(map, RimWorld.ThingDefOf.TunnelHiveSpawner) == null);
			return true;
		}
		/*
		private static float GetScoreAt(IntVec3 cell, Map map, HiveDefExtension HiveExt)
		{
			float minTemp = HiveExt.minTemp ?? 17f;
			float maxTemp = HiveExt.maxTemp ?? 40f;
			if ((float)(int)distToColonyBuilding[cell] > HiveExt.maxColonyDistance && HiveExt.mustBeNearColony)
			{
				return 0f;
			}
			if ((float)(int)distToColonyBuilding[cell] < HiveExt.minColonyDistance)
			{
				return 0f;
			}
			if (!cell.Walkable(map) && HiveExt.mustBeWalkable)
			{
				return 0f;
			}
			if (cell.Fogged(map) && HiveExt.mustBeVisable)
			{
				return 0f;
			}
			if (CellHasBlockingThings(cell, map))
			{
				return 0f;
			}
            if (HiveExt.requiresRoofed)
            {
                if (!cell.Roofed(map))
				{
					return 0f;
				}
                else
                {
                    if (HiveExt.mustBeThickRoof && !cell.GetRoof(map).isThickRoof)
					{
						return 0f;
					}
                }
            }
			Region region = cell.GetRegion(map);
			if (region == null)
			{
				return 0f;
			}
			if (closedAreaSize[cell] < HiveExt.minClosedAreaSize)
			{
				return 0f;
			}
			float temperature = cell.GetTemperature(map);
			if (temperature < minTemp)
			{
				return 0f;
			}
			if (temperature > maxTemp)
			{
				return 0f;
			}
			float temperatureBonus = 0f;
			if (HiveExt.bonusTempScore!=0)
			{
				if (HiveExt.bonusAboveTemp.HasValue && temperature > HiveExt.bonusAboveTemp.Value)
				{
					temperatureBonus = HiveExt.bonusTempScore;
				}
				if (HiveExt.bonusBelowTemp.HasValue && temperature < HiveExt.bonusBelowTemp.Value)
				{
					temperatureBonus = HiveExt.bonusTempScore;
				}
			}
			float mountainousnessScoreAt = GetMountainousnessScoreAt(cell, map);
			if (HiveExt.minMountainouseness == 0)
			{
				mountainousnessScoreAt = 1f;
			}
            else
            {
				if (mountainousnessScoreAt < HiveExt.minMountainouseness)
				{
					return 0f;
				}
			}
			int num = StraightLineDistToUnroofed(cell, map);
			float value = regionsDistanceToUnroofed.TryGetValue(region, out value) ? Mathf.Min(value, (float)num * 4f) : ((float)num * 1.15f);
			value = Mathf.Pow(value, 1.55f);
			float num2 = Mathf.InverseLerp(0f, 12f, num);
			float num3 = Mathf.Lerp(1f, 0.18f, map.glowGrid.GameGlowAt(cell));
			float num4 = 1f - Mathf.Clamp(DistToBlocker(cell, map) / 11f, 0f, 0.6f);
			float num5 = Mathf.InverseLerp(-17f, -7f, temperature);
			float f = value * num2 * num4 * mountainousnessScoreAt * num3 * num5;
			f = Mathf.Pow(f, 1.2f);
			if (f < 7.5f)
			{
				return 0f;
			}
			return f;
		}
		*/
		private static float GetScoreAt(IntVec3 cell, Map map, HiveDefExtension HiveExt)
        {
			return GetScoreAt(cell, map, HiveExt.minClosedAreaSize, HiveExt.requiresRoofed, HiveExt.mustBeThickRoof, HiveExt.mustBeVisable, HiveExt.mustBeWalkable, HiveExt.mustBeNearColony, HiveExt.maxColonyDistance, HiveExt.minColonyDistance, HiveExt.minMountainouseness, HiveExt.minTemp.Value, HiveExt.maxTemp.Value, HiveExt.bonusTempScore, HiveExt.bonusAboveTemp, HiveExt.bonusBelowTemp);

		}
		private static float GetScoreAt(IntVec3 cell, Map map, HivelikeIncidentDef HiveExt)
        {
			return GetScoreAt(cell, map, HiveExt.minClosedAreaSize, HiveExt.requiresRoofed, HiveExt.mustBeThickRoof, HiveExt.mustBeVisable, HiveExt.mustBeWalkable, HiveExt.mustBeNearColony, HiveExt.maxColonyDistance, HiveExt.minColonyDistance, HiveExt.minMountainouseness, HiveExt.minTemp.Value, HiveExt.maxTemp.Value, HiveExt.bonusTempScore, HiveExt.bonusAboveTemp, HiveExt.bonusBelowTemp);
		}

		private static float GetScoreAt(IntVec3 cell, Map map, int minClosedAreaSize, bool requiresRoofed, bool mustBeThickRoof, bool mustBeVisable, bool mustBeWalkable, bool mustBeNearColony, float maxColonyDistance, float minColonyDistance, float minMountainouseness = 0f, float minTemp = 17f, float maxTemp = 40f, float bonusTempScore = 0f, float? bonusAboveTemp = null, float? bonusBelowTemp = null)
		{
		//	Log.Message("cell "+cell + ", map: " + map + ", minClosedAreaSize: " + minClosedAreaSize + ", requiresRoofed: " + requiresRoofed + ", mustBeThickRoof: " + mustBeThickRoof + ", mustBeVisable: " + mustBeVisable + ", mustBeWalkable: " + mustBeWalkable + ", mustBeNearColony: " + mustBeNearColony + ", maxColonyDistance: " + maxColonyDistance + ", minColonyDistance: " + minColonyDistance + ", minMountainouseness: " + minMountainouseness + ", minTemp: " + minTemp + ", maxTemp: " + maxTemp + ", bonusTempScore: " + bonusTempScore + ", bonusAboveTemp: " + (bonusAboveTemp.HasValue ? bonusAboveTemp.Value.ToString() : "Null") + ", bonusBelowTemp: " + (bonusBelowTemp.HasValue ? bonusBelowTemp.Value.ToString() : "Null"));
			if (mustBeNearColony && (float)(int)RimWorld.InfestationCellFinder.distToColonyBuilding[cell] > maxColonyDistance)
			{
				return 0f;
			}
			if ((float)(int)RimWorld.InfestationCellFinder.distToColonyBuilding[cell] < minColonyDistance)
			{
				return 0f;
			}
			if (!cell.Walkable(map) && mustBeWalkable)
			{
				return 0f;
			}
			if (cell.Fogged(map) && mustBeVisable)
			{
				return 0f;
			}
			if (RimWorld.InfestationCellFinder.CellHasBlockingThings(cell, map))
			{
				return 0f;
			}
			if (requiresRoofed)
            {
                if (!cell.Roofed(map))
				{
					return 0f;
				}
                else
                {
                    if (mustBeThickRoof && !cell.GetRoof(map).isThickRoof)
					{
						return 0f;
					}
                }
			}
			Region region = cell.GetRegion(map);
			if (region == null)
			{
				return 0f;
			}
			if (RimWorld.InfestationCellFinder.closedAreaSize[cell] < minClosedAreaSize)
			{
				return 0f;
			}
			float temperature = cell.GetTemperature(map);
			if (temperature < minTemp)
			{
				return 0f;
			}
			if (temperature > maxTemp)
			{
				return 0f;
			}
			float temperatureBonus = 0f;
			if (bonusTempScore!=0)
			{
				if (bonusAboveTemp.HasValue && temperature > bonusAboveTemp.Value)
				{
					temperatureBonus = bonusTempScore;
				}
				if (bonusBelowTemp.HasValue && temperature < bonusBelowTemp.Value)
				{
					temperatureBonus = bonusTempScore;
				}
			}
			float mountainousnessScoreAt = RimWorld.InfestationCellFinder.GetMountainousnessScoreAt(cell, map);
			if (minMountainouseness == 0)
			{
				mountainousnessScoreAt = 1f;
			}
            else
			{
				if (mountainousnessScoreAt < minMountainouseness)
				{
					return 0f;
				}
			}
			int num = RimWorld.InfestationCellFinder.StraightLineDistToUnroofed(cell, map);
			float value = RimWorld.InfestationCellFinder.regionsDistanceToUnroofed.TryGetValue(region, out value) ? Mathf.Min(value, (float)num * 4f) : ((float)num * 1.15f);
			value = Mathf.Pow(value, 1.55f);
			float num2 = Mathf.InverseLerp(0f, 12f, num);
			float num3 = Mathf.Lerp(1f, 0.18f, map.glowGrid.GameGlowAt(cell));
			float num4 = 1f - Mathf.Clamp(RimWorld.InfestationCellFinder.DistToBlocker(cell, map) / 11f, 0f, 0.6f);
			float num5 = Mathf.InverseLerp(-17f, -7f, temperature);
			float f = value * num2 * num4 * mountainousnessScoreAt * num3 * num5;
			f = Mathf.Pow(f, 1.2f);
			if (f < 7.5f)
			{
				return 0f;
			}
			Log.Message("scoreing");
			return f;
		}

		private static void CalculateLocationCandidates(Map map, HiveDefExtension HiveExt)
		{
			locationCandidates.Clear();
			RimWorld.InfestationCellFinder.CalculateTraversalDistancesToUnroofed(map);
			RimWorld.InfestationCellFinder.CalculateClosedAreaSizeGrid(map);
			RimWorld.InfestationCellFinder.CalculateDistanceToColonyBuildingGrid(map);
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 cell = new IntVec3(j, 0, i);
					float scoreAt = GetScoreAt(cell, map, HiveExt);
					if (!(scoreAt <= 0f))
					{
						locationCandidates.Add(new LocationCandidate(cell, scoreAt));
					}
				}
			}
		}
		
		private static void CalculateLocationCandidates(Map map, HivelikeIncidentDef HiveExt)
		{
			locationCandidates.Clear();
			RimWorld.InfestationCellFinder.CalculateTraversalDistancesToUnroofed(map);
			RimWorld.InfestationCellFinder.CalculateClosedAreaSizeGrid(map);
			RimWorld.InfestationCellFinder.CalculateDistanceToColonyBuildingGrid(map);
			for (int i = 0; i < map.Size.z; i++)
			{
				for (int j = 0; j < map.Size.x; j++)
				{
					IntVec3 cell = new IntVec3(j, 0, i);
					float scoreAt = GetScoreAt(cell, map, HiveExt);
					if (!(scoreAt <= 0f))
					{
						locationCandidates.Add(new LocationCandidate(cell, scoreAt));
					}
				}
			}
		}

	}

}