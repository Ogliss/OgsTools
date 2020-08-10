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
	// InvisibilityMatPool GetInvisibleMat HairMatAt_NewTemp

	//	[HarmonyPatch(typeof(PawnGraphicSet), "HeadMatAt")]
	public static class AvP_PawnGraphicSet_HeadMatAt_Invis_Patch
	{
		public static void Postfix(PawnGraphicSet __instance, ref Material __result)
		{
			Pawn pawn = __instance.pawn;
			if (pawn.IsInvisible() && pawn.RaceProps.Humanlike)
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
					}
					return;
				}

			}
		}


	}

	//	[HarmonyPatch(typeof(PawnGraphicSet), "HeadMatAt_NewTemp")]
	public static class AvP_PawnGraphicSet_HeadMatAt_NewTemp_Invis_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(PawnGraphicSet __instance, ref Material __result)
		{
			Pawn pawn = __instance.pawn;
			if (pawn.IsInvisible() && pawn.RaceProps.Humanlike)
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
					}
					return;
				}

			}
		}


	}

}