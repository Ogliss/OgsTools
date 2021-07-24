using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// Token: 0x02000B9B RID: 2971
	public static class PawnsArrivalModeWorkerUtility
	{
		// Token: 0x060045EC RID: 17900 RVA: 0x0017880C File Offset: 0x00176A0C
		public static void PlaceInTunnelsNearSpawnCenter(IncidentParms parms, List<Pawn> pawns)
		{
			Map map = (Map)parms.target;
			bool flag = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
			TunnelRaidUtility.DropThingsNear(parms.spawnCenter, map, pawns.Cast<Thing>(), parms.podOpenDelay, false, true, flag || parms.raidArrivalModeForQuickMilitaryAid, parms.faction);
		}

		// Token: 0x060045ED RID: 17901 RVA: 0x00178868 File Offset: 0x00176A68
		public static List<Pair<List<Pawn>, IntVec3>> SplitIntoRandomGroupsNearMapEdge(List<Pawn> pawns, Map map, bool arriveInPods)
		{
			List<Pair<List<Pawn>, IntVec3>> list = new List<Pair<List<Pawn>, IntVec3>>();
			if (!pawns.Any<Pawn>())
			{
				return list;
			}
			int maxGroupsCount = PawnsArrivalModeWorkerUtility.GetMaxGroupsCount(pawns.Count);
			Rand.PushState();
			int num = (maxGroupsCount == 1) ? 1 : Rand.RangeInclusive(2, maxGroupsCount);
			Rand.PopState();
			for (int i = 0; i < num; i++)
			{
				IntVec3 second = PawnsArrivalModeWorkerUtility.FindNewMapEdgeGroupCenter(map, list, arriveInPods);
				list.Add(new Pair<List<Pawn>, IntVec3>(new List<Pawn>(), second)
				{
					First =
					{
						pawns[i]
					}
				});
			}
			for (int j = num; j < pawns.Count; j++)
			{
				list.RandomElement<Pair<List<Pawn>, IntVec3>>().First.Add(pawns[j]);
			}
			return list;
		}

		// Token: 0x060045EE RID: 17902 RVA: 0x00178914 File Offset: 0x00176B14
		private static IntVec3 FindNewMapEdgeGroupCenter(Map map, List<Pair<List<Pawn>, IntVec3>> groups, bool arriveInPods)
		{
			IntVec3 result = IntVec3.Invalid;
			float num = 0f;
			for (int i = 0; i < 4; i++)
			{
				IntVec3 intVec;
				if (arriveInPods)
				{
					intVec = DropCellFinder.FindRaidDropCenterDistant(map, false);
				}
				else if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec, map, CellFinder.EdgeRoadChance_Hostile, false, null))
				{
					intVec = DropCellFinder.FindRaidDropCenterDistant(map, false);
				}
				if (!groups.Any<Pair<List<Pawn>, IntVec3>>())
				{
					result = intVec;
					break;
				}
				float num2 = float.MaxValue;
				for (int j = 0; j < groups.Count; j++)
				{
					float num3 = (float)intVec.DistanceToSquared(groups[j].Second);
					if (num3 < num2)
					{
						num2 = num3;
					}
				}
				if (!result.IsValid || num2 > num)
				{
					num = num2;
					result = intVec;
				}
			}
			return result;
		}

		// Token: 0x060045EF RID: 17903 RVA: 0x001789C5 File Offset: 0x00176BC5
		private static int GetMaxGroupsCount(int pawnsCount)
		{
			if (pawnsCount <= 1)
			{
				return 1;
			}
			return Mathf.Clamp(pawnsCount / 2, 2, 3);
		}

		// Token: 0x060045F0 RID: 17904 RVA: 0x001789D8 File Offset: 0x00176BD8
		public static void SetPawnGroupsInfo(IncidentParms parms, List<Pair<List<Pawn>, IntVec3>> groups)
		{
			parms.pawnGroups = new Dictionary<Pawn, int>();
			for (int i = 0; i < groups.Count; i++)
			{
				for (int j = 0; j < groups[i].First.Count; j++)
				{
					parms.pawnGroups.Add(groups[i].First[j], i);
				}
			}
		}

		// Token: 0x0400287B RID: 10363
		private const int MaxGroupsCount = 3;
	}
}
