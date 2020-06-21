using RimWorld;
using System;
using Verse;

namespace AbilitesExtended
{
	public static class AbilityTracker_Extentions
	{

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
