using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace OgsCompOversizedWeapon
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches_OversizedWeapon
    {
        public static bool enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
        public static bool enabled_rooloDualWield;
        public static bool enabled_YayosCombat;
        static HarmonyPatches_OversizedWeapon()
        {
            enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
            enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "Roolo.DualWield");
            enabled_YayosCombat = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "com.yayo.combat3");

            var harmony = new Harmony("rimworld.Ogliss.comps.oversized");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            if (enabled_rooloDualWield)
            {
                {
                    MethodInfo target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "DrawEquipmentAimingOverride", null, null);
                    if (target == null)
                    {
                        Log.Warning("Target: DualWield.Harmony.PawnRenderer_DrawEquipmentAiming.DrawEquipmentAimingOverride Not found");
                    }
                    MethodInfo patch = typeof(PawnRenderer_DrawEquipmentAiming_DualWield_Transpiler).GetMethod("Transpiler");
                    if (patch == null)
                    {
                        Log.Warning("Patch is null PawnRenderer_DrawEquipmentAiming_DualWield_Transpiler.Transpiler");
                    }
                    if (target != null && patch != null)
                    {
                        if (harmony.Patch(target, null, null, new HarmonyMethod(patch)) == null)
                        {
                            Log.Warning("OgsCompOversizedWeapon: DualWield Patch Failed to apply");
                        }
                        else
                        {
                        //    Log.Message("OgsCompOversizedWeapon: DualWield Patch applied!");
                        }
                    }
                }
                {
                    MethodInfo target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("DualWield.Harmony.PawnRenderer_DrawEquipmentAiming", "DualWield.Harmony"), "Prefix", null, null);
                    if (target == null)
                    {
                        Log.Warning("Target: DualWield.Harmony.PawnRenderer_DrawEquipmentAiming.Prefix Not found");
                    }
                    MethodInfo patch = typeof(PawnRenderer_DrawEquipmentAiming_DualWieldPrefix_Transpiler).GetMethod("Transpiler");
                    if (patch == null)
                    {
                        Log.Warning("Patch is null PawnRenderer_DrawEquipmentAiming_DualWieldPrefix_Transpiler.Transpiler");
                    }
                    if (target != null && patch != null)
                    {
                        if (harmony.Patch(target, null, null, new HarmonyMethod(patch)) == null)
                        {
                            Log.Warning("OgsCompOversizedWeapon: DualWieldPrefix Patch Failed to apply");
                        }
                        else
                        {
                        //    Log.Message("OgsCompOversizedWeapon: DualWieldPrefix Patch applied!");
                        }
                    }
                }
            }
            if (enabled_YayosCombat)
            {
                // PawnRenderer_override.DrawEquipmentAiming
                MethodInfo target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("yayoCombat.PawnRenderer_override", "yayoCombat"), "DrawEquipmentAiming", null, null);
                if (target == null)
                {
                    target = AccessTools.Method(GenTypes.GetTypeInAnyAssembly("yayoCombat.patch_DrawEquipmentAiming", "yayoCombat"), "Prefix", null, null);
                }
                if (target == null)
                {
                    Log.Warning("Target: yayoCombat.patch_DrawEquipmentAiming.Prefix Not found");
                }
                MethodInfo patch = typeof(PawnRenderer_DrawEquipmentAiming_YayoPrefix_Transpiler).GetMethod("Transpiler");
                if (patch == null)
                {
                    Log.Warning("Patch is null PawnRenderer_DrawEquipmentAiming_Yayo_Transpiler.Transpiler");
                }
                if (target != null && patch != null)
                {
                    if (harmony.Patch(target, null, null, new HarmonyMethod(patch)) == null)
                    {
                        Log.Warning("OgsCompOversizedWeapon: YayosCombat Patch Failed to apply");
                    }
                    else
                    {
                    //    Log.Message("OgsCompOversizedWeapon: YayosCombat Patch applied!");
                    }
                }
            }
            harmony.Patch(AccessTools.Method(typeof(Thing), "get_DefaultGraphic"), null,
                new HarmonyMethod(typeof(HarmonyPatches_OversizedWeapon), nameof(get_DefaultGraphic_PostFix)));
        }
       
        public static void get_DefaultGraphic_PostFix(Thing __instance, Graphic ___graphicInt, ref Graphic __result)
        {
            if (___graphicInt == null) return;
            if (__instance.ParentHolder is Pawn) return;
            if (!(__instance is ThingWithComps withComps)) return;
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