using RimWorld;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ConfigureablePlants
{
    // ConfigureablePlants.ConfigureablePlant
    public class ConfigureablePlant : Plant
    {
        public ConfigureablePlantProperties Props => this.def.plant as ConfigureablePlantProperties;
        public new bool HasEnoughLightToGrow => true;
        public override bool DyingBecauseExposedToLight
        {
            get
            {
                return this.def.plant.cavePlant && base.Spawned && base.Map.glowGrid.GameGlowAt(base.Position, true) > 0f;
            }
        }
        public new float GrowthRateFactor_Light
        {
            get
            {
                float num = base.Map.glowGrid.GameGlowAt(base.Position, false);
                if (num >= this.def.plant.growMinGlow && num <= this.def.plant.growOptimalGlow)
                {
                    return 1f;
                }
                if (num > this.def.plant.growOptimalGlow)
                {
                    return GenMath.InverseLerp(this.def.plant.growOptimalGlow, this.def.plant.growOptimalGlow * 2, num);
                }
                return GenMath.InverseLerp(this.def.plant.growMinGlow, this.def.plant.growOptimalGlow, num);
            }
        }
        public new float GrowthRateFactor_Temperature
        {
            get
            {
                float num;
                if (!GenTemperature.TryGetTemperatureForCell(base.Position, base.Map, out num))
                {
                    return 1f;
                }
                if (num < Props.tempsOptimal.min)
                {
                    return Mathf.InverseLerp(Props.tempsLimits.min, Props.tempsOptimal.min, num);
                }
                if (num > Props.tempsOptimal.max) return Mathf.InverseLerp(Props.tempsLimits.max, Props.tempsOptimal.max, num);
                return 1f;
            }
        }

        public override float GrowthRate
        {
            get
            {
                if (this.Blighted || this.Resting)
                {
                    return 0f;
                }
                /*
                if (base.Spawned && !PlantUtility.GrowthSeasonNow(base.Position, base.Map, false))
                {
                    return 0f;
                }
                */
                return this.GrowthRateFactor_Fertility * this.GrowthRateFactor_Temperature * this.GrowthRateFactor_Light;
            }
        }
        public static float Inverse(float val) => 1f / val;

        public override void TickLong()
        {
            if (base.Destroyed)
            {
                return;
            }
            if (this.AllComps != null)
            {
                int i = 0;
                int count = this.AllComps.Count;
                while (i < count)
                {
                    this.AllComps[i].CompTickLong();
                    i++;
                }
            }
            float num = this.growthInt;
            bool flag = this.LifeStage == PlantLifeStage.Mature;
            this.growthInt += this.GrowthPerTick * 2000f;
            if (this.growthInt > 1f)
            {
                this.growthInt = 1f;
            }
            if (((!flag && this.LifeStage == PlantLifeStage.Mature) || (int)(num * 10f) != (int)(this.growthInt * 10f)) && this.CurrentlyCultivated())
            {
                base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
            }
            this.unlitTicks = 0;
            this.ageInt += 2000;
            if (this.Dying)
            {
                Map map = base.Map;
                bool isCrop = this.IsCrop;
                bool harvestableNow = this.HarvestableNow;
                bool dyingBecauseExposedToLight = this.DyingBecauseExposedToLight;
                int num2 = Mathf.CeilToInt(this.CurrentDyingDamagePerTick * 2000f);
                base.TakeDamage(new DamageInfo(DamageDefOf.Rotting, (float)num2, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                if (base.Destroyed)
                {
                    if (isCrop && this.def.plant.Harvestable && MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfRot-" + this.def.defName, 240f))
                    {
                        string key;
                        if (harvestableNow)
                        {
                            key = "MessagePlantDiedOfRot_LeftUnharvested";
                        }
                        else if (dyingBecauseExposedToLight)
                        {
                            key = "MessagePlantDiedOfRot_ExposedToLight";
                        }
                        else
                        {
                            key = "MessagePlantDiedOfRot";
                        }
                        Messages.Message(key.Translate(this.GetCustomLabelNoCount(false)), new TargetInfo(base.Position, map, false), MessageTypeDefOf.NegativeEvent, true);
                    }
                    return;
                }
            }
            this.cachedLabelMouseover = null;
        }
        // Token: 0x0600525E RID: 21086 RVA: 0x001BC6D4 File Offset: 0x001BA8D4
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.LifeStage == PlantLifeStage.Growing)
            {
                stringBuilder.AppendLine("PercentGrowth".Translate(this.GrowthPercentString));
                stringBuilder.AppendLine("GrowthRate".Translate() + ": " + this.GrowthRate.ToStringPercent());
                if (!this.Blighted)
                {
                    if (this.Resting)
                    {
                        stringBuilder.AppendLine("PlantResting".Translate());
                    }
                    if (!this.HasEnoughLightToGrow)
                    {
                        stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + this.def.plant.growMinGlow.ToStringPercent());
                    }
                    float growthRateFactor_Temperature = this.GrowthRateFactor_Temperature;
                    if (growthRateFactor_Temperature < 0.99f)
                    {
                        if (growthRateFactor_Temperature < 0.01f)
                        {
                            stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
                        }
                        else
                        {
                            stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf.RoundToInt(growthRateFactor_Temperature * 100f).ToString()));
                        }
                    }
                }
            }
            else if (this.LifeStage == PlantLifeStage.Mature)
            {
                if (this.HarvestableNow)
                {
                    stringBuilder.AppendLine("ReadyToHarvest".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("Mature".Translate());
                }
            }
            if (this.DyingBecauseExposedToLight)
            {
                stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
            }
            if (this.Blighted)
            {
                stringBuilder.AppendLine("Blighted".Translate() + " (" + this.Blight.Severity.ToStringPercent() + ")");
            }
            string text = base.InspectStringPartsFromComps();
            if (!text.NullOrEmpty())
            {
                stringBuilder.Append(text);
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
        private string cachedLabelMouseover;
    }
}
