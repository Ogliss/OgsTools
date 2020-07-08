using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AdvancedGraphics
{
	// Token: 0x020002F3 RID: 755
	public class Graphic_AdvancedSingle : Graphic
	{
		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x06001562 RID: 5474 RVA: 0x0007D239 File Offset: 0x0007B439
		public override Material MatSingle
		{
			get
			{
				return this.subGraphics[0].MatSingle;
			}
		}

		public override void Init(GraphicRequest req)
		{
			this.data = req.graphicData;
			if (req.path.NullOrEmpty())
			{
				throw new ArgumentNullException("folderPath");
			}
			if (req.shader == null)
			{
				throw new ArgumentNullException("shader");
			}
			this.path = req.path;
			this.color = req.color;
			this.colorTwo = req.colorTwo;
			this.drawSize = req.drawSize;
			List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
									where !x.name.EndsWith(Graphic_AdvancedSingle.MaskSuffix) && !x.name.EndsWith(Graphic_AdvancedSingle.GlowSuffix) && !x.name.EndsWith(Graphic_AdvancedSingle.GlowMaskSuffix) && !x.name.Contains(Graphic_AdvancedMulti.NSuffix) && !x.name.Contains(Graphic_AdvancedMulti.SSuffix) && !x.name.Contains(Graphic_AdvancedMulti.ESuffix) && !x.name.Contains(Graphic_AdvancedMulti.WSuffix)
									orderby x.name
									select x).ToList<Texture2D>();
			if (list.NullOrEmpty<Texture2D>())
			{
				Log.Error("Collection cannot init: No textures found at path " + req.path, false);
				this.subGraphics = new Graphic[]
				{
					BaseContent.BadGraphic
				};
				return;
			}
			else
			{
				
			}
			this.subGraphics = new Graphic[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				string path = req.path + "/" + list[i].name;
			//	Log.Message("loaded "+ path);
				this.subGraphics[i] = GraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, this.drawSize, this.color, this.colorTwo, null, req.shaderParameters);
			}
		}

		// Token: 0x06001563 RID: 5475 RVA: 0x0007D251 File Offset: 0x0007B451
		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return GraphicDatabase.Get<Graphic_AdvancedSingle>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
		}

		// Token: 0x06001564 RID: 5476 RVA: 0x0007CDD6 File Offset: 0x0007AFD6
		public override Material MatAt(Rot4 rot, Thing thing = null)
		{
			if (thing == null)
			{
				return this.MatSingle;
			}
			return this.MatSingleFor(thing);
		}

		// Token: 0x06001565 RID: 5477 RVA: 0x0007D26D File Offset: 0x0007B46D
		public override Material MatSingleFor(Thing thing)
		{
			if (thing == null)
			{
				return this.MatSingle;
			}
			return this.SubGraphicFor(thing).MatSingle;
		}

		// Token: 0x06001566 RID: 5478 RVA: 0x0007D285 File Offset: 0x0007B485
		public Graphic SubGraphicFor(Thing thing)
		{
			CompAdvancedGraphic comp = thing.TryGetComp<CompAdvancedGraphic>();
			CompQuality quality = thing.TryGetComp<CompQuality>();
			CompArt art = thing.TryGetComp<CompArt>();
			if (comp != null)
			{
			//	Log.Message("SubGraphicFor "+ thing.Label + " found CompAdvancedGraphic int " + comp.gfxint);
				if (!comp.Props.onlyArtable || (art != null && art.Active))
				{
					if (comp.Props.quality)
					{
						return this.SubGraphicForQuality(thing, comp);
					}
					if (comp.Props.randomised)
					{
						return this.SubGraphicRandomized(thing, comp);
					}
				}
				if (!comp.Props.tagged.NullOrEmpty())
				{
					Graphic sub = subGraphics.FirstOrDefault(x => x.path.Contains(comp.Props.tagged));
					if (sub!=null)
					{
						return subGraphics.FirstOrDefault(x => x.path.Contains(comp.Props.tagged));
					}
				}
			}
			else
			{

			//	Log.Message("SubGraphicFor " + thing.Label + " Didnt find CompAdvancedGraphic");
			}
			return this.subGraphics[0];
		}

		// Token: 0x06001567 RID: 5479 RVA: 0x0007D29C File Offset: 0x0007B49C
		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Graphic graphic;
			if (thing != null)
			{
				graphic = this.SubGraphicFor(thing);
			}
			else
			{
				graphic = this.subGraphics[0];
			}
			graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
		}

		// Token: 0x06001568 RID: 5480 RVA: 0x0007D2D0 File Offset: 0x0007B4D0
		public Graphic SubGraphicForQuality(Thing thing, CompAdvancedGraphic advancedWeaponGraphic)
		{

			CompQuality quality = thing.TryGetComp<CompQuality>();
			if (quality == null)
			{
				Log.Warning(string.Format("WARNING!! {0} is set to use quality graphics but has no CompQuality, using random graphic", thing.Label));
				Rand.PushState();
				advancedWeaponGraphic.gfxint = Rand.RangeInclusive(0, subGraphics.Length - 1);
				Rand.PopState();
			}
			if (advancedWeaponGraphic.gfxint == -1)
			{
				if ((int)quality.Quality >= (int)advancedWeaponGraphic.Props.minQuality)
				{
					int i = (int)quality.Quality - (int)advancedWeaponGraphic.Props.minQuality + 1;
					advancedWeaponGraphic.gfxint = Math.Min(i, subGraphics.Length - 1);
				}
				else
				{
					advancedWeaponGraphic.gfxint = 0;
				}
			}
			return subGraphics[advancedWeaponGraphic.gfxint];
		}

		// Token: 0x06001568 RID: 5480 RVA: 0x0007D2D0 File Offset: 0x0007B4D0
		public Graphic SubGraphicRandomized(Thing thing, CompAdvancedGraphic advancedWeaponGraphic)
		{

			if (advancedWeaponGraphic.gfxint == -1)
			{
			//	Log.Message("SubGraphicFor " + thing.Label + " init CompAdvancedGraphic int " + advancedWeaponGraphic.gfxint);
				Rand.PushState();
				advancedWeaponGraphic.gfxint = Rand.RangeInclusive(0, subGraphics.Length - 1);
				Rand.PopState();
			}
		//	Log.Message("SubGraphicFor " + thing.Label + " now CompAdvancedGraphic int " + advancedWeaponGraphic.gfxint);
			return subGraphics[advancedWeaponGraphic.gfxint];
		}

		// Token: 0x06001569 RID: 5481 RVA: 0x0007D3AA File Offset: 0x0007B5AA
		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"Advanced(path=",
				this.path,
				", count=",
				this.subGraphics.Length,
				")"
			});
		}
		protected Graphic[] subGraphics;
		public int Count
		{
			get
			{
				return this.subGraphics.Length;
			}
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x06000452 RID: 1106 RVA: 0x000118B0 File Offset: 0x0000FAB0
		public Graphic[] Graphics
		{
			get
			{
				return this.subGraphics;
			}
		}
		public static readonly string MaskSuffix = "_m";
		public static readonly string GlowSuffix = "_Glow";
		public static readonly string GlowMaskSuffix = "_Glow_m";
		public static readonly string NSuffix = "_north";
		public static readonly string SSuffix = "_south";
		public static readonly string ESuffix = "_east";
		public static readonly string WSuffix = "_west";
		public static readonly string MMaskSuffix = "m";
		public static readonly string MaskIconSuffix = "_m";
	}
}
