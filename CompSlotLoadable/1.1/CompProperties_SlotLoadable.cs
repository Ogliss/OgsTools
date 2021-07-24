using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OgsCompSlotLoadable
{
    public class CompProperties_SlotLoadable : CompProperties
    {
        public bool gizmosOnEquip = true;
        public QualityCategory qualityRestriction = QualityCategory.Awful;

        public List<SlotLoadableDef> slots = new List<SlotLoadableDef>();

        public CompProperties_SlotLoadable()
        {
            compClass = typeof(CompSlotLoadable);
        }
    }
}