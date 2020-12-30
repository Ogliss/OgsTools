using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace OgsLasers
{
	// Token: 0x02000019 RID: 25
	[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PostResolve", null)]
	public class Patch_GenerateImpliedDefs_PostResolve
	{
		// Token: 0x06000088 RID: 136 RVA: 0x00004224 File Offset: 0x00002424
		public static void Postfix()
		{
			ThingDef thingDef;
			if (DefDatabase<ThingDef>.GetNamedSilentFail("Mote_BloodPuff") == null)
			{
				thingDef = new ThingDef();
				thingDef.defName = "Mote_BloodPuff";
				thingDef.label = "blood";
				thingDef.category = ThingCategory.Mote;
				thingDef.thingClass = typeof(Graphic_Mote);
				thingDef.graphicData = new GraphicData();
				thingDef.graphicData.texPath = "Things/Mote/BodyImpact";
				thingDef.graphicData.shaderType = DefDatabase<ShaderTypeDef>.GetNamed("Mote");
				thingDef.altitudeLayer = AltitudeLayer.MoteOverhead;
				thingDef.tickerType = TickerType.Normal;
				thingDef.useHitPoints = false;
				thingDef.isSaveable = false;
				thingDef.rotatable = false;
				thingDef.mote = new MoteProperties();
				thingDef.mote.fadeInTime = 0.04f;
				thingDef.mote.solidTime = 0.25f;
				thingDef.mote.fadeOutTime = 0.1f;
				DefGenerator.AddImpliedDef<ThingDef>(thingDef);
			}

			if (DefDatabase<ThingDef>.GetNamedSilentFail("Mote_Blood_Puff") == null)
			{
				thingDef = new ThingDef();
				thingDef.defName = "Mote_Blood_Puff";
				thingDef.label = "blood";
				thingDef.category = ThingCategory.Mote;
				thingDef.thingClass = typeof(Graphic_Mote);
				thingDef.graphicData = new GraphicData();
				thingDef.graphicData.texPath = "Things/Mote/BodyImpact";
				thingDef.graphicData.shaderType = DefDatabase<ShaderTypeDef>.GetNamed("Mote");
				thingDef.altitudeLayer = AltitudeLayer.MoteOverhead;
				thingDef.tickerType = TickerType.Normal;
				thingDef.useHitPoints = false;
				thingDef.isSaveable = false;
				thingDef.rotatable = false;
				thingDef.mote = new MoteProperties();
				thingDef.mote.fadeInTime = 0.04f;
				thingDef.mote.solidTime = 0.25f;
				thingDef.mote.fadeOutTime = 0.1f;
				DefGenerator.AddImpliedDef<ThingDef>(thingDef);
			}
			EffecterDef effecterDef;
			if (DefDatabase<EffecterDef>.GetNamedSilentFail("LaserImpact") == null)
			{
				effecterDef = new EffecterDef();
				effecterDef.defName = "LaserImpact";
				effecterDef.label = "laser impact";
				effecterDef.children = new List<SubEffecterDef>();
				effecterDef.children.Add(new SubEffecterDef()
				{
					subEffecterClass = typeof(SubEffecter_SprayerTriggered),
					moteDef = ThingDef.Named("Mote_SparkFlash"),
					positionLerpFactor = 0.6f,
					chancePerTick = 0.2f,
					scale = new FloatRange(2.5f, 4.5f),
					spawnLocType = MoteSpawnLocType.OnSource
				});
				effecterDef.children.Add(new SubEffecterDef()
				{
					subEffecterClass = typeof(SubEffecter_SprayerTriggered),
					positionRadius = 0.2f,
					moteDef = ThingDef.Named("Mote_AirPuff"),
					burstCount = new IntRange(4,5),
					speed = new FloatRange(0.4f,0.8f),
					scale = new FloatRange(0.5f, 0.8f),
					spawnLocType = MoteSpawnLocType.OnSource
				});
				effecterDef.children.Add(new SubEffecterDef()
				{
					subEffecterClass = typeof(SubEffecter_SprayerTriggered),
					positionRadius = 0.2f,
					moteDef = ThingDef.Named("Mote_SparkThrownFast"),
					burstCount = new IntRange(4,5),
					speed = new FloatRange(3.3f,5f),
					scale = new FloatRange(0.1f, 0.2f),
					spawnLocType = MoteSpawnLocType.OnSource
				});
				effecterDef.children.Add(new SubEffecterDef()
				{
					subEffecterClass = typeof(SubEffecter_SprayerTriggered),
					positionRadius = 0.2f,
					moteDef = ThingDef.Named("Mote_MicroSparksFast"),
					burstCount = new IntRange(1,1),
					speed = new FloatRange(0.3f,0.4f),
					rotationRate = new FloatRange(5f,10f),
					scale = new FloatRange(0.3f, 0.5f),
					spawnLocType = MoteSpawnLocType.OnSource
				});

				effecterDef.children.Add(new SubEffecterDef()
				{
					subEffecterClass = typeof(SubEffecter_SprayerTriggered),
					positionRadius = 0.1f,
					moteDef = ThingDef.Named("Mote_SparkFlash"),
					burstCount = new IntRange(1, 1),
					scale = new FloatRange(0.9f, 1.3f),
					spawnLocType = MoteSpawnLocType.OnSource
				});
				effecterDef.offsetTowardsTarget = new FloatRange();
				effecterDef.positionRadius = 0.01f;
				DefGenerator.AddImpliedDef<EffecterDef>(effecterDef);
			}

		}
	}
}
