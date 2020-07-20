using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PerfectlyGenericItem
{
    class PGICompRemover : ThingComp 
    {
        public override void CompTickRare()
        {
            base.CompTickRare();
            if (PGISettings.Instance.removePGI)
            {
                parent.DeSpawn();
            }
        }
    }
}
