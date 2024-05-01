using AlienRace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AdvancedGraphics
{
    public class AlienRaceUtility
    {
		public static Vector2 AlienRacesPatch(Pawn pawn, Thing eq)
		{
			ThingDef_AlienRace alienDef = pawn.def as ThingDef_AlienRace;
			Vector2 s;
			if (alienDef != null)
			{
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
