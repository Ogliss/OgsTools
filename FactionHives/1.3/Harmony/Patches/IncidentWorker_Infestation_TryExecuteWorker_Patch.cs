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
    [HarmonyPatch(typeof(ExtraHives.IncidentWorker_Infestation), "TryExecuteWorker")]
    public static class IncidentWorker_Infestation_TryExecuteWorker_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(ref IncidentParms parms)
        {
            if (parms.target is Map && (parms.target as Map).IsPlayerHome)
            {
                Faction faction = parms.faction;
                HiveFactionEvolutionTracker evolutionTracker = Find.World.GetComponent<HiveFactionEvolutionTracker>();
                HiveFactionExtension hive = faction?.def.GetModExtension<HiveFactionExtension>();
                if (faction != null)
                {
                //    Log.Message("faction not null");
                    if ((parms.target is Map map))
                    {
                    //    Log.Message("target is map");
                        if (evolutionTracker != null && hive != null)
                        {
                        //    Log.Message("evolutionTracker & hive");
                            if (evolutionTracker.HiveFactionStages.TryGetValue(faction.ToString(), out int stage))
                            {
                                float mult = hive.CurStage.pointMultipler;
                            //    Log.Message("IncidentWorker_RaidEnemy HiveFaction Stage: " + stage + " Multiplier: " + mult + " Result: " + (parms.points * mult));
                                parms.points *= mult;

                            }
                        }
                    }
                }
            }
        }
        private static readonly IntRange RaidDelay = new IntRange(1000, 2000);
    }
}