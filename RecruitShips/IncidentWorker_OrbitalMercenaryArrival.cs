using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Recruiters
{
	// Token: 0x020009EA RID: 2538
	public class IncidentWorker_OrbitalMercenaryArrival : IncidentWorker
	{
		// Token: 0x06003C5D RID: 15453 RVA: 0x0013ED05 File Offset: 0x0013CF05
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			return base.CanFireNowSub(parms) && ((Map)parms.target).passingShipManager.passingShips.Count < 5;
		}

		// Token: 0x06003C5E RID: 15454 RVA: 0x0013ED34 File Offset: 0x0013CF34
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.passingShipManager.passingShips.Count >= 5)
			{
				return false;
			}
		//	Log.Message("TraderKindDefs with RecruiterExt: " + DefDatabase<TraderKindDef>.AllDefs.Where(x=> x.HasModExtension<RecruiterExt>()).Count());
			TraderKindDef traderKindDef;

			if ((from x in DefDatabase<TraderKindDef>.AllDefs.Where(x => /*x.HasModExtension<RecruiterExt>()*/  x.stockGenerators.Any(y => y is StockGenerator_Mercs))
				 where this.CanSpawn(map, x)
				 select x).TryRandomElement(out traderKindDef))
			{
				TradeShip tradeShip = new TradeShip(traderKindDef, this.GetFaction(traderKindDef));
				if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && (b.GetComp<CompPowerTrader>() == null || b.GetComp<CompPowerTrader>().PowerOn)))
				{
					base.SendStandardLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(tradeShip.name, tradeShip.def.label, (tradeShip.Faction == null) ? "TraderArrivalNoFaction".Translate() : "TraderArrivalFromFaction".Translate(tradeShip.Faction.Named("FACTION"))), LetterDefOf.PositiveEvent, parms, LookTargets.Invalid, Array.Empty<NamedArgument>());
				}
				map.passingShipManager.AddShip(tradeShip);
				tradeShip.GenerateThings();
				return true;
			}
			throw new InvalidOperationException();
		}

		// Token: 0x06003C5F RID: 15455 RVA: 0x0013EE94 File Offset: 0x0013D094
		private Faction GetFaction(TraderKindDef trader)
		{
			if (trader.faction == null)
			{
				return null;
			}
			Faction result;
			if (!(from f in Find.FactionManager.AllFactions
				  where f.def == trader.faction
				  select f).TryRandomElement(out result))
			{
				return null;
			}
			return result;
		}

		// Token: 0x06003C60 RID: 15456 RVA: 0x0013EEE4 File Offset: 0x0013D0E4
		private bool CanSpawn(Map map, TraderKindDef trader)
		{
			if (!trader.orbital)
			{
				return false;
			}
			if (trader.faction == null)
			{
				return true;
			}
			Faction faction = this.GetFaction(trader);
			if (faction == null)
			{
				return false;
			}
			using (List<Pawn>.Enumerator enumerator = map.mapPawns.FreeColonists.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.CanTradeWith(faction, trader))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x04002382 RID: 9090
		private const int MaxShips = 5;
	}
}
