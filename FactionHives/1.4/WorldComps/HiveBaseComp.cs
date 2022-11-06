using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace ExtraHives.Planet
{
	public class HiveBaseComp : WorldObjectComp 
	{
		public int HivePhase = 1;
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref this.HivePhase, "HivePhase", 1);
		}
	}
}
