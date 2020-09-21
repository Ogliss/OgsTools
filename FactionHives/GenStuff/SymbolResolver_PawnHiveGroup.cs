using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExtraHives.GenStuff
{
    // Token: 0x02001155 RID: 4437
    public class SymbolResolver_PawnHiveGroup : SymbolResolver
	{
		// Token: 0x060069B6 RID: 27062 RVA: 0x0024F9E4 File Offset: 0x0024DBE4
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

		// Token: 0x060069B7 RID: 27063 RVA: 0x0024FA38 File Offset: 0x0024DC38
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

		// Token: 0x040040C6 RID: 16582
		private const float DefaultPoints = 250f;
	}
}
