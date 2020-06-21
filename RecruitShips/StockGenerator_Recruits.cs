using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Recruiters
{
	// Token: 0x02000DB0 RID: 3504
	public class StockGenerator_Recruits : StockGenerator
	{
		// Token: 0x060054FE RID: 21758 RVA: 0x001C42E9 File Offset: 0x001C24E9
		public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
		{
			if (this.respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
			{
				yield break;
			}
			int count = this.countRange.RandomInRange;
			int num;
			for (int i = 0; i < count; i = num + 1)
			{ 
				Faction faction2 = Faction.OfPlayer;
				/*
				if (faction != null)
				{
					faction2 = faction;
				}
				else
				{
					if (!(from fac in Find.FactionManager.AllFactionsVisible
						  where fac != Faction.OfPlayer && fac.def.humanlikeFaction
						  select fac).TryRandomElement(out faction2))
					{
						yield break;
					}
				}
				*/
				if (!(from kind in DefDatabase<PawnKindDef>.AllDefsListForReading
					  where kind.defaultFactionType != null && kind.RaceProps.Humanlike && (kind.defaultFactionType == Faction.OfPlayer.def || kind.defaultFactionType == faction2.def)
					  select kind).TryRandomElement(out this.slaveKindDef))
				{
					yield break;
				}
				PawnGenerationRequest request = PawnGenerationRequest.MakeDefault();
				request.KindDef = ((this.slaveKindDef != null) ? this.slaveKindDef : PawnKindDefOf.Slave);
				request.Faction = faction2;
				request.Tile = forTile;
				request.ForceAddFreeWarmLayerIfNeeded = !this.trader.orbital;
				request.RedressValidator = ((Pawn x) => x.royalty == null || !x.royalty.AllTitlesForReading.Any<RoyalTitle>());
				yield return PawnGenerator.GeneratePawn(request);
				num = i;
			}
			yield break;
		}

		// Token: 0x060054FF RID: 21759 RVA: 0x001C4300 File Offset: 0x001C2500
		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability > Tradeability.None;
		}

		// Token: 0x04002E8F RID: 11919
		private bool respectPopulationIntent;

		// Token: 0x04002E90 RID: 11920
		public PawnKindDef slaveKindDef;
	}
}
