using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ExtraHives
{
    [StaticConstructorOnStartup]
    class Main
    {
        public static bool CrashedShipsExtension = false;
        public static List<ThingDef> HiveDefs = new List<ThingDef>();
        public static List<ThingDef> TunnelDefs = new List<ThingDef>();
        static Main()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                foreach (Assembly assembly in p.assemblies.loadedAssemblies)
                {
                    Type type = assembly.GetType("CrashedShipsExtension.IncidentWorker_CrashedShip");
                    if (type != null)
                    {
                        CrashedShipsExtension = true;
                        break;
                    }
                }
                if (CrashedShipsExtension)
                {
                //    Log.Message("CrashedShipsExtension Loaded");
                    break;
                }
            }
            HiveDefs = DefDatabase<ThingDef>.AllDefs.Where(x=> x.HasModExtension<ExtraHives.HiveDefExtension>()).ToList();
            TunnelDefs = DefDatabase<ThingDef>.AllDefs.Where(x=> x.HasModExtension<ExtraHives.TunnelExtension>()).ToList();
        //    Log.Message("ExtraHives: loaded "+HiveDefs.Count + " HiveDefs and " + TunnelDefs.Count + " TunnelDefs");
        }

    }
}
