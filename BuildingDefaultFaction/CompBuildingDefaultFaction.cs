using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace BuildingDefaultFaction
{
    // BuildingDefaultFaction.CompProperties_BuildingDefaultFaction
    public class CompProperties_BuildingDefaultFaction : CompProperties
    {
        public FactionDef faction;

        public bool debug = false;
        public CompProperties_BuildingDefaultFaction()
        {
            compClass = typeof(CompBuildingDefaultFaction);
        }
    }
    // BuildingDefaultFaction.CompBuildingDefaultFaction
    public class CompBuildingDefaultFaction : ThingComp
    {
        public CompProperties_BuildingDefaultFaction Props => props as CompProperties_BuildingDefaultFaction;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (Props.debug)
            {
                Log.Message("Debug: CompBuildingDefaultFaction spawned on " + this.parent);
            }
            if ((parent as Building) != null && !respawningAfterLoad)
            {
                if (Props.debug)
                {
                    Log.Message("Debug: "+this.parent+" is Building");
                }
                Faction f = Find.FactionManager.FirstFactionOfDef(Props.faction);
                if (f != null)
                {
                    if (Props.debug)
                    {
                        Log.Message("Debug: " + this.parent + " Faction should be " + f.Name);
                    }
                    parent.SetFaction(f);
                    if (Props.debug)
                    {
                        Log.Message("Debug: " + this.parent + " Faction is " + parent.Faction.Name);
                    }
                }
            }
        }
    }
}
