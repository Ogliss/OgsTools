using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AnimatedProjectile
{
    public class AnimatedProjectileProperties : ProjectileProperties
    {
        public int ticksPerFrame = 15;
        public bool growerDistance = false;
        public float growerStartSize = 1f;
        public float growerEndSize = 1f;
        public float rotation = 0f;
        public string drawGlowMote = string.Empty;
        public bool drawGlow = false;
        public float drawGlowSizeFactor = 6f;
        public bool setsFire = false;
        public float setFireChance = 0.75f;
        public bool ignites = false;
        public float igniteChance = 0.25f;
    }
}
