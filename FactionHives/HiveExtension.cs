﻿using RimWorld;
using Verse;

namespace ExtraHives
{
    public class HiveExtension : DefModExtension
	{
		public FactionDef Faction;
		public ThingDef TunnelDef;

		public bool mustBeNearColony = true;
		public float maxColonyDistance = 30f;
		public float minColonyDistance = 0f;

		public bool requiresRoofed = true;
		public bool requiresUnroofed = false;
		public bool mustBeThickRoof = true;
		public bool mustBeVisable = true;
		public bool mustBeWalkable = true;

		public int minClosedAreaSize = 2;

		public float minMountainouseness = 0.17f;
		public float? minTemp = null;
		public float? maxTemp = null;
		public float? bonusTempScore = null;
		public float? bonusAboveTemp = null;
		public float? bonusBelowTemp = null;
	}
}
