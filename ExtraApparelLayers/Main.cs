using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ExtraApparelLayers
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.ExtraApparelLayers");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            shellApparelDefs = new List<ThingDef>();
            shellApparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsApparel && LastLayerShellOrHigher(x.apparel.LastLayer, ApparelLayerDefOf.Shell));
        }
        public static bool LastLayerShellOrHigher(ApparelLayerDef LastLayer, ApparelLayerDef Shell)
        {
            return LastLayer.drawOrder >= Shell.drawOrder;
        }

        public static List<ThingDef> shellApparelDefs;
    }
}
