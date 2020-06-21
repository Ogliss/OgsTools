using DualWield;
using DualWield.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace OgsCompOversizedWeapon
{
    public static class ExtentionMethodsCompOversizedWeapon
    {
        // Token: 0x06000022 RID: 34 RVA: 0x00003818 File Offset: 0x00001A18
        public static bool TryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
        {
            result = null;
            bool flag = instance.pawn.HasMissingArmOrHand();
            bool result2;
            if (flag)
            {
                result2 = false;
            }
            else
            {
                ExtendedDataStorage extendedDataStorage = Base.Instance.GetExtendedDataStorage();
                foreach (ThingWithComps thingWithComps in instance.AllEquipmentListForReading)
                {
                    ExtendedThingWithCompsData extendedThingWithCompsData;
                    bool flag2 = extendedDataStorage.TryGetExtendedDataFor(thingWithComps, out extendedThingWithCompsData) && extendedThingWithCompsData.isOffHand;
                    if (flag2)
                    {
                        result = thingWithComps;
                        return true;
                    }
                }
                result2 = false;
            }
            return result2;
        }
    }
}
