using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CrashedShipsExtension
{
    // Token: 0x0200034F RID: 847 CrashedShipsExtension.IncidentWorker_CrashedShip
    public class IncidentWorker_CrashedShip : IncidentWorker
    {
        // Token: 0x17000227 RID: 551
        // (get) Token: 0x06000EA6 RID: 3750 RVA: 0x0006C28C File Offset: 0x0006A68C
        protected virtual int CountToSpawn(IncidentParms parms)
        {
            return 1;
            CompProperties_SpawnerOnDamaged props = this.def.mechClusterBuilding?.GetCompProperties<CompProperties_SpawnerOnDamaged>();
            if (props == null)
            {
                return 1;
            }
            Rand.PushState();
            int r = Math.Max(1, Rand.RangeInclusive(1, (int)(props.defaultPoints * parms.points)));
            Rand.PopState();
            return r;
        }

        // Token: 0x06000EA7 RID: 3751 RVA: 0x0006C290 File Offset: 0x0006A690
        public override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            /*
            CompProperties_SpawnerOnDamaged props = this.def.mechClusterBuilding?.GetCompProperties<CompProperties_SpawnerOnDamaged>();
            if (props == null)
            {
                return false;
            }
            if (props)
            {

            }
            */
            return map.listerThings.ThingsOfDef(this.def.mechClusterBuilding).Count <= 0 ;
        }

        // Token: 0x06000EA8 RID: 3752 RVA: 0x0006C2D0 File Offset: 0x0006A6D0
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            
            Map map = (Map)parms.target;
            int num = 0;
            int countToSpawn = this.CountToSpawn(parms);
            List<TargetInfo> list = new List<TargetInfo>();
            Rand.PushState();
            float shrapnelDirection = Rand.Range(0f, 360f);
            Rand.PopState();
            Faction faction = null;
            CompProperties_SpawnerOnDamaged props = this.def.mechClusterBuilding?.GetCompProperties<CompProperties_SpawnerOnDamaged>();
            if (props == null)
            {
                return false;
            }
            /*
            if (faction == null)
            {
                faction = onDamaged.OfFaction;
                if (faction == null)
                {
                    return false;
                }
            }
            */
            for (int i = 0; i < countToSpawn; i++)
            {
                IntVec3 intVec;
                Building CrashedShipPart = (Building)ThingMaker.MakeThing(this.def.mechClusterBuilding, null);
                CompSpawnerOnDamaged onDamaged = CrashedShipPart.TryGetComp<CompSpawnerOnDamaged>();
                ThingDef faller = onDamaged?.Props.skyFaller ?? ThingDefOf.CrashedShipPartIncoming;
                if (!CellFinderLoose.TryFindSkyfallerCell(faller, map, out intVec, 14, default(IntVec3), -1, false, true, true, true, true, false, null))
                {
                    break;
                }
            //    CrashedShipPart.SetFaction(faction, null);
                if (onDamaged != null)
                {
                    onDamaged.pointsLeft = Mathf.Max(parms.points * 0.9f, 300f);
                }

                Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(faller, CrashedShipPart);
                skyfaller.shrapnelDirection = shrapnelDirection;
                Thing thing =
                GenSpawn.Spawn(skyfaller, intVec, map, WipeMode.Vanish);
                num++;
                list.Add(CrashedShipPart);
            }
            if (num > 0)
            {
                base.SendStandardLetter(parms, list, Array.Empty<NamedArgument>());
            }
            return num > 0;
        }

        // Token: 0x0400094F RID: 2383
        private const float ShipPointsFactor = 0.9f;

        // Token: 0x04000950 RID: 2384
        private const int IncidentMinimumPoints = 300;
    }
}
