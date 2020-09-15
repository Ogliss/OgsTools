using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using Verse.Profile;
using Verse.Sound;

namespace ExtraHives
{
	// Token: 0x0200033C RID: 828
	public static class DebugActionsMisc
	{
		[DebugAction("General", "Increment time 90 day", allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void IncrementTime1Day()
		{
			Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + (60000 * 90));
		}
	}
}
