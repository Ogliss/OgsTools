using RimWorld;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// ExtraHives.PlaceWorker_ShowHiveRadius
	public class PlaceWorker_ShowHiveRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (thing == null)
			{
				return;
			}
			CompSpawnerHives compPlantHarmRadius = thing.TryGetComp<CompSpawnerHives>();
			if (compPlantHarmRadius == null)
			{
				return;
			}
			if (compPlantHarmRadius.Props.radiusPerDayCurve.EnumerableNullOrEmpty())
			{
				return;
			}
			float currentRadius = compPlantHarmRadius.CurrentRadius;
			if (currentRadius < 50f)
			{
				GenDraw.DrawRadiusRing(center, currentRadius);
			}
		}
	}
}
