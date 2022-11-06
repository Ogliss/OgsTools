﻿using System;
using RimWorld.BaseGen;
using Verse;

namespace ExtraHives.GenStuff
{
	// ExtraHives.GenStuff.SymbolResolver_RandomDamage
	internal class SymbolResolver_RandomDamage : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			map.listerThings.AllThings.FindAll((Thing t1) => t1.Faction != rp.faction).ForEach(delegate (Thing t)
			{
				Rand.PushState();
				t.HitPoints -= t.HitPoints / Rand.RangeInclusive(3, 10);
				Rand.PopState();
			});
			map.listerThings.AllThings.FindAll((Thing t2) => t2.def.IsMeat || t2.def.defName == "Pemmican").ForEach(delegate (Thing t)
			{
				t.DeSpawn(DestroyMode.Vanish);
			});
		}
	}
}
