using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ExtraApparelLayers
{
    public static class ApparelLayerUtility
    {
        public static List<ApparelLayerDef> shellLayers = new List<ApparelLayerDef>();
        public static List<ApparelLayerDef> overheadLayers = new List<ApparelLayerDef>();
        public static List<ApparelLayerDef> eyeCoverLayers = new List<ApparelLayerDef>();
        public static ApparelLayerDef topShell = null;
        public static ApparelLayerDef bottomShell = null;
        public static ApparelLayerDef topOverhead = null;
        public static ApparelLayerDef bottomOverhead = null;
        public static List<ApparelLayerDef> bodyLayers = new List<ApparelLayerDef>();

        public static float headgearYSpace = PawnRenderer.YOffset_OnHead - (PawnRenderer.YOffset_Head +0.001f); //tatoo
        public static float shellYSpace = PawnRenderer.YOffset_Head - PawnRenderer.YOffset_Shell;
        public static float bodyYSpace = PawnRenderer.YOffset_Shell - (PawnRenderer.YOffset_Body + 0.0014478763f); //tatoo  0.0101351343f
        static ApparelLayerUtility()
        {
            shellLayers = DefDatabase<ApparelLayerDef>.AllDefsListForReading.FindAll(x => x.defName.Contains("Shell")).OrderBy(x => x.drawOrder).ToList();
            overheadLayers = DefDatabase<ApparelLayerDef>.AllDefsListForReading.FindAll(x => x.defName.Contains("Overhead")).OrderBy(x => x.drawOrder).ToList();
            topShell = shellLayers.Last();
            bottomShell = shellLayers.First();
            topOverhead = overheadLayers.Last();
            bottomOverhead = overheadLayers.First();

            shellApparelDefs = new List<ThingDef>();
            shellApparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsApparel && LastLayerShell(x.apparel.LastLayer, ApparelLayerDefOf.Shell) && !x.apparel.LastLayer.IsUtilityLayer);

            overheadApparelDefs = new List<ThingDef>();
            overheadApparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsApparel && LastLayerOverhead(x.apparel.LastLayer, ApparelLayerDefOf.Shell) && !x.apparel.LastLayer.IsUtilityLayer);
            bodyLayers = DefDatabase<ApparelLayerDef>.AllDefsListForReading.FindAll(x => x.drawOrder < bottomShell.drawOrder).OrderBy(x => x.drawOrder).ToList();
            /*
            Log.Message($"bodyLayers: {bodyLayers.Count}, shellLayers: {shellLayers.Count}, overheadLayers: {overheadLayers.Count}");
            Log.Message($"Shell: {bottomShell}-{topShell}, Overhead: {bottomOverhead}-{topOverhead}");
            Log.Message($"bodyYSpace: {bodyYSpace}, shellYSpace: {shellYSpace}, headgearYSpace: {headgearYSpace}");
            Log.Message($"shellApparelDefs: {shellApparelDefs.Count}, overheadApparelDefs: {overheadApparelDefs.Count}");
            foreach (var item in overheadApparelDefs)
            {
                Log.Message($"Def: {item.LabelCap}, LastLayer: {item.apparel.LastLayer}");
            }
            */
        }

        public static List<ApparelGraphicRecord> apparelGraphicRecordsOrdered(List<ApparelGraphicRecord> list)
        {
        //    Log.Message($"returning {list.Count} appareal, in draw order of last layer");
            return list.OrderBy(x => x.sourceApparel.def.apparel.LastLayer.drawOrder).ToList();
        }


        public static bool LastLayer(ApparelLayerDef LastLayer, ApparelLayerDef layer)
        {
            bool result = LastLayer == layer;
            if (!result)
            {
                if (layer == ApparelLayerDefOf.Shell)
                {
                    result = shellLayers.Contains(LastLayer);
                }
                else if (layer == ApparelLayerDefOf.Overhead)
                {
                    result = overheadLayers.Contains(LastLayer);
                }
                /*
                else if (EasyApparelLayers_Main.settings.eyeCoverLastLayerPatch && layer == ApparelLayerDefOf.EyeCover)
                {
                    result = eyeCoverLayers.Contains(LastLayer);
                }
                else if (EasyApparelLayers_Main.settings.uilityLastLayerPatch && layer == ApparelLayerDefOf.EyeCover)
                {
                    result = uilityLayers.Contains(LastLayer);
                }
                */
                else Log.Warning($"Unexspected ApparelLayerDef: {layer}");
            }
            return result;
        }

        public static bool LastLayerShell(ApparelLayerDef LastLayer, ApparelLayerDef Shell = null)
        {
            if (!EasyApparelLayers_Main.settings.shellLastLayerPatch)
            {
                return LastLayer == ApparelLayerDefOf.Shell;
            }
            return shellLayers.Contains(LastLayer);// ((LastLayer.drawOrder >= bottomShell.drawOrder && LastLayer.drawOrder <= topShell.drawOrder)) || LastLayer.defName.Contains("Shell") && !(LastLayer.defName.Contains("Head") || LastLayer.defName.Contains("head") || LastLayer.defName.Contains("Belt")) && !LastLayer.IsUtilityLayer;
        }

        public static bool LastLayerOverhead(ApparelLayerDef LastLayer, ApparelLayerDef Shell = null)
        {
            if (!EasyApparelLayers_Main.settings.overheadLastLayerPatch)
            {
                return LastLayer == ApparelLayerDefOf.Overhead;
            }
            return overheadLayers.Contains(LastLayer);// (LastLayer.drawOrder >= bottomOverhead.drawOrder && LastLayer.drawOrder <= topOverhead.drawOrder) && !(LastLayer.defName.Contains("Shell") || LastLayer.defName.Contains("Belt")) && !LastLayer.IsUtilityLayer;
        }

        public static bool LastLayerShellOrHigher(ApparelLayerDef LastLayer, ApparelLayerDef Shell = null)
        {
            if (!EasyApparelLayers_Main.settings.shellLastLayerPatch)
            {
                return LastLayer == ApparelLayerDefOf.Shell;
            }
            return (LastLayer.drawOrder >= bottomShell.drawOrder) && !LastLayer.defName.Contains("Belt") && !LastLayer.IsUtilityLayer;
        }

        public static bool LastLayerOverheadOrHigher(ApparelLayerDef LastLayer, ApparelLayerDef Overhead = null)
        {
            if (!EasyApparelLayers_Main.settings.overheadLastLayerPatch)
            {
                return LastLayer == ApparelLayerDefOf.Overhead;
            }
            return LastLayer.drawOrder >= bottomOverhead.drawOrder && !LastLayer.IsUtilityLayer && !LastLayer.defName.Contains("Belt");
        }

        public static List<ThingDef> shellApparelDefs;
        public static List<ThingDef> overheadApparelDefs;

    }
}
