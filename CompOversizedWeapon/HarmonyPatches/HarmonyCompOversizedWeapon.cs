using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace OgsCompOversizedWeapon
{
    [StaticConstructorOnStartup]
    public static class HarmonyCompOversizedWeapon
    {
        public static bool enabled_AlienRaces;
        public static bool enabled_rooloDualWield;
        static HarmonyCompOversizedWeapon()
        {
            enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
            enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "Roolo.DualWield");

            var harmony = new Harmony("rimworld.Ogliss.comps.oversized");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            MethodInfo target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null);
            if (target == null && enabled_rooloDualWield)
            {
                Log.Warning("Target: DualWield.Harmony.PawnRenderer_DrawEquipmentAiming.DrawEquipmentAimingOverride Not found");
            }
            MethodInfo patch = typeof(Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler).GetMethod("Transpiler");
            if (patch == null && enabled_rooloDualWield)
            {
                Log.Warning("Patch is null Harmony_PawnRenderer_DrawEquipmentAimingOverride_Transpiler.Transpiler");
            }
            if (target != null && patch != null && enabled_rooloDualWield)
            {
                if (harmony.Patch(target, null, null, new HarmonyMethod(patch)) == null)
                {
                    Log.Warning("OgsCompOversizedWeapon: DualWield Patch Failed to apply");
                }
            }
            harmony.Patch(AccessTools.Method(typeof(Thing), "get_DefaultGraphic"), null,
                new HarmonyMethod(typeof(HarmonyCompOversizedWeapon), nameof(get_DefaultGraphic_PostFix)));
        }

        public static void get_DefaultGraphic_PostFix(Thing __instance, Graphic ___graphicInt, ref Graphic __result)
        {
            if (___graphicInt == null) return;
            if (__instance.ParentHolder is Pawn) return;
            ThingWithComps withComps = __instance as ThingWithComps;
            if (withComps == null) return;
            CompOversizedWeapon compOversizedWeapon = null;
            for (int i = 0; i < withComps.AllComps.Count; i++)
            {
                compOversizedWeapon = withComps.AllComps[i] as CompOversizedWeapon;
                if (compOversizedWeapon != null)
                {
                    break;
                }
            }
            if (compOversizedWeapon != null)
            {
            //    Log.Message("am i doing anything? :" + __instance);
                //Following commented-out section is an unnecessary "optimization" that actually hurts performance due to the reflection involved.
                //var activatableEffect =
                //    thingWithComps.AllComps.FirstOrDefault(
                //        y => y.GetType().ToString().Contains("ActivatableEffect"));
                //if (activatableEffect != null)
                //{
                //    var getPawn = Traverse.Create(activatableEffect).Property("GetPawn").GetValue<Pawn>();
                //    if (getPawn != null)
                //        return;
                //}
                if (compOversizedWeapon.Props?.groundGraphic == null)
                {
                    ___graphicInt.drawSize = __instance.def.graphicData.drawSize;
                    __result = ___graphicInt;
                }
                else // compOversizedWeapon.Props.groundGraphic != null
                {
                    if (compOversizedWeapon.IsEquipped)
                    {
                        ___graphicInt.drawSize = __instance.def.graphicData.drawSize;
                        __result = ___graphicInt;
                    }
                    else
                    {
                        if (compOversizedWeapon.Props.groundGraphic.GraphicColoredFor(__instance) is Graphic
                            newResult)
                        {
                            newResult.drawSize = compOversizedWeapon.Props.groundGraphic.drawSize;
                            __result = newResult;
                        }
                        else
                        {
                            ___graphicInt.drawSize = __instance.def.graphicData.drawSize;
                            __result = ___graphicInt;
                        }
                    }
                }
            }
        }
    }
}