using RimWorld;
using System;
using Verse;

namespace Dropships
{
    // Token: 0x02000BE7 RID: 3047
    public static class PlayerPawnsArriveMethodExtension
    {
        // Token: 0x060047C2 RID: 18370 RVA: 0x00180DF5 File Offset: 0x0017EFF5
#pragma warning disable CS0436 // Type conflicts with imported type
        public static string ToStringHuman(this PlayerPawnsArriveMethod method)
        {
            if (method == PlayerPawnsArriveMethod.Standing)
            {
                return "PlayerPawnsArriveMethod_Standing".Translate();
            }
            if (method == PlayerPawnsArriveMethod.DropShip)
            {
                return "PlayerPawnsArriveMethod_DropShip".Translate();
            }
            if (method != PlayerPawnsArriveMethod.DropPods)
            {
                throw new NotImplementedException();
            }
            return "PlayerPawnsArriveMethod_DropPods".Translate();
        }
    }
#pragma warning restore CS0436 // Type conflicts with imported type
}
