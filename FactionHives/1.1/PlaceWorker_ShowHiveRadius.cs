using RimWorld;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// Token: 0x02001100 RID: 4352 ExtraHives.PlaceWorker_ShowHiveRadius
	public class PlaceWorker_ShowHiveRadius : PlaceWorker
	{
		// Token: 0x06006854 RID: 26708 RVA: 0x002441A0 File Offset: 0x002423A0
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
