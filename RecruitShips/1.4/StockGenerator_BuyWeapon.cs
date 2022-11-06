using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Recruiters
{
	// Token: 0x02000E45 RID: 3653
	public class StockGenerator_BuyWeapon : StockGenerator
	{
		// Token: 0x06005971 RID: 22897 RVA: 0x001DCD44 File Offset: 0x001DAF44
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			yield break;
		}

		// Token: 0x06005972 RID: 22898 RVA: 0x001DCD50 File Offset: 0x001DAF50
		public override bool HandlesThingDef(ThingDef thingDef)
		{
			ThingDef stuff = GenStuff.DefaultStuffFor(thingDef);
			return thingDef.tradeability != Tradeability.None && thingDef.techLevel <= this.maxTechLevelBuy && thingDef.IsWeapon;
		}

	}
}
