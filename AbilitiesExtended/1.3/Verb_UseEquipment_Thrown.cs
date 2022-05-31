using Verse;

namespace AbilitesExtended
{
    // AbilitesExtended.Verb_UseEquipment_Thrown
    public class Verb_UseEquipment_Thrown : Verb_EquipmentLaunchProjectile
    {
        // Token: 0x060000E5 RID: 229 RVA: 0x000081A4 File Offset: 0x000063A4
        public void PostCastShot(bool inResult, out bool outResult)
        {
            outResult = inResult;
            ThingWithComps weapon = CasterPawn.equipment.Primary;
            CasterPawn.equipment.Notify_EquipmentRemoved(weapon);
        }

        public override bool TryCastShot()
        {
            bool result = base.TryCastShot();
            if (result)
            {
                PostCastShot(result, out result);
            }
            return result;
        }

    }
}
