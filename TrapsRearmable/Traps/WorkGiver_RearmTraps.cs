using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace TrapsRearmable
{
    // Token: 0x02000003 RID: 3
    public class WorkGiver_RearmTraps : WorkGiver_Scanner
    {
        // Token: 0x06000006 RID: 6 RVA: 0x00002106 File Offset: 0x00000306
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            foreach (Designation designation in pawn.Map.designationManager.SpawnedDesignationsOfDef(TrapsDefOf.TR_RearmTrap))
            {
                yield return designation.target.Thing;
            }
        //    IEnumerator<Designation> enumerator = null;
            yield break;
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000007 RID: 7 RVA: 0x00002116 File Offset: 0x00000316
        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.ClosestTouch;
            }
        }

        // Token: 0x06000008 RID: 8 RVA: 0x00002116 File Offset: 0x00000316
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Verse.Danger.Deadly;
        }

        // Token: 0x06000009 RID: 9 RVA: 0x0000211C File Offset: 0x0000031C
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(t, TrapsDefOf.TR_RearmTrap) == null)
            {
                return false;
            }
            LocalTargetInfo localTargetInfo = t;
            if (!ReservationUtility.CanReserve(pawn, localTargetInfo, 1, -1, null, forced))
            {
                return false;
            }
            List<Thing> thingList = GridsUtility.GetThingList(t.Position, t.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i] != t && thingList[i].def.category == ThingCategory.Item && (ForbidUtility.IsForbidden(thingList[i], pawn) || StoreUtility.IsInValidStorage(thingList[i]) || !HaulAIUtility.CanHaulAside(pawn, thingList[i], out IntVec3 intVec)))
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000021CC File Offset: 0x000003CC
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            List<Thing> thingList = GridsUtility.GetThingList(t.Position, t.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (thingList[i] != t && thingList[i].def.category == ThingCategory.Item)
                {
                    Job job = HaulAIUtility.HaulAsideJobFor(pawn, thingList[i]);
                    if (job != null)
                    {
                        return job;
                    }
                }
            }
            return new Job(TrapsDefOf.TR_RearmTrapJob, t);
        }
    }
}
