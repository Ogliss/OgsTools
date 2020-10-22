using System;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using static Verse.DamageInfo;

namespace ExtraHives.GenStuff
{
	// Token: 0x02000050 RID: 80
	internal class SymbolResolver_RandomCorpse : SymbolResolver
	{
		// Token: 0x06000187 RID: 391 RVA: 0x0000EA24 File Offset: 0x0000CC24
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Rand.PushState();
			for (int i = 0; i < Rand.RangeInclusive(10, 25); i++)
			{
				IntVec3 randomCell = rp.rect.RandomCell;
				if (GenGrid.Standable(randomCell, map) && GridsUtility.GetFirstItem(randomCell, map) == null && GridsUtility.GetFirstPawn(randomCell, map) == null && GridsUtility.GetFirstBuilding(randomCell, map) == null)
				{
					Pawn val = PawnGenerator.GeneratePawn(PawnKindDefOf.Villager, Find.FactionManager.RandomEnemyFaction(false, false, false, (TechLevel)0));
					((Thing)val).Kill((DamageInfo?)new DamageInfo(DamageDefOf.Cut, 9999f, 0f, -1f, (Thing)null, (BodyPartRecord)null, (ThingDef)null, (SourceCategory)0, (Thing)null), (Hediff)null);
					Corpse corpse = val.Corpse;
					corpse.timeOfDeath = 10000;
					ThingCompUtility.TryGetComp<CompRottable>((Thing)(object)corpse).RotImmediately();
					GenSpawn.Spawn((Thing)(object)corpse, randomCell, map, (WipeMode)0);
					for (int j = 0; j < 5; j++)
					{
						IntVec3 val2 = default(IntVec3);
						RCellFinder.TryFindRandomCellNearWith(randomCell, (Predicate<IntVec3>)((IntVec3 ni) => GenGrid.Walkable(ni, map)), map, out val2, 1, 3);
						GenSpawn.Spawn(RimWorld.ThingDefOf.Filth_CorpseBile, val2, map, (WipeMode)0);
					}
				}
			}
			Rand.PopState();
		}
	}
}
