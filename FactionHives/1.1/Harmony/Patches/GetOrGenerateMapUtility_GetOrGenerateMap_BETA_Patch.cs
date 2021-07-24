using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ExtraHives.HarmonyInstance
{

    [HarmonyPatch(typeof(GetOrGenerateMapUtility), "GetOrGenerateMap", new[] { typeof(int), typeof(IntVec3), typeof(WorldObjectDef) })]
    public static class GetOrGenerateMapUtility_GetOrGenerateMap_MapSize_Patch
    {
        public static void Prefix(int tile, ref IntVec3 size, WorldObjectDef suggestedMapParentDef)
        {
            Map map = Current.Game.FindMap(tile);
            Faction faction = null;
            if (map != null)
            {
                if (map.ParentFaction != null)
                {
                    faction = map.ParentFaction;
                }
            }
            else
            {
                MapParent mapParent = Find.WorldObjects.MapParentAt(tile);
                if (mapParent?.Faction != null)
                {
                    faction = mapParent.Faction;
                }
            }
            if (faction == null)
            {
                return;
            }

            HiveFactionEvolutionTracker evolutionTracker = Find.World.GetComponent<HiveFactionEvolutionTracker>();
            HiveFactionExtension hive = faction.def.GetModExtension<HiveFactionExtension>();
            if (evolutionTracker != null && hive != null)
            {
                int sizemod = 0;
            //    Log.Message("GetOrGenerateMap for " + faction + " Default: " + size.x + " Max Hive size: " + hive.CurStage.sizeRange.max);
                for (int i = 0; i < 10; i++)
                {
                    if (hive.CurStage.sizeRange.max >= ((size.x + sizemod) / 2) * 0.75f)
                    {
                        sizemod = (int)(size.x * 0.25f);
                    }
                    else
                    {
                        break;
                    }
                }
                if (sizemod > 0)
                {
                    size.x += sizemod;
                    size.z += sizemod;
                //    Log.Message("GetOrGenerateMap for " + faction +" applied sizemod: "+ sizemod +" resulting: "+ size);
                }
            }
            else
            {
            //    Log.Message("GetOrGenerateMap evolutionTracker: "+ (evolutionTracker != null) + " hiveext: " + (hive != null));
            }
        }
    }

}