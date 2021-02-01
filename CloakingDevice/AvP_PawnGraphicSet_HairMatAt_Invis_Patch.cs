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

namespace CloakingDevice.HarmonyInstance
{
	// InvisibilityMatPool GetInvisibleMat

	//    [HarmonyPatch(typeof(PawnGraphicSet), "HairMatAt")]
	public static class AvP_PawnGraphicSet_HairMatAt_Invis_Patch
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
						__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).hairGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).hairGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
						return;
					}
				}
				else
				{
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


	}

	//	[HarmonyPatch(typeof(PawnGraphicSet), "HairMatAt_NewTemp")]
	public static class AvP_PawnGraphicSet_HairMatAt_NewTemp_Invis_Patch
	{
		[HarmonyPostfix]
		public static void Postfix(PawnGraphicSet __instance, Rot4 facing, ref Material __result)
		{
			Pawn pawn = __instance.pawn;
			if (pawn.IsInvisible() && pawn.RaceProps.Humanlike)
			{
				if (pawn.CarriedBy != null)
				{
					if (pawn.CarriedBy.isCloaked())
					{
						__result.SetTexture(AvPConstants.Invisiblegraphics(pawn).hairGraphic.MatSingle.name, AvPConstants.Invisiblegraphics(pawn).hairGraphic.MatSingle.mainTexture);
						__result.shader = ShaderDatabase.Cutout;
						return;
					}
				}
				else
				{
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


	}

}