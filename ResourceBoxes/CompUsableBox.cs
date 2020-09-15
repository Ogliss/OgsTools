using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	// Token: 0x02000D2C RID: 3372
	public class CompUsableBox : CompUsable
	{
		// Token: 0x060051E7 RID: 20967 RVA: 0x001B4B06 File Offset: 0x001B2D06
		protected override string FloatMenuOptionLabel(Pawn pawn)
		{
			return string.Format(this.Props.useLabel, this.parent.Label);
		}

	}
}
