using System.Collections.Generic;
using Verse;

namespace PerfectlyGenericItem
{
    public class PGISettings : ModSettings
    {
        public bool removePGI = false;
        public bool replaceFactions = false;

        public PGISettings()
        {
            PGISettings.Instance = this;
        }

        public static PGISettings Instance;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.removePGI, "PGI_remove", false);
            Scribe_Values.Look(ref this.replaceFactions, "PGI_replaceFactions", false);

        }
    }
}
