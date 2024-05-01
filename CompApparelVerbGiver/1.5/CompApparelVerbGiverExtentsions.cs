using System;
using Verse;

namespace CompApparelVerbGiver
{
    public static class CompApparelVerbGiverExtentsions
    {
        public static void Notify_PrimaryDestroyed(this Pawn_EquipmentTracker tracker)
        {
            if (tracker.Primary != null)
            {
                tracker.Remove(tracker.Primary);
            }
            if (tracker.pawn.Spawned)
            {
                tracker.pawn.stances.CancelBusyStanceSoft();
            }
        }
    }
}
