﻿using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
    public class TunnelExtension : DefModExtension
    {
        public FactionDef Faction;
        public ThingDef HiveDef;
        public List<PawnKindDef> PawnKinds = new List<PawnKindDef>();
        public List<PawnKindDef> AlwaysSpawnWith = new List<PawnKindDef>();
        public bool AlwaysSpawnWithGuards = false;
        public bool strikespreexplode = false;
        public bool explodesprespawn = false;
        public float spawnWavePoints = 0f;
        public DamageDef damageDef;
        public float blastradius = 2f;
    }
}
