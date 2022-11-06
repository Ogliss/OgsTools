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
using RimWorld.BaseGen;

namespace ExtraHives.HarmonyInstance
{
    
    [HarmonyPatch(typeof(GenStuff.SymbolResolver_PawnHiveGroup), "Resolve")]
    public static class SymbolResolver_PawnHiveGroup_Resolve_HiveStage_Patch
    {
        public static void Prefix(ref ResolveParams rp)
        {
            if (rp.faction!=null)
            {
                Faction faction = rp.faction;
                HiveFactionEvolutionTracker evolutionTracker = Find.World.GetComponent<HiveFactionEvolutionTracker>();
                HiveFactionExtension hive = faction.def.GetModExtension<HiveFactionExtension>();
                if (evolutionTracker != null && hive != null)
                {
                    if (evolutionTracker.HiveFactionStages.TryGetValue(faction.ToString(), out int stage))
                    {
                        float mult = hive.CurStage.pointMultipler;
                        if (rp.pawnGroupMakerParams != null)
                        {
                        //    Log.Message("SymbolResolver_PawnHiveGroup HiveFaction Stage: " + stage + " Multiplier: "+ mult+" Result: "+ (rp.pawnGroupMakerParams.points * mult));
                            rp.pawnGroupMakerParams.points *= mult;
                        }
                    }
                }
            }
        }
    }
    
}