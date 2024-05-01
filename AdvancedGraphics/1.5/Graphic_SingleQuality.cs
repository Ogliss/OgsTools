using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AdvancedGraphics
{
	// AdvancedGraphics.Graphic_SingleQuality
	public class Graphic_SingleQuality : Graphic_Single
	{
		public override void Init(GraphicRequest req)
		{
			this.data = req.graphicData;
			this.path = req.path;
			this.color = req.color;
			this.colorTwo = req.colorTwo;
			this.drawSize = req.drawSize;
			MaterialRequest req2 = default(MaterialRequest);
			string mPath = req.path + "_" + "Normal";
			Texture2D texm = ContentFinder<Texture2D>.Get(req.path + "_" + "Normal", false);
            if (texm ==null)
			{
				mPath = req.path;
				texm = ContentFinder<Texture2D>.Get(mPath, true);
			}
            else
            {
			//	Log.Message("Quality Graphic: Normal Found");
			}
			req2.mainTex = texm;
			req2.shader = req.shader;
			req2.color = this.color;
			req2.colorTwo = this.colorTwo;
			req2.renderQueue = req.renderQueue;
			req2.shaderParameters = req.shaderParameters;
			if (req.shader.SupportsMaskTex())
			{
				req2.maskTex = ContentFinder<Texture2D>.Get(mPath + Graphic_Single.MaskSuffix, false);
				if (req2.maskTex == null)
				{
				//	Log.Message("Quality Graphic: Normal failed Mask");
				}
			}
			this.matNormal = MaterialPool.MatFrom(req2);
			{
				MaterialRequest req3 = default(MaterialRequest);
				string qPath = this.path + "_" + "Awful";
				Texture2D tex = ContentFinder<Texture2D>.Get(qPath, false);
				if (tex != null)
				{
				//	Log.Message("Quality Graphic: Awful Found");
					req3.mainTex = tex;
					req3.shader = req.shader;
					req3.color = this.color;
					req3.colorTwo = this.colorTwo;
					req3.renderQueue = req.renderQueue;
					req3.shaderParameters = req.shaderParameters;
					if (req.shader.SupportsMaskTex())
					{
						req3.maskTex = ContentFinder<Texture2D>.Get(qPath + Graphic_Single.MaskSuffix, false);
                        if (req3.maskTex == null)
						{
						//	Log.Message("Quality Graphic: Awful failed Mask");
						}
					}
					this.matAwful = MaterialPool.MatFrom(req3);
				}
			}
			{
				MaterialRequest req3 = default(MaterialRequest);
				string qPath = this.path + "_" + "Poor";
				Texture2D tex = ContentFinder<Texture2D>.Get(qPath, false);
				if (tex != null)
				{
				//	Log.Message("Quality Graphic: Poor Found");
					req3.mainTex = tex;
					req3.shader = req.shader;
					req3.color = this.color;
					req3.colorTwo = this.colorTwo;
					req3.renderQueue = req.renderQueue;
					req3.shaderParameters = req.shaderParameters;
					if (req.shader.SupportsMaskTex())
					{
						req3.maskTex = ContentFinder<Texture2D>.Get(qPath + Graphic_Single.MaskSuffix, false);
						if (req3.maskTex == null)
						{
						//	Log.Message("Quality Graphic: Poor failed Mask");
						}
					}
					this.matPoor = MaterialPool.MatFrom(req3);
				}
			}
			{
				MaterialRequest req3 = default(MaterialRequest);
				string qPath = this.path + "_" + "Good";
				Texture2D tex = ContentFinder<Texture2D>.Get(qPath, false);
				if (tex != null)
				{
				//	Log.Message("Quality Graphic: Good Found");
					req3.mainTex = tex;
					req3.shader = req.shader;
					req3.color = this.color;
					req3.colorTwo = this.colorTwo;
					req3.renderQueue = req.renderQueue;
					req3.shaderParameters = req.shaderParameters;
					if (req.shader.SupportsMaskTex())
					{
						req3.maskTex = ContentFinder<Texture2D>.Get(qPath + Graphic_Single.MaskSuffix, false);
						if (req3.maskTex == null)
						{
						//	Log.Message("Quality Graphic: Good failed Mask");
						}
					}
					this.matGood = MaterialPool.MatFrom(req3);
				}
			}
			{
				MaterialRequest req3 = default(MaterialRequest);
				string qPath = this.path + "_" + "Excellent";
				Texture2D tex = ContentFinder<Texture2D>.Get(qPath, false);
				if (tex != null)
				{
				//	Log.Message("Quality Graphic: Excellent Found");
					req3.mainTex = tex;
					req3.shader = req.shader;
					req3.color = this.color;
					req3.colorTwo = this.colorTwo;
					req3.renderQueue = req.renderQueue;
					req3.shaderParameters = req.shaderParameters;
					if (req.shader.SupportsMaskTex())
					{
						req3.maskTex = ContentFinder<Texture2D>.Get(qPath + Graphic_Single.MaskSuffix, false);
						if (req3.maskTex == null)
						{
						//	Log.Message("Quality Graphic: Excellent failed Mask");
						}
					}
					this.mat = MaterialPool.MatFrom(req3);
				}
			}
			{
				MaterialRequest req3 = default(MaterialRequest);
				string qPath = this.path + "_" + "Masterwork";
				Texture2D tex = ContentFinder<Texture2D>.Get(qPath, false);
				if (tex != null)
				{
				//	Log.Message("Quality Graphic: Masterwork Found");
					req3.mainTex = tex;
					req3.shader = req.shader;
					req3.color = this.color;
					req3.colorTwo = this.colorTwo;
					req3.renderQueue = req.renderQueue;
					req3.shaderParameters = req.shaderParameters;
					if (req.shader.SupportsMaskTex())
					{
						req3.maskTex = ContentFinder<Texture2D>.Get(qPath + Graphic_Single.MaskSuffix, false);
						if (req3.maskTex == null)
						{
						//	Log.Message("Quality Graphic: Masterwork failed Mask");
						}
					}
					this.matMasterwork = MaterialPool.MatFrom(req3);
				}
			}
			{
				MaterialRequest req3 = default(MaterialRequest);
				string qPath = this.path + "_" + "Legendary";
				Texture2D tex = ContentFinder<Texture2D>.Get(qPath, false);
				if (tex != null)
				{
				//	Log.Message("Quality Graphic: Legendary Found");
					req3.mainTex = tex;
					req3.shader = req.shader;
					req3.color = this.color;
					req3.colorTwo = this.colorTwo;
					req3.renderQueue = req.renderQueue;
					req3.shaderParameters = req.shaderParameters;
					if (req.shader.SupportsMaskTex())
					{
						req3.maskTex = ContentFinder<Texture2D>.Get(qPath + Graphic_Single.MaskSuffix, false);
						if (req3.maskTex == null)
						{
						//	Log.Message("Quality Graphic: Legendary failed Mask");
						}
					}
					this.matLegendary = MaterialPool.MatFrom(req3);
				}
			}
			this.mat = matNormal;
			if (this.matAwful == null)
			{
				if (this.matPoor != null)
				{
					this.matAwful = this.matPoor;
				}
				else
				{
					this.matAwful = this.matNormal;
				}
			}
			if (this.matPoor == null)
			{
				if (this.matAwful != null)
				{
					this.matPoor = this.matAwful;
				}
				else
				{
					this.matPoor = this.matNormal;
				}
			}
			if (this.matGood == null)
			{
				this.matGood = this.matNormal;
			}
			if (this.matExecellent == null)
			{
				if (this.matGood != null)
				{
					this.matExecellent = this.matGood;
				}
				else
				{
					this.matExecellent = this.matNormal;
				}
			}
			if (this.matMasterwork == null)
			{
				if (this.matExecellent != null)
				{
					this.matMasterwork = this.matExecellent;
				}
				else
				if (this.matGood != null)
				{
					this.matMasterwork = this.matGood;
				}
				else
				{
					this.matMasterwork = this.matNormal;
				}
			}
			if (this.matLegendary == null)
			{
				if (this.matMasterwork != null)
				{
					this.matLegendary = this.matMasterwork;
				}
				else
				if (this.matExecellent != null)
				{
					this.matLegendary = this.matExecellent;
				}
				else
				if (this.matGood != null)
				{
					this.matLegendary = this.matGood;
				}
				else
				{
					this.matLegendary = this.matNormal;
				}
			}
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			return GraphicDatabase.Get<Graphic_SingleQuality>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
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
            if (thing !=null)
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
			CompQuality quality = thing.TryGetComp<CompQuality>();
			if (quality != null)
			{
				switch (quality.Quality)
				{
					case QualityCategory.Awful:
						this.mat = this.matAwful;
						return this.mat;
					case QualityCategory.Poor:
						this.mat = this.matPoor;
						return this.mat;
					case QualityCategory.Normal:
						this.mat = this.matNormal;
						return this.mat;
					case QualityCategory.Good:
						this.mat = this.matGood;
						return this.mat;
					case QualityCategory.Excellent:
						this.mat = this.matExecellent;
						return this.mat;
					case QualityCategory.Masterwork:
						this.mat = this.matMasterwork;
						return this.mat;
					case QualityCategory.Legendary:
						this.mat = this.matLegendary;
						return this.mat;
					default:
						return base.MatSingleFor(thing);
				}
			}
			return base.MatSingleFor(thing);
		}
		public Graphic QualityGraphicFor(Thing thing)
		{
			CompQuality quality = thing.TryGetComp<CompQuality>();
			if (quality != null)
			{
				Material qualMat;
				switch (quality.Quality)
				{
					case QualityCategory.Awful:
						qualMat = this.matAwful;
						break;
					case QualityCategory.Poor:
						qualMat = this.matPoor;
						break;
					case QualityCategory.Normal:
						qualMat = this.matNormal;
						break;
					case QualityCategory.Good:
						qualMat = this.matGood;
						break;
					case QualityCategory.Excellent:
						qualMat = this.matExecellent;
						break;
					case QualityCategory.Masterwork:
						qualMat = this.matMasterwork;
						break;
					case QualityCategory.Legendary:
						qualMat = this.matLegendary;
						break;
					default:
						qualMat = base.MatSingleFor(thing);
						break;
				}

				int idx = this.path.LastIndexOf('/');

				if (idx != -1)
				{
				//	Log.Message(this.path.Substring(0, idx));
				//	Log.Message(this.path.Substring(idx + 1));
					string tex = this.path.Substring(0, idx) + "/" + qualMat.mainTexture.name;
				//	Log.Message("Quality Graphic using: " + tex + " drawSize: "+this.drawSize + " color: " + thing.DrawColor + " colorTwo: " + thing.DrawColorTwo);
					Graphic graphic = GraphicDatabase.Get(typeof(Graphic_Single), tex, this.data.shaderType.Shader, this.drawSize, thing.DrawColor, thing.DrawColorTwo, this.data, this.data.shaderParameters) as Graphic;
					return graphic;
				}
			}
			return this;
		}
		protected Material matAwful;
		protected Material matPoor;
		protected Material matGood;
		protected Material matNormal;
		protected Material matExecellent;
		protected Material matMasterwork;
		protected Material matLegendary;
	}
}
