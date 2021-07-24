using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OgsCompSlotLoadable
{
    // OgsCompSlotLoadable.SlottedBonusExtension
    public class SlottedBonusExtension : DefModExtension
    {
        public List<ThingDef> additionalProjectiles = new List<ThingDef>();

        public Color color = Color.white;

        public DamageDef damageDef = null;

        public float armorPenetration = 0f;

        public SlotBonusProps_DefensiveHealChance defensiveHealChance = null;

        public float muzzleFlashMod = 0.0f;

        public ThingDef projectileReplacer = null;

        public VerbProperties verbReplacer = null;

        public SoundDef soundCastReplacer = null;
        public List<StatModifier> statModifiers = null;

        public SlotBonusProps_VampiricEffect vampiricHealChance = null;

        public float weaponRangeMod = 0.0f;
    }
}
