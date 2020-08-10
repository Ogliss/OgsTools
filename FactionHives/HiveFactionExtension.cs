using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace ExtraHives
{
    public class HiveFactionExtension : DefModExtension
	{
		public bool overrideBaseGen = false;
		public string baseGenOrride = string.Empty;
		public bool baseDamage = true;
		public bool randomHives = false;
		public IntRange sizeRange = new IntRange(44, 60);

		public ThingDef cultivatedPlantDef;
	}
}
