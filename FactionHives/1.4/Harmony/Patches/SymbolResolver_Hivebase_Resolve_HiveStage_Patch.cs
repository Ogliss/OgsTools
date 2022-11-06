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
    
    [HarmonyPatch(typeof(GenStuff.SymbolResolver_Hivebase), "Resolve")]
    public static class SymbolResolver_Hivebase_Resolve_HiveStage_Patch
    {
        public static void Postfix(ExtraHives.GenStuff.SymbolResolver_Hivebase __instance, ref ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            if (rp.faction.def.HasModExtension<HiveFactionExtension>())
            {
                float mult = 1f;

                HiveFactionEvolutionTracker evolutionTracker = Find.World.GetComponent<HiveFactionEvolutionTracker>();
                HiveFactionExtension hive = rp.faction.def.GetModExtension<HiveFactionExtension>();
                if (evolutionTracker != null)
                {
                    if (evolutionTracker.HiveFactionStages.TryGetValue(rp.faction.ToString(), out int stage))
                    {
                        mult = hive.CurStage.pointMultipler;
                    //    Log.Message("SymbolResolver_Hivebase HiveFaction Stage: " + stage + " Multiplier: " + mult + " Result: " + ((rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange) * mult));
                    }
                }
                /*
                rp.rect.Width = (int)(rp.rect.Width / sizemod);
                rp.rect.Height = (int)(rp.rect.Height / sizemod);
                */
                if (rp.pawnGroupMakerParams != null)
                {
                    rp.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange) * mult;
                }
                else
                {
                    rp.pawnGroupMakerParams = new PawnGroupMakerParms();
                    rp.pawnGroupMakerParams.tile = map.Tile;
                    rp.pawnGroupMakerParams.faction = rp.faction;
                    rp.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange) * mult;
                    rp.pawnGroupMakerParams.inhabitants = true;
                    rp.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
                }
            }
        }
    }
    
} 