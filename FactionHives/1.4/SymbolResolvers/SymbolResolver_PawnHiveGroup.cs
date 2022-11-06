using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExtraHives.GenStuff
{
	// ExtraHives.GenStuff.SymbolResolver_PawnHiveGroup
	public class SymbolResolver_PawnHiveGroup : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			return (from x in rp.rect.Cells
					where x.Standable(BaseGen.globalSettings.map)
					select x).Any<IntVec3>();
		}

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			PawnGroupMakerParms pawnGroupMakerParms = rp.pawnGroupMakerParams;
			if (pawnGroupMakerParms == null)
			{
				pawnGroupMakerParms = new PawnGroupMakerParms();
				pawnGroupMakerParms.tile = map.Tile;
				pawnGroupMakerParms.faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
				pawnGroupMakerParms.points = 250f;
			}
			pawnGroupMakerParms.groupKind = (rp.pawnGroupKindDef ?? RimWorld.PawnGroupKindDefOf.Combat);
			List<PawnKindDef> list = new List<PawnKindDef>();
			foreach (Pawn pawn in PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true))
			{
				list.Add(pawn.kindDef);
			//	Log.Message("generating "+ pawn);
				ResolveParams resolveParams = rp;
				resolveParams.singlePawnToSpawn = pawn;
				BaseGen.symbolStack.Push("ExtraHives_Pawn", resolveParams, null);
			}
		}

		private const float DefaultPoints = 250f;
	}
}
