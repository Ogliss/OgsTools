using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ConfigureablePlants
{
    // ConfigureablePlants.ConfigureablePlantProperties
    public class ConfigureablePlantProperties : PlantProperties
    {
        public bool canspawn = true;
        public bool spawnwild = true;
        public FloatRange tempsOptimal = new FloatRange(10f, 42f);
        public FloatRange tempsLimits = new FloatRange(0f, 58f);
        public float optionalChance = 0.01f;
        public List<optionalThings> optionals = new List<optionalThings>();

        public struct optionalThings
        {
            public ThingDef def;
            public float weight;
        }
    }
}
