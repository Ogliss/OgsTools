using System;
using Verse;

namespace HunterMarkingSystem
{
    // Token: 0x02000020 RID: 32
    public class TrophyPartDefExtension : DefModExtension
    {
        public bool TrophyPart = false;
        public float TrophyChance = 0.5f;
        public bool PartHealthModifier = true;
        public ThingDef TrophyDef = null;
    }
}
