using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AdvancedGraphics.HarmonyInstance
{
	// Token: 0x020001AF RID: 431
	[HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
	public static class AG_ApparelGraphicRecordGetter_TryGetGraphicApparel_AdvancedGraphics_Patch
	{

		// Token: 0x060008F6 RID: 2294 RVA: 0x0004A904 File Offset: 0x00048B04
		[HarmonyPostfix]
		public static void Postfix(Apparel apparel, BodyTypeDef bodyType, ref ApparelGraphicRecord rec)
		{
			if (apparel.def.apparel.wornGraphicPath.NullOrEmpty())
			{
				return;
			}
			CompAdvancedGraphic comp = apparel.TryGetComp<CompAdvancedGraphic>();
			CompQuality quality = apparel.TryGetComp<CompQuality>();
			CompArt art = apparel.TryGetComp<CompArt>();
			bool adv = comp != null;
			if (adv)
			{
				if (comp._graphic == null || comp._graphic.path.NullOrEmpty())
				{


					bool flag = apparel.DefaultGraphic as Graphic_RandomRotated != null;
					if (flag)
					{
					//	Log.Message(apparel.LabelShortCap + " as Graphic_RandomRotated");
						Traverse traverse = Traverse.Create(apparel.DefaultGraphic);
						Graphic_AdvancedSingle subGraphic = AG_Thing_get_DefaultGraphic_CompAdvancedGraphic_Patch.subgraphic.GetValue(apparel.DefaultGraphic) as Graphic_AdvancedSingle;
						if (subGraphic != null)
						{
						//	Log.Message(apparel.LabelShortCap + " as Graphic_AdvancedSingle");
							comp._graphic = subGraphic.SubGraphicFor(apparel);
						}
					}
					else
					{
						comp._graphic = apparel.DefaultGraphic;
						return;
					}

				}
				string path = apparel.def.apparel.wornGraphicPath;
				Shader shader = ShaderDatabase.Cutout;
				if (apparel.def.apparel.useWornGraphicMask)
				{
					shader = ShaderDatabase.CutoutComplex;
				}
				if (apparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparel.def.apparel.wornGraphicPath == BaseContent.PlaceholderImagePath)
				{
					path = comp._graphic.path;
				}
				else
				{
					path = comp._graphic.path + "_" + bodyType.defName;
				}
				Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor, apparel.DrawColorTwo);
				rec = new ApparelGraphicRecord(graphic, apparel);
			}
			if (apparel.DrawColorTwo != Color.white)
			{

				rec.graphic = rec.graphic.GetColoredVersion(rec.graphic.Shader, apparel.DrawColor, apparel.DrawColorTwo);
			}
		}

	}
}
