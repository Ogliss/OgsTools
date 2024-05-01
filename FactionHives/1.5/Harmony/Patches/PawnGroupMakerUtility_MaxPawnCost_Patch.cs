using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace ExtraHives.HarmonyInstance
{
    [HarmonyPatch(typeof(PawnGroupMakerUtility), "MaxPawnCost")]
    public static class PawnGroupMakerUtility_MaxPawnCost_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(Faction faction, float totalPoints, RaidStrategyDef raidStrategy, PawnGroupKindDef groupKind, ref float __result)
        {
            HiveFactionEvolutionTracker evolutionTracker = Find.World.GetComponent<HiveFactionEvolutionTracker>();
            HiveFactionExtension hive = faction.def.GetModExtension<HiveFactionExtension>();
            if (faction != null)
            {
                if (evolutionTracker != null && hive != null)
                {
                    if (evolutionTracker.HiveFactionStages.TryGetValue(faction.ToString(), out int stage))
                    {
                        SimpleCurve curves = hive.CurStage.maxPawnCostPerTotalPointsCurve ?? faction.def.maxPawnCostPerTotalPointsCurve;
                        float num = curves.Evaluate(totalPoints);
                        if (raidStrategy != null)
                        {
                            num = Mathf.Min(num, totalPoints / raidStrategy.minPawns);
                        }
                        num = Mathf.Max(num, faction.def.MinPointsToGeneratePawnGroup(groupKind) * 1.2f);
                        if (raidStrategy != null)
                        {
                            num = Mathf.Max(num, raidStrategy.Worker.MinMaxAllowedPawnGenOptionCost(faction, groupKind) * 1.2f);
                        }
                        __result = num;
                        return false;
                    }
                }
            }
            return true;

        }
    }
}