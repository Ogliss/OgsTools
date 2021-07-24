using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimUniques
{
    public class RelicTracker : WorldComponent
    {
        public RelicTracker(World world) : base(world)
        {
            this.world = world;
        }

        public Dictionary<ThingDef, RelicTracked> spawnedRelics = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasModExtension<RelicExtension>()).ToDictionary(p => p, p => new RelicTracked());

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!spawnedRelics.EnumerableNullOrEmpty())
                {
                    spawnedRelics.RemoveAll(x => x.Key == null);
                    List<ThingDef> l = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.HasModExtension<RelicExtension>() && !spawnedRelics.ContainsKey(x)).ToList();
                    for (int i = 0; i < l.Count; i++)
                    {
                        spawnedRelics.Add(l[i], new RelicTracked());
                    }
                }
            }
            Scribe_Collections.Look<ThingDef, RelicTracked>(ref this.spawnedRelics, "spawnedRelics");
            base.ExposeData();
        }

        public bool CanSpawn(Thing thing)
        {
            return CanSpawn(thing.def);
        }

        public bool CanSpawn(Thing thing, out RelicTracked data)
        {
            return CanSpawn(thing.def, out data);
        }

        public bool CanSpawn(ThingDef def)
        {
            return CanSpawn(def, out RelicTracked data);
        }

        public bool CanSpawn(ThingDef def, out RelicTracked data)
        {
            if (spawnedRelics.TryGetValue(def, out data))
            {
                return !data.CanSpawn;
            }
            return true;
        }
    }

}