using RimWorld;
using System;
using Verse;

namespace ExtraHives
{
	[DefOf]
	public static class PawnGroupKindDefOf
	{
		static PawnGroupKindDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnGroupKindDefOf));
		}
		
		public static PawnGroupKindDef Hive_ExtraHives;
		public static PawnGroupKindDef Tunneler_ExtraHives;
		
	}
}
