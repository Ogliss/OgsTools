using RimWorld;
using System;
using Verse;

namespace ExtraHives
{
	[DefOf]
	public static class ThingDefOf
	{
		static ThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));

		public static ThingDef Tunneler_ExtraHives;
		public static ThingDef InfestedMeteoriteIncoming_ExtraHives; 
		
	}
}
