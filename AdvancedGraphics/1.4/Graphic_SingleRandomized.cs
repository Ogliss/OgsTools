using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AdvancedGraphics
{
	// AdvancedGraphics.Graphic_SingleRandomized
	public class Graphic_SingleRandomized : Graphic_Single
	{
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
			int idx = this.path.LastIndexOf('/');
			string name = string.Empty;
			/*
			if (idx != -1)
			{
					string s1 = this.path.Substring(0, idx);
					Log.Message(s1);
				string s2 = this.path.Substring(idx + 1);
				name = s2;
					Log.Message(s2);

				this.path = req.path;// + "/" + s2;
					Log.Message(path);
			}
			*/
			bool flag = ContentFinder<Texture2D>.Get(this.path, false);
			List<Texture2D> list;

			if (flag)
			{
				list = (from x in ContentFinder<Texture2D>.GetAllInFolder(path)
										where (Regex.Match(x.name, @"^[^0-9]*").Success || x.name.Contains(name)) && !x.name.EndsWith(MaskSuffix) && !x.name.EndsWith(GlowSuffix) && !x.name.EndsWith(GlowMaskSuffix) && !x.name.Contains(NSuffix) && !x.name.Contains(SSuffix) && !x.name.Contains(ESuffix) && !x.name.Contains(WSuffix)
										orderby x.name
										select x).ToList<Texture2D>();
			}
            else
			{
				list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
						where (Regex.Match(x.name, @"^[^0-9]*").Success || x.name.Contains(name)) && !x.name.EndsWith(MaskSuffix) && !x.name.EndsWith(GlowSuffix) && !x.name.EndsWith(GlowMaskSuffix) && !x.name.Contains(NSuffix) && !x.name.Contains(SSuffix) && !x.name.Contains(ESuffix) && !x.name.Contains(WSuffix)
						orderby x.name
						select x).ToList<Texture2D>();
			}
			if (list.NullOrEmpty<Texture2D>())
			{
				Log.Error("Collection cannot init: No textures found at path " + path);
				this.subGraphics = new Graphic[]
				{
					BaseContent.BadGraphic
				};
				return;
			}

			//	Log.Message("found " + list.Count()+" Variants");

			this.subGraphics = new Graphic[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				string path = req.path + "/" + list[i].name;
				//	Log.Message("loaded "+ path);
				this.subGraphics[i] = GraphicDatabase.Get(typeof(Graphic_Single), path, req.shader, this.drawSize, this.color, this.colorTwo, this.data, req.shaderParameters);
			}
			this.mat = this.subGraphics[0].MatSingle;
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return GraphicDatabase.Get<Graphic_Single>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
		}

		public override Material MatSingle
		{
			get
			{
				return base.MatSingle;
			}
		}
		public override Material MatAt(Rot4 rot, Thing thing = null)
		{
			if (thing != null)
			{
				return MatSingleFor(thing);

			}
			switch (rot.AsInt)
			{
				case 0:
					return this.MatNorth;
				case 1:
					return this.MatEast;
				case 2:
					return this.MatSouth;
				case 3:
					return this.MatWest;
				default:
					return BaseContent.BadMat;
			}
		}

		public override Material MatSingleFor(Thing thing)
		{
			return RandomGraphicFor(thing).MatSingle;
		}
		public Graphic RandomGraphicFor(Thing thing)
		{
			Graphic graphic;
			CompQuality quality = thing.TryGetComp<CompQuality>();
			if (quality != null)
			{
				if (subGraphics.Any(x => x.path.Contains(quality.Quality.GetLabel().CapitalizeFirst())))
				{
					graphic = subGraphics.Where(x => x.path.Contains(quality.Quality.GetLabel().CapitalizeFirst())).ToList()[thing.thingIDNumber % Count];
					return graphic.GetColoredVersion(graphic.Shader, thing.DrawColor, thing.DrawColorTwo);
				}
			}
			graphic = subGraphics[thing.thingIDNumber % Count];
			return graphic.GetColoredVersion(graphic.Shader, thing.DrawColor, thing.DrawColorTwo);
		}

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
		//	public static readonly string MaskSuffix = "_m";
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
