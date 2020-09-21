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
using ExtraHives.ExtensionMethods;

namespace ExtraHives.HarmonyInstance
{
    
    [HarmonyPatch(typeof(GenStep_Settlement), "ScatterAt")]
	public static class GenStep_Settlement_ScatterAt_ExtraHives_Patch
    {
		[HarmonyPrefix, HarmonyPriority(Priority.Last)]
        public static bool Prefix(IntVec3 c, Map map,ref GenStepParams parms, int stackCount = 1)
		{
			bool result;
			try
			{
				if (map.ParentFaction != null && map.ParentFaction.HiveExt() != null)
				{
					HiveFactionExtension HFExt = map.ParentFaction.HiveExt();
					if (HFExt.overrideBaseGen)
					{
						IntRange intRange = HFExt.CurStage.sizeRange;
						int randomInRange = intRange.RandomInRange;
						int randomInRange2 = intRange.RandomInRange;
						CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
						rect.ClipInsideMap(map);
						ResolveParams resolveParams = default(ResolveParams);
						resolveParams.rect = rect;
						resolveParams.faction = map.ParentFaction;
						resolveParams.cultivatedPlantDef = HFExt.cultivatedPlantDef ?? RimWorld.ThingDefOf.Plant_Grass;
						/*
						resolveParams.pathwayFloorDef = DefDatabase<TerrainDef>.AllDefsListForReading.FindAll((TerrainDef t) => t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Medium && t.costStuffCount < 6).RandomElement<TerrainDef>();
						resolveParams.wallStuff = DefDatabase<ThingDef>.AllDefsListForReading.FindAll((ThingDef t) => t.stuffProps != null && (t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Light || t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Medium || t.terrainAffordanceNeeded == TerrainAffordanceDefOf.Heavy) && t.BaseMarketValue < 6f).RandomElement<ThingDef>();
						*/
						BaseGen.globalSettings.map = map;
						BaseGen.globalSettings.minBuildings = 8;
						BaseGen.globalSettings.minBarracks = 2;
						BaseGen.symbolStack.Push(HFExt.baseGenOverride.NullOrEmpty() ? "ExtraHives_HiveBaseMaker" : HFExt.baseGenOverride, resolveParams, null);
						BaseGen.Generate();
						if (HFExt.baseDamage)
						{
							BaseGen.globalSettings.map = map;
							BaseGen.symbolStack.Push("ExtraHives_HiveRandomDamage", resolveParams, null);
							BaseGen.Generate();
						}
						if (HFExt.randomHives)
						{
							BaseGen.globalSettings.map = map;
							BaseGen.symbolStack.Push("ExtraHives_HiveRandomHives", resolveParams, null);
							BaseGen.Generate();
						}
						result = false;
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = true;
				}
			}
			catch (Exception ex)
			{
			//	Log.Message(ex.Message, false);
				result = true;
			}
			return result;
		}
    }
    
}