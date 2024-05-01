using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace CompApparelVerbGiver.HarmonyInstance
{
	[HarmonyPatch(typeof(Pawn_MeleeVerbs), "GetUpdatedAvailableVerbsList")]
	public static class Pawn_MeleeVerbs_GetUpdatedAvailableVerbsList_Patch
	{
		public static FieldInfo pawn = typeof(Pawn_MeleeVerbs).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
		public static void Postfix(Pawn_MeleeVerbs __instance, Pawn ___pawn, bool terrainTools, List<VerbEntry> __result)
		{
			List<Verb> verbsToAdd = new List<Verb>();

			if (___pawn.apparel != null)
			{
				List<Apparel> wornApparel = ___pawn.apparel.WornApparel;
				for (int l = 0; l < wornApparel.Count; l++)
				{
					CompApparelVerbGiver comp2 = wornApparel[l].GetComp<CompApparelVerbGiver>();
					if (comp2 == null)
					{
						continue;
					}
					List<Verb> allVerbs3 = comp2.AllVerbs;
					if (allVerbs3 == null)
					{
						continue;
					}
					for (int m = 0; m < allVerbs3.Count; m++)
					{
						if (IsUsableMeleeVerb(allVerbs3[m]))
						{
							verbsToAdd.Add(allVerbs3[m]);
						}
					}
                }
                float num = 0f;
                foreach (Verb v in Pawn_MeleeVerbs.verbsToAdd)
                {
                    float num2 = VerbUtility.InitialVerbWeight(v, ___pawn);
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                foreach (Verb v in verbsToAdd)
                {
                    float num2 = VerbUtility.InitialVerbWeight(v, ___pawn);
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
                foreach (Verb verb3 in verbsToAdd)
				{
					verb3.caster = ___pawn;
					__result.Add(new VerbEntry(verb3, ___pawn, verbsToAdd, num));
				}
			}
			bool IsUsableMeleeVerb(Verb v)
			{
				if (v.IsStillUsableBy(___pawn))
				{
					return v.IsMeleeAttack;
				}
				return false;
			}
		}
		public static float InitialVerbWeight(Verb v, Pawn p)
		{
			return DPS(v, p) * AdditionalSelectionFactor(v);
		}
		public static float DPS(Verb v, Pawn p)
		{
			return v.verbProps.AdjustedMeleeDamageAmount(v, p) * (1f + v.verbProps.AdjustedArmorPenetration(v, p)) * v.verbProps.accuracyTouch / v.verbProps.AdjustedFullCycleTime(v, p);
		}
		private static float AdditionalSelectionFactor(Verb v)
		{
			float num = (v.tool != null) ? v.tool.chanceFactor : 1f;
			if (v.verbProps.meleeDamageDef != null && !v.verbProps.meleeDamageDef.additionalHediffs.NullOrEmpty<DamageDefAdditionalHediff>())
			{
				foreach (DamageDefAdditionalHediff damageDefAdditionalHediff in v.verbProps.meleeDamageDef.additionalHediffs)
				{
					num += 0.1f;
				}
			}
			return num;
		}
	}
}