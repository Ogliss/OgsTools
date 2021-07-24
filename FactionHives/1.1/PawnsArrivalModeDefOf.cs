using RimWorld;
using System;

namespace ExtraHives
{
	[DefOf]
	public static class PawnsArrivalModeDefOf
	{
		static PawnsArrivalModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PawnsArrivalModeDefOf));
		}

		public static PawnsArrivalModeDef EdgeWalkInGroups;
		public static PawnsArrivalModeDef EdgeDropGroups;
		public static PawnsArrivalModeDef EdgeTunnelIn_ExtraHives;
		public static PawnsArrivalModeDef EdgeTunnelInGroups_ExtraHives;
		public static PawnsArrivalModeDef CenterTunnelIn_ExtraHives;
		public static PawnsArrivalModeDef RandomTunnelIn_ExtraHives;
	}
}
