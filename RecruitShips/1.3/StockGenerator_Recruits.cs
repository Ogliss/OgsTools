using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Recruiters
{
	public class StockGenerator_Recruits : StockGenerator
	{
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
				Faction faction2;

				if (faction != null)
				{
					faction2 = faction;
				}
				else
				{
					faction2 = Faction.OfPlayer;
					/*
						if (!(from fac in Find.FactionManager.AllFactionsVisible
							  where fac != Faction.OfPlayer && fac.def.humanlikeFaction
							  select fac).TryRandomElement(out faction2))
						{
							yield break;
						}
					*/
				}
				if (!(from kind in DefDatabase<PawnKindDef>.AllDefsListForReading
					  where kind.defaultFactionType != null && kind.RaceProps.Humanlike && (kind.defaultFactionType == Faction.OfPlayer.def || kind.defaultFactionType == faction2.def)
					  select kind).TryRandomElement(out this.slaveKindDef))
				{
					yield break;
				}

				PawnGenerationRequest request = PawnGenerationRequest.MakeDefault();
				request.KindDef = ((this.slaveKindDef != null) ? this.slaveKindDef : PawnKindDefOf.Slave);
				request.Faction = null;
				request.Tile = forTile;
				request.ForceAddFreeWarmLayerIfNeeded = !this.trader.orbital;
				request.RedressValidator = ((Pawn x) => x.royalty == null || !x.royalty.AllTitlesForReading.Any<RoyalTitle>());
				Pawn p = PawnGenerator.GeneratePawn(request);
				p.guest.joinStatus = JoinStatus.JoinAsColonist;
				yield return p;
				num = i;
			}
			yield break;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef.category == ThingCategory.Pawn && thingDef.race.Humanlike && thingDef.tradeability > Tradeability.None;
		}

		private bool respectPopulationIntent;

		public PawnKindDef slaveKindDef;
	}
}
