using AlienRace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace OgsCompActivatableEffect
{
	public class AlienRaceUtility
	{
		public static Vector2 AlienRacesPatch(Pawn pawn, Thing eq)
		{
			AlienRace.ThingDef_AlienRace alienDef = pawn.def as AlienRace.ThingDef_AlienRace;
			Vector2 s;
			if (alienDef != null)
			{

				//	AlienRace.GraphicPaths paths = alienDef.alienRace.graphicPaths.GetCurrentGraphicPath(pawn.ageTracker.CurLifeStage);
				//	Log.Message(pawn.Name + " is Alien");
				s = alienDef.alienRace.generalSettings.alienPartGenerator.customDrawSize;
			}
			else
			{

				s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
			}
			return s;
		}

	}
}
