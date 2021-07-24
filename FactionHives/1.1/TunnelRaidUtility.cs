
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ExtraHives
{
    // Token: 0x0200070C RID: 1804
    public static class TunnelRaidUtility
    {
        public static string TunnelerArrivalmode()
        {
            string str = string.Empty;
            str = "HE_Tunnelers".Translate();
            return str;
        }

        // Token: 0x06002762 RID: 10082 RVA: 0x0012C458 File Offset: 0x0012A858
        public static void MakeTunnelAt(IntVec3 c, Map map, ActiveDropPodInfo info, Faction faction = null)
        {
            ThingDef TunnelDef = ThingDefOf.Tunneler_ExtraHives;
            //    Log.Message(string.Format("making tunnelSpawner: {0}, @: {1}, {2}, {3}", TunnelDef, c, map, info.innerContainer.ContentsString));
            TunnelRaidSpawner tunnelSpawner = (TunnelRaidSpawner)ThingMaker.MakeThing(TunnelDef, null);
            if (tunnelSpawner.SpawnedFaction == null)
            {
                tunnelSpawner.SpawnedFaction = faction;
                if (tunnelSpawner.SpawnedFaction != null)
                {
                //    Log.Message("tunnelSpawner.Faction set " + tunnelSpawner.SpawnedFaction.Name);
                }
            }
            foreach (Thing item in info.innerContainer)
            {
                tunnelSpawner.GetDirectlyHeldThings().TryAddOrTransfer(item, false);
            }
            GenSpawn.Spawn(tunnelSpawner, c, map);
        }

        // Token: 0x06002763 RID: 10083 RVA: 0x0012C48C File Offset: 0x0012A88C
        public static void DropThingsNear(IntVec3 dropCenter, Map map, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true, Faction faction = null)
        {
            TunnelRaidUtility.tempList.Clear();
            foreach (Thing item in things)
            {
                List<Thing> list = new List<Thing>
                {
                    item
                };
                TunnelRaidUtility.tempList.Add(list);
            }
            TunnelRaidUtility.DropThingGroupsNear(dropCenter, map, TunnelRaidUtility.tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch, faction);
            TunnelRaidUtility.tempList.Clear();
        }

        // Token: 0x06002764 RID: 10084 RVA: 0x0012C518 File Offset: 0x0012A918
        public static void DropThingGroupsNear(IntVec3 dropCenter, Map map, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true, Faction faction = null)
        {
            foreach (List<Thing> list in thingsGroups)
            {
                List<Thing> list2 = list.Where(x => x.def.thingClass == typeof(Pawn)).ToList();
                IntVec3 intVec;
                if (!DropCellFinder.TryFindDropSpotNear(dropCenter, map, out intVec, true, canRoofPunch))
                {
                    Log.Warning(string.Concat(new object[]
                    {
                        "DropThingsNear failed to find a place to drop ",
                        list.FirstOrDefault<Thing>(),
                        " near ",
                        dropCenter,
                        ". Dropping on random square instead."
                    }), false);
                    intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(map) && (c.Roofed(map) && c.GetRoof(map) != RoofDefOf.RoofRockThick), map, 1000);
                }
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SetForbidden(true, false);
                }
                if (instaDrop)
                {
                    foreach (Thing thing in list)
                    {
                        GenPlace.TryPlaceThing(thing, intVec, map, ThingPlaceMode.Near, null, null);
                    }
                }
                else
                {
                    ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
                    foreach (Thing item in list)
                    {
                        activeDropPodInfo.innerContainer.TryAddOrTransfer(item, true);
                    }
                    activeDropPodInfo.openDelay = openDelay;
                    activeDropPodInfo.leaveSlag = leaveSlag;

                    TunnelRaidUtility.MakeTunnelAt(intVec, map, activeDropPodInfo, faction);
                }
            }
        }

        public static void MakeTunnelAt(IntVec3 c, Map map, List<Thing> info)
        {
            ThingDef TunnelDef = DefDatabase<ThingDef>.GetNamed("TunnelerDef");
            TunnelRaidSpawner tunnelSpawner = (TunnelRaidSpawner)ThingMaker.MakeThing(TunnelDef, null);
            foreach (Thing item in info)
            {
                tunnelSpawner.GetDirectlyHeldThings().TryAddOrTransfer(item, false);
            }
            GenSpawn.Spawn(tunnelSpawner, c, map);
        }

        private static List<List<Thing>> tempList = new List<List<Thing>>();
    }
}
