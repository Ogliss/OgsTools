using System;
using ExtraHives.ExtensionMethods;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace ExtraHives.GenStuff
{
	// ExtraHives.GenStuff.SymbolResolver_RandomHives
	internal class SymbolResolver_RandomHives : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			Rand.PushState();
			for (int i = 0; i < Rand.RangeInclusive(5, 20); i++)
			{
				Faction faction = rp.faction;
				IntVec3 randomCell = rp.rect.RandomCell;
				bool flag = randomCell.Standable(map) && randomCell.GetFirstItem(map) == null && randomCell.GetFirstPawn(map) == null && randomCell.GetFirstBuilding(map) == null;
				if (flag)
				{
					bool flag2 = Rand.RangeInclusive(1, 4) < 3;
					if (flag2)
					{
						ThingDef hivedef = faction.HivedefsFor().RandomElement() ?? RimWorld.ThingDefOf.Hive;
						Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(hivedef, null), randomCell, map, WipeMode.Vanish);
						hive.SetFaction(faction, null);
						foreach (CompSpawner compSpawner in hive.GetComps<CompSpawner>())
						{
							if (compSpawner.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
							{
								compSpawner.TryDoSpawn();
								break;
							}
						}
					}
					else
					{
						ThingDef hivedef = faction.HivedefsFor().RandomElement() ?? RimWorld.ThingDefOf.Hive;
						Hive hive2 = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(hivedef, null), randomCell, map, WipeMode.Vanish);
						hive2.SetFaction(faction, null);
						foreach (CompSpawner compSpawner2 in hive2.GetComps<CompSpawner>())
						{
							if (compSpawner2.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
							{
								compSpawner2.TryDoSpawn();
								break;
							}
						}
					}
				}
			}
			Rand.PopState();
		}
	}
}
