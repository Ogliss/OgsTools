using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse.AI.Group;

namespace Verse
{
    
    public static class MetamorphasisToolsPawn
    {
        [DebugAction("Pawns", null, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void AgePawn10Years(Pawn p)
        {
            p.ageTracker.AgeBiologicalTicks = p.ageTracker.AgeBiologicalTicks+ 36000000;
            p.ageTracker.AgeChronologicalTicks = p.ageTracker.AgeChronologicalTicks + 36000000;
        }

    }
    
}
