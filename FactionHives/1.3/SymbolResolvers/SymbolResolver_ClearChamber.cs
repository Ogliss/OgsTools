using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExtraHives.GenStuff
{
	// ExtraHives.GenStuff.SymbolResolver_ClearChamber
	public class SymbolResolver_ClearChamber : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			float dist = Math.Min(rp.rect.TopRight.DistanceTo(rp.rect.BottomLeft), GenRadial.MaxRadialPatternRadius);
			List<IntVec3> cells = GenRadial.RadialCellsAround(rp.rect.CenterCell, dist, true).ToList();
			foreach (IntVec3 c in cells)
			{
				if (rp.clearEdificeOnly != null && rp.clearEdificeOnly.Value)
				{
					Building edifice = c.GetEdifice(BaseGen.globalSettings.map);
					if (edifice != null && edifice.def.destroyable)
					{
						edifice.Destroy(DestroyMode.Vanish);
					}
				}
				else if (rp.clearFillageOnly != null && rp.clearFillageOnly.Value)
				{
					SymbolResolver_ClearChamber.tmpThingsToDestroy.Clear();
					SymbolResolver_ClearChamber.tmpThingsToDestroy.AddRange(c.GetThingList(BaseGen.globalSettings.map));
					for (int i = 0; i < SymbolResolver_ClearChamber.tmpThingsToDestroy.Count; i++)
					{
						if (SymbolResolver_ClearChamber.tmpThingsToDestroy[i].def.destroyable && SymbolResolver_ClearChamber.tmpThingsToDestroy[i].def.Fillage != FillCategory.None)
						{
							SymbolResolver_ClearChamber.tmpThingsToDestroy[i].Destroy(DestroyMode.Vanish);
						}
					}
				}
				else
				{
					SymbolResolver_ClearChamber.tmpThingsToDestroy.Clear();
					SymbolResolver_ClearChamber.tmpThingsToDestroy.AddRange(c.GetThingList(BaseGen.globalSettings.map));
					for (int j = 0; j < SymbolResolver_ClearChamber.tmpThingsToDestroy.Count; j++)
					{
						if (SymbolResolver_ClearChamber.tmpThingsToDestroy[j].def.destroyable)
						{
							SymbolResolver_ClearChamber.tmpThingsToDestroy[j].Destroy(DestroyMode.Vanish);
						}
					}
				}
				if (rp.clearRoof != null && rp.clearRoof.Value)
				{
					BaseGen.globalSettings.map.roofGrid.SetRoof(c, null);
				}
			}
		}

		private static List<Thing> tmpThingsToDestroy = new List<Thing>();
	}
}
