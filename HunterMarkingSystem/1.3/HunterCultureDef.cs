using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
    // Token: 0x0200029B RID: 667
    public class HunterCultureDef : Def
    {
        // Token: 0x06000B5D RID: 2909 RVA: 0x0005A917 File Offset: 0x00058D17
        public static HunterCultureDef Named(string defName)
        {
            return DefDatabase<HunterCultureDef>.GetNamed(defName, true);
        }
        
        
        [MustTranslate]
        public string pawnSingular = "member";
        
        [MustTranslate]
        public string pawnsPlural = "members";
        
        public string leaderTitle = "leader";
        
        [NoTranslate]
        public List<string> UnbloodedbackstoryCategories = new List<string>();
        [NoTranslate]
        public List<string> BloodedbackstoryCategories = new List<string>();

        [NoTranslate]
        public List<string> hairTags = new List<string>();
        public HediffDef UnbloodedHediff;
        public HediffDef UnmarkedHediff;
        public HediffDef MarkedHediff;

        public bool AllowMechanicalMarking = false;
        public List<ThingDef> markerRaceDefs = new List<ThingDef>();
        public List<FactionDef> markerFactionDefs = new List<FactionDef>();
        public List<ThingDef> allowedRaceDefs = new List<ThingDef>();
        public List<FactionDef> allowedFactionDefs = new List<FactionDef>();
        public List<ThingDef> disallowedRaceDefs = new List<ThingDef>();
        public List<FactionDef> disallowedFactionDefs = new List<FactionDef>();
    }
}
