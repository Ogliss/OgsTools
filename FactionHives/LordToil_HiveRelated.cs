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
	// Token: 0x02000796 RID: 1942
	public abstract class LordToil_HiveRelated : LordToil
	{
		// Token: 0x17000937 RID: 2359
		// (get) Token: 0x0600329C RID: 12956 RVA: 0x00119609 File Offset: 0x00117809
		private LordToil_HiveRelatedData Data
		{
			get
			{
				return (LordToil_HiveRelatedData)this.data;
			}
		}

		// Token: 0x0600329D RID: 12957 RVA: 0x00119616 File Offset: 0x00117816
		public LordToil_HiveRelated()
		{
			this.data = new LordToil_HiveRelatedData();
		}

		// Token: 0x0600329E RID: 12958 RVA: 0x00119629 File Offset: 0x00117829
		protected void FilterOutUnspawnedHives()
		{
			this.Data.assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Value == null || !x.Value.Spawned);
		}

		// Token: 0x0600329F RID: 12959 RVA: 0x0011965C File Offset: 0x0011785C
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

		// Token: 0x060032A0 RID: 12960 RVA: 0x001196A0 File Offset: 0x001178A0
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
