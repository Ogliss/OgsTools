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
using CloakingDevice.settings;
using CloakingDevice.ExtensionMethods;

namespace CloakingDevice.HarmonyInstance
{
	// InvisibilityMatPool GetInvisibleMat

	//    [HarmonyPatch(typeof(PawnRenderer), "OverrideMaterialIfNeeded")]
	public static class AvP_PawnRenderer_OverrideMaterialIfNeeded_Xenomorph_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(PawnRenderer __instance, Material original, Pawn pawn, ref Material __result)
		{
			if (pawn.IsInvisible())
			{
				if (pawn.CarriedBy != null)
				{
					if (pawn.CarriedBy.isXenomorph())
					{
						__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
						return;
					}
					if (pawn.CarriedBy.isCloaked())
					{
						if (pawn.CarriedBy.Faction == Faction.OfPlayer)
						{
							__result.SetTexture(AvPConstants.CloakNoiseTex, TexGame.RippleTex);
							__result.color = AvPConstants.YautjaCloakColor;
						}
						else
						{
							__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.mainTexture);
							__result.shader = ShaderDatabase.Cutout;
						}
						return;
					}
				}
				else
				{
					if (pawn.isXenomorph())
					{
						__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
					}
					if (pawn.isCloaked())
					{
						if (pawn.Faction == Faction.OfPlayer)
						{
							__result.SetTexture(AvPConstants.CloakNoiseTex, TexGame.RippleTex);
							__result.color = AvPConstants.YautjaCloakColor;
						}
						else
						{
							__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.mainTexture);
							__result.shader = ShaderDatabase.Cutout;
						}
						return;
					}
				}

			}
		}

		// To
	}

	//    [HarmonyPatch(typeof(PawnRenderer), "OverrideMaterialIfNeeded_NewTemp")]
	public static class AvP_PawnRenderer_OverrideMaterialIfNeeded_NewTemp_Xenomorph_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(PawnRenderer __instance, Material original, Pawn pawn, ref Material __result)
		{
			if (pawn.IsInvisible())
			{
				if (pawn.CarriedBy != null)
				{
					if (pawn.CarriedBy.isXenomorph())
					{
						__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
						return;
					}
					if (pawn.CarriedBy.isCloaked())
					{
						if (pawn.CarriedBy.Faction == Faction.OfPlayer)
						{
							__result.SetTexture(AvPConstants.CloakNoiseTex, TexGame.RippleTex);
							__result.color = AvPConstants.YautjaCloakColor;
						}
						else
						{
							__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.mainTexture);
							__result.shader = ShaderDatabase.Cutout;
						}
						return;
					}
				}
				else
				{
					if (pawn.isXenomorph())
					{
						__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).nakedGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
					}
					if (pawn.isCloaked())
					{
						if (pawn.Faction == Faction.OfPlayer)
						{
							__result.SetTexture(AvPConstants.CloakNoiseTex, TexGame.RippleTex);
							__result.color = AvPConstants.YautjaCloakColor;
						}
						else
						{
							__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).headGraphic.MatSingle.mainTexture);
							__result.shader = ShaderDatabase.Cutout;
						}
						return;
					}
				}

			}
		}

		// To
	}

}