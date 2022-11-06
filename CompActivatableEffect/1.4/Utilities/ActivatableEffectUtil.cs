using DualWield;
using DualWield.Storage;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace OgsCompActivatableEffect
{
    [StaticConstructorOnStartup]
    public static class ActivatableEffectUtil
    {
        public static void DrawMeshModified(Mesh mesh, Vector3 position, Quaternion rotation, Material mat, int layer, Thing eq)
        {
            CompEquippable equippable = eq.TryGetComp<CompEquippable>();
            Pawn pawn = equippable.PrimaryVerb.CasterPawn;
            Draw(mesh, default(Matrix4x4), mat, layer, eq, pawn, position, rotation);
            return;
        }

        public static void Draw(Mesh mesh, Matrix4x4 matrix, Material mat, int layer, Thing eq, Pawn pawn, Vector3 position, Quaternion rotation)
        {
            if (matrix == default(Matrix4x4))
            {
                Graphics.DrawMesh(mesh, position, rotation, mat, layer);
            }
            else Graphics.DrawMesh(mesh, matrix, mat, layer);
            ActivatableEffectUtil.DrawMeshExtra(mesh, matrix, mat, layer, eq, pawn, position, rotation);
        }

        public static void DrawMeshExtra(Mesh mesh, Matrix4x4 matrix, Material mat, int layer, Thing eq, Pawn pawn, Vector3 position, Quaternion rotation)
        {
            //    Log.Message("DrawMeshModified");
            ThingWithComps thingWithComps = eq as ThingWithComps;
            //    CompEquippable equippable = eq.TryGetComp<CompEquippable>();
            var compOversized = thingWithComps.def.comps.FirstOrDefault(x => x.compClass.Name.Contains("CompOversizedWeapon"));
            var compActivatableEffect = thingWithComps?.GetComp<CompActivatableEffect>();
            if (compActivatableEffect?.Graphic == null) return;
            if (!compActivatableEffect.IsActiveNow) return;
            var matSingle = compActivatableEffect.Graphic.MatSingle;
            Vector3 s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            if (compOversized != null)
            {
                if (pawn.RaceProps.Humanlike)
                {
                    if (HarmonyPatches_ActivatableEffect.enabled_AlienRaces)
                    {
                        Vector2 v = AlienRaceUtility.AlienRacesPatch(pawn, eq);
                        float f = Mathf.Max(v.x, v.y);
                        s = new Vector3(eq.def.graphicData.drawSize.x * f, 1f, eq.def.graphicData.drawSize.y * f);
                    }
                    else
                    {
                        s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                    }
                }
                else
                {
                    Vector2 v = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
                    s = new Vector3(eq.def.graphicData.drawSize.x + v.x / 10, 1f, eq.def.graphicData.drawSize.y + v.y / 10);
                }
            }
            Vector3 vector3 = position;
            vector3.y -= 0.0005f;
            matrix.SetTRS(vector3, rotation, s);
            Graphics.DrawMesh(mesh, matrix, matSingle, 0);
        }


        #region DualWield
        public static void AddOffHandEquipment(this Pawn_EquipmentTracker instance, ThingWithComps newEq)
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
        #endregion
    }
}
