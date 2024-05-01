using ExtraHives.ExtensionMethods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ExtraHives
{
	public abstract class LordToil_HiveRelated : LordToil
	{
		private LordToil_HiveRelatedData Data
		{
			get
			{
				return (LordToil_HiveRelatedData)this.data;
			}
		}

		public LordToil_HiveRelated()
		{
			this.data = new LordToil_HiveRelatedData();
		}

		protected void FilterOutUnspawnedHives()
		{
			this.Data.assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Value == null || !x.Value.Spawned);
		}

		protected Hive GetHiveFor(Pawn pawn)
		{
			Hive hive;
			if (this.Data.assignedHives.TryGetValue(pawn, out hive))
			{
				return hive;
			}
			hive = this.FindClosestHive(pawn);
			if (hive != null)
			{
				this.Data.assignedHives.Add(pawn, hive);
			}
			return hive;
		}

		private Hive FindClosestHive(Pawn pawn)
		{
			ThingDef hiveDef = RimWorld.ThingDefOf.Hive;
			Hive hive = null;
			bool pawnFaction = pawn.Faction != null;
			bool pawnDefaultFaction = pawn.kindDef.defaultFactionType != null;
			FactionDef factionDef = pawnFaction ? pawn.Faction.def : (pawnDefaultFaction ? pawn.kindDef.defaultFactionType : null);

			if (factionDef == null)
			{
				return null;
			}
			List <ThingDef> defs = factionDef.HivedefsFor();

			if (pawn.Faction!=null && !defs.NullOrEmpty())
			{
				foreach (ThingDef td in defs)
				{
					hiveDef = td;
					hive = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(hiveDef), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, (Thing x) => x.Faction == pawn.Faction, null, 0, 30, false, RegionType.Set_Passable, false) as Hive;
					if (hive != null)
					{
						return hive;
					}
				}
			}
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(hiveDef), PathEndMode.Touch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 30f, (Thing x) => x.Faction == pawn.Faction, null, 0, 30, false, RegionType.Set_Passable, false) as Hive;
		}

	}
}
