using System;
using System.Xml;
using Verse;

namespace CrashedShipsExtension
{
    // Token: 0x020008EB RID: 2283
    public class PawnGenOption
	{
		public PawnGenOption(RimWorld.PawnGenOption opt)
        {
			this.kind = opt.kind;
			this.selectionWeight = opt.selectionWeight;
        }
		public PawnGenOption(PawnKindDef kind, float weight)
        {
			this.kind = kind;
			this.selectionWeight = weight;
        }

		public float Cost
		{
			get
			{
				return this.kind.combatPower;
			}
		}

		// Token: 0x0600374F RID: 14159 RVA: 0x0012BEB0 File Offset: 0x0012A0B0
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"(",
				(this.kind != null) ? this.kind.ToString() : "null",
				" w=",
				this.selectionWeight.ToString("F2"),
				" c=",
				(this.kind != null) ? this.Cost.ToString("F2") : "null",
				")"
			});
		}

		// Token: 0x06003750 RID: 14160 RVA: 0x0012BF3D File Offset: 0x0012A13D
		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "kind", xmlRoot.Name, null, null);
			this.selectionWeight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}

		// Token: 0x04001EBF RID: 7871
		public PawnKindDef kind;

		// Token: 0x04001EC0 RID: 7872
		public float selectionWeight;
	}
}
