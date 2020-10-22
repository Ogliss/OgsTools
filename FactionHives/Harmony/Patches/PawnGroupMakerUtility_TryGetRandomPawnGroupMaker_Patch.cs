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
    [HarmonyPatch(typeof(PawnGroupMakerUtility), "TryGetRandomPawnGroupMaker")]
    public static class PawnGroupMakerUtility_TryGetRandomPawnGroupMaker_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(PawnGroupMakerParms parms, ref PawnGroupMaker pawnGroupMaker)
        {
            Faction faction = parms.faction;
            HiveFactionEvolutionTracker evolutionTracker = Find.World.GetComponent<HiveFactionEvolutionTracker>();
            if (faction != null)
            {
                HiveFactionExtension hive = faction.def.GetModExtension<HiveFactionExtension>();
                if (evolutionTracker != null && hive != null)
                {
                    if (evolutionTracker.HiveFactionStages.TryGetValue(faction.ToString(), out int stage))
                    {
                        if (parms.seed != null)
                        {
                            Rand.PushState(parms.seed.Value);
                        }
                        if (!hive.CurStage.pawnGroupMakers.NullOrEmpty())
                        {
                            string li = string.Empty;
                        //    Log.Message("TryGetRandomPawnGroupMaker HiveFaction using pawnGroupMaker from Stage: " + stage);
                        }
                        bool result = (from gm in hive.CurStage.pawnGroupMakers ?? parms.faction.def.pawnGroupMakers
                                       where gm.kindDef == parms.groupKind && gm.CanGenerateFrom(parms)
                                       select gm).TryRandomElementByWeight((PawnGroupMaker gm) => gm.commonality, out pawnGroupMaker);
                        if (parms.seed != null)
                        {
                            Rand.PopState();
                        }
                    //    Log.Message("TryGetRandomPawnGroupMaker HiveFaction Stage: " + stage + " pawnGroupMaker: " + pawnGroupMaker.kindDef);
                    }
                }
            }
        }
    }
}