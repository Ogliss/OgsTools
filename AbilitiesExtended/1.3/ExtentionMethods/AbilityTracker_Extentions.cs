using RimWorld;
using System;
using System.Linq;
using Verse;

namespace AbilitesExtended
{
	public static class AbilityTracker_Extentions
	{

        public static bool isPsyker(Pawn pawn)
        {
            return isPsyker(pawn, out int Level);
        }

        public static bool isPsyker(Pawn pawn, out int Level)
        {
            return isPsyker(pawn, out Level, out float Mult);
        }

        public static bool isPsyker(Pawn pawn, out int Level, out float Mult)
        {
            bool result = false;
            Mult = 0f;
            Level = 0;

            if (pawn.RaceProps.Humanlike)
            {
                if (pawn.health.hediffSet.hediffs.Any(x => x.GetType() == typeof(Hediff_Level)))
                {
                    Level = (pawn.health.hediffSet.hediffs.First(x => x.GetType() == typeof(Hediff_Level)) as Hediff_Level).level;
                    result = true;
                }
                else
                if (pawn.story.traits.HasTrait(TraitDefOf.PsychicSensitivity))
                {
                    result = pawn.story.traits.DegreeOfTrait(TraitDefOf.PsychicSensitivity) > 0;
                    Level = pawn.story.traits.DegreeOfTrait(TraitDefOf.PsychicSensitivity);
                }
                else
                {
                    TraitDef Corruptionpsyker = DefDatabase<TraitDef>.GetNamedSilentFail("Psyker");
                    if (Corruptionpsyker != null)
                    {
                        result = true;
                        pawn.story.traits.HasTrait(Corruptionpsyker);
                        Level = pawn.story.traits.DegreeOfTrait(Corruptionpsyker);
                    }
                }
                Mult = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * (pawn.needs.mood.CurInstantLevelPercentage - pawn.health.hediffSet.PainTotal);
            }
            /*
            else
            {
                ToolUserPskyerDefExtension extension = null;
                if (pawn.def.HasModExtension<ToolUserPskyerDefExtension>())
                {
                    extension = pawn.def.GetModExtension<ToolUserPskyerDefExtension>();
                }
                else
                if (pawn.kindDef.HasModExtension<ToolUserPskyerDefExtension>())
                {
                    extension = pawn.kindDef.GetModExtension<ToolUserPskyerDefExtension>();
                }
                if (extension != null)
                {
                    result = true;
                    Level = extension.Level;
                }
                if (pawn.needs != null && pawn.needs.mood != null)
                {
                    Mult = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * (pawn.needs.mood.CurInstantLevelPercentage - pawn.health.hediffSet.PainTotal);
                }
                else
                {
                    Mult = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * (1 - pawn.health.hediffSet.PainTotal);
                }
            }
            */
            return result;
        }

        public static void TryGainEquipmentAbility(this Pawn_AbilityTracker tracker, AbilityDef abilityDef, ThingWithComps thing)
		{
            if (abilityDef is EquipmentAbilityDef def && (!def.requirePsyker || isPsyker(tracker.pawn)))
            {
                EquipmentAbility ab = tracker.abilities.FirstOrFallback(x => x.def == def && x is EquipmentAbility y && y.sourceEquipment == thing) as EquipmentAbility;
                if (ab == null)
                {
                    ab = Activator.CreateInstance(def.abilityClass, new object[]
                    {
                    tracker.pawn,
                    def,
                    thing
                    }) as EquipmentAbility;
                    ab.sourceEquipment = thing;
                    tracker.abilities.Add(ab);
                    tracker.Notify_TemporaryAbilitiesChanged();
                }
            }
		}

		public static void TryRemoveEquipmentAbility(this Pawn_AbilityTracker tracker, AbilityDef def, ThingWithComps thing)
		{
            if (!(def is EquipmentAbilityDef))
            {
				return;
            }
			EquipmentAbility ab = tracker.abilities.FirstOrFallback(x=> x.def == def && x is EquipmentAbility y && y.sourceEquipment == thing) as EquipmentAbility;
            if (ab != null)
            {
				tracker.abilities.Remove(ab);
				tracker.Notify_TemporaryAbilitiesChanged();
			}
		}

		public static void GainAbility(this Pawn_AbilityTracker tracker, AbilityDef def, Thing source)
		{
			if (!tracker.abilities.Any((Ability a) => a.def == def))
			{
				tracker.abilities.Add(Activator.CreateInstance(def.abilityClass, new object[]
				{
					tracker.pawn,
					def,
					source
				}) as Ability);
			}
		}

	}
}
