using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CompTurret.HarmonyInstance
{
	// Token: 0x02000019 RID: 25
	[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PostResolve", null)]
    public class Patch_GenerateImpliedDefs_PostResolve
	{
		// Token: 0x06000088 RID: 136 RVA: 0x00004224 File Offset: 0x00002424
		public static void Postfix()
		{
			ThingDef thingDef;
			thingDef = new ThingDef();
			thingDef.defName = "Mote_CompTurretStun";
			thingDef.label = "Mote";
			thingDef.category = ThingCategory.Mote;
			thingDef.thingClass = typeof(Graphic_Mote);
			thingDef.graphicData = new GraphicData();
			thingDef.graphicData.graphicClass = typeof(MoteCompTurretAttached);
			thingDef.graphicData.shaderType = DefDatabase<ShaderTypeDef>.GetNamed("Mote");
			thingDef.altitudeLayer = AltitudeLayer.MetaOverlays;
			thingDef.tickerType = TickerType.Normal;
			thingDef.useHitPoints = false;
			thingDef.isSaveable = false;
			thingDef.rotatable = false;
			thingDef.mote = new MoteProperties();
			thingDef.mote.solidTime = 9999;
			thingDef.mote.needsMaintenance = true;
			DefGenerator.AddImpliedDef<ThingDef>(thingDef);


		}
	}
}
