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
    public class Main : Mod
    {
        public Main(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.ExtraApparelLayers");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            shellApparelDefs = new List<ThingDef>();
            shellApparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsApparel && LastLayerShellOrHigher(x.apparel.LastLayer, ApparelLayerDefOf.Shell) && !x.apparel.LastLayer.IsUtilityLayer);
            overheadApparelDefs = new List<ThingDef>();
            overheadApparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsApparel && LastLayerOverheadOrHigher(x.apparel.LastLayer, ApparelLayerDefOf.Shell) && !x.apparel.LastLayer.IsUtilityLayer);

        }
        public static bool LastLayerShellOrHigher(ApparelLayerDef LastLayer, ApparelLayerDef Shell = null)
        {
            return LastLayer.drawOrder >= ApparelLayerDefOf.Shell.drawOrder && LastLayer.drawOrder < ApparelLayerDefOf.Overhead.drawOrder && !(LastLayer.defName.Contains("Head") || LastLayer.defName.Contains("head")) && !LastLayer.IsUtilityLayer;
        }
        public static bool LastLayerOverheadOrHigher(ApparelLayerDef LastLayer, ApparelLayerDef Overhead = null)
        {
            return LastLayer.drawOrder >= ApparelLayerDefOf.Overhead.drawOrder && !LastLayer.IsUtilityLayer;
        }

        public static List<ThingDef> shellApparelDefs;
        public static List<ThingDef> overheadApparelDefs;
    }
}
