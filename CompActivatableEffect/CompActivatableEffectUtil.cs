using DualWield;
using DualWield.Storage;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace OgsCompActivatableEffect
{
    [StaticConstructorOnStartup]
    public static class CompActivatableEffectUtil
    {

        // Token: 0x06000021 RID: 33 RVA: 0x000037B0 File Offset: 0x000019B0
        public static void CAEAddOffHandEquipment(this Pawn_EquipmentTracker instance, ThingWithComps newEq)
        {
            ThingOwner<ThingWithComps> value = Traverse.Create(instance).Field("equipment").GetValue<ThingOwner<ThingWithComps>>();
            ExtendedDataStorage extendedDataStorage = Base.Instance.GetExtendedDataStorage();
            bool flag = extendedDataStorage != null;
            if (flag)
            {
                extendedDataStorage.GetExtendedDataFor(newEq).isOffHand = true;
                LessonAutoActivator.TeachOpportunity(DW_DefOff.DW_Penalties, 0);
                LessonAutoActivator.TeachOpportunity(DW_DefOff.DW_Settings, 0);
                value.TryAdd(newEq, true);
            }
        }


        // Token: 0x06000022 RID: 34 RVA: 0x00003818 File Offset: 0x00001A18
        public static bool CAETryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
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
