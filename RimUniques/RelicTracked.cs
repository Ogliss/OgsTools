using Verse;

namespace RimUniques
{
    public class RelicTracked : IExposable
    {
        public RelicTracked()
        {

        }
        public RelicTracked(ThingDef relicDef)
        {
            if (relicDef.GetModExtension<RelicExtension>() is RelicExtension ext)
            {
                this.maxCount = ext.relicProps.maxCount;
                this.reacquirable = ext.relicProps.reacquirable;
                this.compensate = ext.relicProps.compensate;
                this.compensateRate = ext.relicProps.compensateRate;
            }
        }
        private int curSpawned;
        public int maxCount;
        public bool reacquirable;
        public bool compensate;
        public float compensateRate;
        public void ExposeData()
        {
            Scribe_Values.Look(ref maxCount, "maxSpawnableCount", 1);
            Scribe_Values.Look(ref curSpawned, "curSpawnedCount", 0);
            Scribe_Values.Look(ref reacquirable, "reacquirableRelic", false);
            Scribe_Values.Look(ref compensate, "compensate", false);
            Scribe_Values.Look(ref compensateRate, "compensateRate", 0.75f);
        }
        public bool CanSpawn
        {
            get
            {
                return curSpawned < maxCount;
            }
        }
        public void SpawnedRelic()
        {
            curSpawned++;
        }
        public void DespawnedRelic()
        {
            if (this.reacquirable)
            {
                curSpawned--;
            }
        }
    }

}