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
    [HarmonyPatch(typeof(IncidentWorker_RaidEnemy), "TryExecuteWorker")]
    public static class IncidentWorker_RaidEnemy_TryExecuteWorker_Patch
    {
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        public static void Prefix(ref IncidentParms parms)
        {
            if (parms.target is Map && (parms.target as Map).IsPlayerHome)
            {
                if (parms.faction != null && (parms.faction.def.HasModExtension<HiveFactionExtension>()))
                {
                    float mult = 1f;
                    int stage = 0;
                    HiveFactionEvolutionTracker evolutionTracker = Find.World.GetComponent<HiveFactionEvolutionTracker>();
                    HiveFactionExtension hive = parms.faction.def.GetModExtension<HiveFactionExtension>();
                    if (evolutionTracker != null)
                    {
                        if (evolutionTracker.HiveFactionStages.TryGetValue(parms.faction.ToString(), out stage))
                        {
                            mult = hive.CurStage.pointMultipler;
                        }
                        else
                        {
                            stage = hive.ActiveStage;
                            evolutionTracker.HiveFactionStages.SetOrAdd(parms.faction.ToString(), stage);
                            mult = hive.CurStage.pointMultipler;
                        }
                    //    Log.Message("IncidentWorker_RaidEnemy HiveFaction Stage: " + stage + " Multiplier: " + mult + " Result: " + (parms.points * mult));
                    }
                    parms.points = parms.points * mult;
                }
            }
        }
    }
}