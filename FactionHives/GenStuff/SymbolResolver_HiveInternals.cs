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
	// Token: 0x020010B0 RID: 4272 ExtraHives.GenStuff.SymbolResolver_HiveInterals
	public class SymbolResolver_HiveInternals : SymbolResolver
	{
		// Token: 0x0600650C RID: 25868 RVA: 0x00233C80 File Offset: 0x00231E80
		public override void Resolve(ResolveParams rp)
		{
			float dist = Math.Min(rp.rect.TopRight.DistanceTo(rp.rect.BottomLeft), GenRadial.MaxRadialPatternRadius - 2);
			List<IntVec3> cells = GenRadial.RadialCellsAround(rp.rect.CenterCell, dist, true).ToList();
			TrySpawnCave(rp.rect.CenterCell, cells, rp);
		}

		// Token: 0x0600650D RID: 25869 RVA: 0x00233CF0 File Offset: 0x00231EF0
		private void TrySpawnCave(IntVec3 c, List<IntVec3> cells, ResolveParams rp)
		{
		//	Log.Message("checking " + cells.Count + " cells for tunnels");
			Map map = BaseGen.globalSettings.map;
			directionNoise = new Perlin(0.0020500000100582838, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			BoolGrid visited = new BoolGrid(map);
			List<IntVec3> group = new List<IntVec3>();
			foreach (IntVec3 allCell in cells)
			{
				if (!visited[allCell] && allCell.Filled(map))
				{
				//	Log.Message("checking " + allCell + " cells for tunnel");
					group.Clear();
					map.floodFiller.FloodFill(allCell, (IntVec3 x) => x.Filled(map), delegate (IntVec3 x)
					{
						visited[x] = true;
						group.Add(x);
					});
				//	Log.Message("found " + group.Count + " cells for tunnel group");
					/*	
						Trim(group, map);
					//	Log.Message("Trim group " + group.Count + " cells left");
						RemoveSmallDisconnectedSubGroups(group, map);
					//	Log.Message("RemoveSmallDisconnectedSubGroups from group " + group.Count + " cells left");
					*/
					if (group.Count >= 10)
					{
					//	Log.Message("making " + group.Count + " tunnels");
						DoOpenTunnels(group, map);
						DoClosedTunnels(group, map);
					}
				}
			}
			foreach (IntVec3 allCell in cells)
			{

			}
		}
		private void Trim(List<IntVec3> group, Map map)
		{
			GenMorphology.Open(group, 6, map);
		}

		private bool IsRock(IntVec3 c, Map map)
		{
			if (c.InBounds(map))
			{
				return c.Filled(map);
			}
			return false;
		}

		private void DoOpenTunnels(List<IntVec3> group, Map map)
		{
			int a = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * 5.8f / 100f);
			a = Mathf.Min(a, 3);
			if (a > 0)
			{
				a = Rand.RangeInclusive(1, a);
			}
		//	Log.Message("DoOpenTunnels Dig " + group.Count + " tunnels attempts " + a);
			float num = TunnelsWidthPerRockCount.Evaluate(group.Count);
			for (int i = 0; i < a; i++)
			{
				IntVec3 start = IntVec3.Invalid;
				float num2 = -1f;
				float dir = -1f;
				float num3 = -1f;
				for (int j = 0; j < 10; j++)
				{
					IntVec3 intVec = FindRandomEdgeCellForTunnel(group, map);
					float distToCave = GetDistToCave(intVec, group, map, 40f, treatOpenSpaceAsCave: false);
					float dist;
					float num4 = FindBestInitialDir(intVec, group, out dist);
					if (!start.IsValid || distToCave > num2 || (distToCave == num2 && dist > num3))
					{
						start = intVec;
						num2 = distToCave;
						dir = num4;
						num3 = dist;
					}
				}
				float width = Rand.Range(num * 0.8f, num);
			//	Log.Message("DoOpenTunnels Dig " + group.Count + " tunnels");
				Dig(start, dir, width, group, map, closed: false);
			}
		}

		private void DoClosedTunnels(List<IntVec3> group, Map map)
		{
			int a = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * 2.5f / 100f);
			a = Mathf.Min(a, 1);
			if (a > 0)
			{
				a = Rand.RangeInclusive(0, a);
			}
		//	Log.Message("DoClosedTunnels " + group.Count + " tunnels attempts " + a);
			float num = TunnelsWidthPerRockCount.Evaluate(group.Count);
			for (int i = 0; i < a; i++)
			{
				IntVec3 start = IntVec3.Invalid;
				float num2 = -1f;
				for (int j = 0; j < 7; j++)
				{
					IntVec3 intVec = group.RandomElement();
					float distToCave = GetDistToCave(intVec, group, map, 30f, treatOpenSpaceAsCave: true);
					if (!start.IsValid || distToCave > num2)
					{
						start = intVec;
						num2 = distToCave;
					}
				}
				float width = Rand.Range(num * 0.8f, num);
			//	Log.Message("DoClosedTunnels Dig " + group.Count + " tunnels");
				Dig(start, Rand.Range(0f, 360f), width, group, map, closed: true);
			}
		}

		private IntVec3 FindRandomEdgeCellForTunnel(List<IntVec3> group, Map map)
		{
			MapGenFloatGrid caves = MapGenerator.Caves;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			tmpCells.Clear();
			tmpGroupSet.Clear();
			tmpGroupSet.AddRange(group);
			for (int i = 0; i < group.Count; i++)
			{
				if (group[i].DistanceToEdge(map) < 3 || caves[group[i]] > 0f)
				{
					continue;
				}
				for (int j = 0; j < 4; j++)
				{
					IntVec3 item = group[i] + cardinalDirections[j];
					if (!tmpGroupSet.Contains(item))
					{
						tmpCells.Add(group[i]);
						break;
					}
				}
			}
			if (!tmpCells.Any())
			{
				Log.Warning("Could not find any valid edge cell.");
				return group.RandomElement();
			}
			return tmpCells.RandomElement();
		}

		private float FindBestInitialDir(IntVec3 start, List<IntVec3> group, out float dist)
		{
			float num = GetDistToNonRock(start, group, IntVec3.East, 40);
			float num2 = GetDistToNonRock(start, group, IntVec3.West, 40);
			float num3 = GetDistToNonRock(start, group, IntVec3.South, 40);
			float num4 = GetDistToNonRock(start, group, IntVec3.North, 40);
			float num5 = GetDistToNonRock(start, group, IntVec3.NorthWest, 40);
			float num6 = GetDistToNonRock(start, group, IntVec3.NorthEast, 40);
			float num7 = GetDistToNonRock(start, group, IntVec3.SouthWest, 40);
			float num8 = GetDistToNonRock(start, group, IntVec3.SouthEast, 40);
			dist = Mathf.Max(num, num2, num3, num4, num5, num6, num7, num8);
			return GenMath.MaxByRandomIfEqual(0f, num + num8 / 2f + num6 / 2f, 45f, num8 + num3 / 2f + num / 2f, 90f, num3 + num8 / 2f + num7 / 2f, 135f, num7 + num3 / 2f + num2 / 2f, 180f, num2 + num7 / 2f + num5 / 2f, 225f, num5 + num4 / 2f + num2 / 2f, 270f, num4 + num6 / 2f + num5 / 2f, 315f, num6 + num4 / 2f + num / 2f);
		}

		private void Dig(IntVec3 start, float dir, float width, List<IntVec3> group, Map map, bool closed, HashSet<IntVec3> visited = null)
		{
		//	Log.Message("Dig ");
			Vector3 vect = start.ToVector3Shifted();
			IntVec3 intVec = start;
			float num = 0f;
			MapGenFloatGrid caves = MapGenerator.Caves;
			bool flag = false;
			bool flag2 = false;
			if (visited == null)
			{
				visited = new HashSet<IntVec3>();
			}
			tmpGroupSet.Clear();
			tmpGroupSet.AddRange(group);
			int num2 = 0;
			while (true)
			{
				if (closed)
				{
				//	Log.Message("Closed ");
					int num3 = GenRadial.NumCellsInRadius(width / 2f + 1.5f);
					for (int i = 0; i < num3; i++)
					{
						IntVec3 intVec2 = intVec + GenRadial.RadialPattern[i];
						if (!visited.Contains(intVec2) && (!tmpGroupSet.Contains(intVec2) || caves[intVec2] > 0f))
						{
						//	Log.Message("closed failed");
							return;
						}
					}
				}
				if (num2 >= 15 && width > 1.4f + BranchedTunnelWidthOffset.max)
				{
				//	Log.Message("Dig Can Branch");
					if (!flag && Rand.Chance(0.1f))
					{
					//	Log.Message("Branch DigInBestDirection("+intVec + ", " + dir + ", FloatRange(40f, 90f), "+ (width - BranchedTunnelWidthOffset.RandomInRange) + ", "+ group + ", "+ map + ", "+ closed + ", " + visited+")");
						DigInBestDirection(intVec, dir, new FloatRange(40f, 90f), width - BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
						flag = true;
					}
					if (!flag2 && Rand.Chance(0.1f))
					{
					//	Log.Message("Branch DigInBestDirection(" + intVec + ", " + dir + ", FloatRange(-90f, -40f), " + (width - BranchedTunnelWidthOffset.RandomInRange) + ", " + group + ", " + map + ", " + closed + ", " + visited + ")");
						DigInBestDirection(intVec, dir, new FloatRange(-90f, -40f), width - BranchedTunnelWidthOffset.RandomInRange, group, map, closed, visited);
						flag2 = true;
					}
				}
				SetCaveAround(intVec, width, map, visited, out bool hitAnotherTunnel);
			//	Log.Message("Dig SetCaveAround(" + intVec + ", " + width + ", " + visited + ", Out HitAnotherTunnel " + hitAnotherTunnel);
				if (hitAnotherTunnel)
				{
					break;
				}
				while (vect.ToIntVec3() == intVec)
				{
					vect += Vector3Utility.FromAngleFlat(dir) * 0.5f;
					num += 0.5f;
				}
				if (!tmpGroupSet.Contains(vect.ToIntVec3()))
				{
					break;
				}
				IntVec3 intVec3 = new IntVec3(intVec.x, 0, vect.ToIntVec3().z);
				if (IsRock(intVec3, map))
				{
					caves[intVec3] = Mathf.Max(caves[intVec3], width);
					visited.Add(intVec3);
				}
				intVec = vect.ToIntVec3();
				dir += (float)directionNoise.GetValue(num * 60f, (float)start.x * 200f, (float)start.z * 200f) * 8f;
				width -= 0.034f;
				if (!(width < 1.4f))
				{
					num2++;
					continue;
				}
				break;
			}
		}

		private void DigInBestDirection(IntVec3 curIntVec, float curDir, FloatRange dirOffset, float width, List<IntVec3> group, Map map, bool closed, HashSet<IntVec3> visited = null)
		{
			int num = -1;
			float dir = -1f;
			for (int i = 0; i < 6; i++)
			{
				float num2 = curDir + dirOffset.RandomInRange;
				int distToNonRock = GetDistToNonRock(curIntVec, group, num2, 50);
				if (distToNonRock > num)
				{
					num = distToNonRock;
					dir = num2;
				}
			}
			if (num >= 18)
			{
				Dig(curIntVec, dir, width, group, map, closed, visited);
			}
		}

		private void SetCaveAround(IntVec3 around, float tunnelWidth, Map map, HashSet<IntVec3> visited, out bool hitAnotherTunnel)
		{
			hitAnotherTunnel = false;
			int num = GenRadial.NumCellsInRadius(tunnelWidth / 2f);
			MapGenFloatGrid caves = MapGenerator.Caves;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = around + GenRadial.RadialPattern[i];
			//	Log.Message("SetCaveAround checking " + intVec);
				if (IsRock(intVec, map))
				{
				//	Log.Message("SetCaveAround using " + intVec);
					if (caves[intVec] > 0f && !visited.Contains(intVec))
					{
					//	Log.Message("SetCaveAround hitAnotherTunnel");
						hitAnotherTunnel = true;
					}
					caves[intVec] = Mathf.Max(caves[intVec], tunnelWidth);
					visited.Add(intVec);
				}
			}
		}

		private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, IntVec3 offset, int maxDist)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			for (int i = 0; i <= maxDist; i++)
			{
				IntVec3 item = from + offset * i;
				if (!groupSet.Contains(item))
				{
					return i;
				}
			}
			return maxDist;
		}

		private int GetDistToNonRock(IntVec3 from, List<IntVec3> group, float dir, int maxDist)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			Vector3 a = Vector3Utility.FromAngleFlat(dir);
			for (int i = 0; i <= maxDist; i++)
			{
				IntVec3 item = (from.ToVector3Shifted() + a * i).ToIntVec3();
				if (!groupSet.Contains(item))
				{
					return i;
				}
			}
			return maxDist;
		}

		private float GetDistToCave(IntVec3 cell, List<IntVec3> group, Map map, float maxDist, bool treatOpenSpaceAsCave)
		{
			MapGenFloatGrid caves = MapGenerator.Caves;
			tmpGroupSet.Clear();
			tmpGroupSet.AddRange(group);
			int num = GenRadial.NumCellsInRadius(maxDist);
			IntVec3[] radialPattern = GenRadial.RadialPattern;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = cell + radialPattern[i];
				if ((treatOpenSpaceAsCave && !tmpGroupSet.Contains(intVec)) || (intVec.InBounds(map) && caves[intVec] > 0f))
				{
					return cell.DistanceTo(intVec);
				}
			}
			return maxDist;
		}

		private void RemoveSmallDisconnectedSubGroups(List<IntVec3> group, Map map)
		{
			groupSet.Clear();
			groupSet.AddRange(group);
			groupVisited.Clear();
			for (int i = 0; i < group.Count; i++)
			{
				if (groupVisited.Contains(group[i]) || !groupSet.Contains(group[i]))
				{
					continue;
				}
				subGroup.Clear();
				map.floodFiller.FloodFill(group[i], (IntVec3 x) => groupSet.Contains(x), delegate (IntVec3 x)
				{
					subGroup.Add(x);
					groupVisited.Add(x);
				});
				if (subGroup.Count < 300 || (float)subGroup.Count < 0.05f * (float)group.Count)
				{
					for (int j = 0; j < subGroup.Count; j++)
					{
						groupSet.Remove(subGroup[j]);
					}
				}
			}
			group.Clear();
			group.AddRange(groupSet);
		}
		private ModuleBase directionNoise;

		private static HashSet<IntVec3> tmpGroupSet = new HashSet<IntVec3>();

		private const float OpenTunnelsPer10k = 5.8f;

		private const float ClosedTunnelsPer10k = 2.5f;

		private const int MaxOpenTunnelsPerRockGroup = 3;

		private const int MaxClosedTunnelsPerRockGroup = 1;

		private const float DirectionChangeSpeed = 8f;

		private const float DirectionNoiseFrequency = 0.00205f;

		private const int MinRocksToGenerateAnyTunnel = 300;

		private const int AllowBranchingAfterThisManyCells = 15;

		private const float MinTunnelWidth = 1.4f;

		private const float WidthOffsetPerCell = 0.034f;

		private const float BranchChance = 0.1f;

		private static readonly FloatRange BranchedTunnelWidthOffset = new FloatRange(0.2f, 0.4f);

		private static readonly SimpleCurve TunnelsWidthPerRockCount = new SimpleCurve
		{
			new CurvePoint(100f, 2f),
			new CurvePoint(300f, 4f),
			new CurvePoint(3000f, 5.5f)
		};

		private static List<IntVec3> tmpCells = new List<IntVec3>();

		private static HashSet<IntVec3> groupSet = new HashSet<IntVec3>();

		private static HashSet<IntVec3> groupVisited = new HashSet<IntVec3>();

		private static List<IntVec3> subGroup = new List<IntVec3>();

		public int SeedPart => 647814558;
	}
}
