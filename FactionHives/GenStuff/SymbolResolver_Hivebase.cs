using System;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace ExtraHives.GenStuff
{
	// Token: 0x02000053 RID: 83
	internal class SymbolResolver_Hivebase : SymbolResolver
	{
		// Token: 0x0600018D RID: 397 RVA: 0x0000EE2C File Offset: 0x0000D02C
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Faction parentFaction = map.ParentFaction;
			int dist = 0;
			bool flag = rp.edgeDefenseWidth != null;
			if (flag)
			{
				dist = rp.edgeDefenseWidth.Value;
			}
			else
			{
				bool flag2 = rp.rect.Width >= 20 && rp.rect.Height >= 20 && (parentFaction.def.techLevel >= TechLevel.Industrial || Rand.Bool);
				if (flag2)
				{
					dist = (Rand.Bool ? 2 : 4);
				}
			}
			float num = (float)rp.rect.Area / 144f * 0.17f;
			ResolveParams resolveParams4 = rp;
			resolveParams4.rect = rp.rect.ContractedBy(dist);
			resolveParams4.faction = parentFaction;
			resolveParams4.floorOnlyIfTerrainSupports = new bool?(rp.floorOnlyIfTerrainSupports ?? true);
			resolveParams4.wallStuff = (rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, true));
			resolveParams4.chanceToSkipWallBlock = new float?(rp.chanceToSkipWallBlock ?? 0.1f);
			resolveParams4.clearEdificeOnly = new bool?(rp.clearEdificeOnly ?? true);
			resolveParams4.noRoof = new bool?(rp.noRoof ?? true);
			resolveParams4.chanceToSkipFloor = new float?(rp.chanceToSkipFloor ?? 0.1f);
			resolveParams4.filthDef = RimWorld.ThingDefOf.Filth_Slime;
			resolveParams4.filthDensity = new FloatRange?(new FloatRange(0.5f, 1f));
			resolveParams4.cultivatedPlantDef = null;
			BaseGen.symbolStack.Push("ExtraHives_HiveMoundMaker", resolveParams4, null);
		//	BaseGen.symbolStack.Push("basePart_outdoors", resolveParams4, null);
		//	BaseGen.symbolStack.Push("basePart_outdoors", resolveParams4, null);
			BaseGen.globalSettings.minEmptyNodes = ((num < 1f) ? 0 : GenMath.RoundRandom(num));
			Lord singlePawnLord = rp.singlePawnLord ?? LordMaker.MakeNewLord(parentFaction, new LordJob_DefendBase(parentFaction, rp.rect.CenterCell), map, null);
			TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false);
			ResolveParams resolveParams = rp;
			resolveParams.rect = rp.rect;
			resolveParams.faction = parentFaction;
			resolveParams.singlePawnLord = singlePawnLord;
			resolveParams.pawnGroupKindDef = (rp.pawnGroupKindDef ?? RimWorld.PawnGroupKindDefOf.Settlement);
			resolveParams.singlePawnSpawnCellExtraPredicate = (rp.singlePawnSpawnCellExtraPredicate ?? ((IntVec3 x) => map.reachability.CanReachMapEdge(x, traverseParms)));
			if (resolveParams.pawnGroupMakerParams == null)
			{
				resolveParams.pawnGroupMakerParams = new PawnGroupMakerParms();
				resolveParams.pawnGroupMakerParams.tile = map.Tile;
				resolveParams.pawnGroupMakerParams.faction = parentFaction;
				resolveParams.pawnGroupMakerParams.points = (rp.settlementPawnGroupPoints ?? SymbolResolver_Settlement.DefaultPawnsPoints.RandomInRange);
				resolveParams.pawnGroupMakerParams.inhabitants = true;
				resolveParams.pawnGroupMakerParams.seed = rp.settlementPawnGroupSeed;
			}
			BaseGen.symbolStack.Push("pawnGroup", resolveParams, null);
		//	BaseGen.symbolStack.Push("BETABaseLightning", rp, null);
			PawnGenerationRequest value = new PawnGenerationRequest(parentFaction.def.pawnGroupMakers.Where(x=> x.kindDef == PawnGroupKindDefOf.Hive_ExtraHives|| x.kindDef == RimWorld.PawnGroupKindDefOf.Combat).RandomElement().options.RandomElementByWeight(x=> x.Cost).kind, parentFaction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false, 1f, false, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null);
			ResolveParams resolveParams2 = rp;
			resolveParams2.faction = parentFaction;
			resolveParams2.singlePawnGenerationRequest = new PawnGenerationRequest?(value);
			resolveParams2.rect = rp.rect;
			resolveParams2.singlePawnLord = singlePawnLord;
			BaseGen.symbolStack.Push("pawn", resolveParams2, null);
			ResolveParams resolveParams3 = rp;
			resolveParams3.rect = rp.rect.ContractedBy(dist);
			resolveParams3.faction = parentFaction;
			BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams3, null);
			ResolveParams resolveParams5 = rp;
			/*
			resolveParams5.floorDef = TerrainDefOf.Bridge;
			resolveParams5.floorOnlyIfTerrainSupports = new bool?(rp.floorOnlyIfTerrainSupports ?? true);
			resolveParams5.allowBridgeOnAnyImpassableTerrain = new bool?(rp.allowBridgeOnAnyImpassableTerrain ?? true);
			resolveParams5.chanceToSkipFloor = new float?(rp.chanceToSkipFloor ?? 0.1f);
			resolveParams5.filthDensity = new FloatRange?(new FloatRange(0.5f, 1f));
			*/
		//	BaseGen.symbolStack.Push("floor", resolveParams5, null);
		//	BaseGen.symbolStack.Push("floor", resolveParams5, null);
			BaseGen.symbolStack.Push("ExtraHives_HiveRandomCorpse", rp, null);
		}

		// Token: 0x040000C5 RID: 197
		public static readonly FloatRange DefaultPawnsPoints = new FloatRange(1150f, 1600f);
	}
}
