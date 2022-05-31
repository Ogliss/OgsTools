using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace ExtraApparelLayers
{
    public class EasyApparelLayers_Main : Mod
    {
        public EasyApparelLayers_Main(ModContentPack content) : base(content)
        {
            settings = GetSettings<EasyApparelLayers_Settings>();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        //    if (settings.overheadYSpacePatch)  harmony.Patch(AccessTools.GetDeclaredMethods(typeof(PawnRenderer)).First((MethodInfo mi) => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.Name.Contains("DrawHeadHair")), null, null, new HarmonyMethod(typeof(EasyApparelLayers_Main), "DrawHeadHairApparelTranspiler", null), null);
            shellApparelDefs = new List<ThingDef>();
            shellApparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsApparel && LastLayerShellOrHigher(x.apparel.LastLayer, ApparelLayerDefOf.Shell) && !x.apparel.LastLayer.IsUtilityLayer);
            overheadApparelDefs = new List<ThingDef>();
            overheadApparelDefs = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.IsApparel && LastLayerOverheadOrHigher(x.apparel.LastLayer, ApparelLayerDefOf.Shell) && !x.apparel.LastLayer.IsUtilityLayer);

        }

        #region overrides
        public override string SettingsCategory() => "EEAP.Title".Translate();
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect inRect1 = inRect.TopPart(0.05f);
            Rect inRect2 = inRect.BottomPart(0.95f);
            Rect Frame = inRect2.ContractedBy(4);
            float width = inRect2.ContractedBy(4).width;

            Listing_Standard listing_Main = new Listing_Standard();
            listing_Main.Begin(Frame);
            listing_Main.ColumnWidth *= 0.488f;
            listing_Main.CheckboxLabeled("EEAP.bodyYSpacePatch".Translate(), ref settings.bodyYSpacePatch, "EEAP.bodyYSpacePatchDesc".Translate());
            listing_Main.CheckboxLabeled("EEAP.shellLastLayerPatch".Translate(), ref settings.shellLastLayerPatch, "EEAP.shellLastLayerPatchDesc".Translate());
            listing_Main.CheckboxLabeled("EEAP.shellYSpacePatch".Translate(), ref settings.shellYSpacePatch, "EEAP.shellYSpacePatchDesc".Translate());
            listing_Main.NewColumn();
            listing_Main.CheckboxLabeled("EEAP.overheadLastLayerPatch".Translate(), ref settings.overheadLastLayerPatch, "EEAP.overheadLastLayerPatchDesc".Translate());
            // listing_Main.CheckboxLabeled("EEAP.overheadYSpacePatch".Translate(), ref settings.overheadYSpacePatch, "EEAP.overheadYSpacePatchDesc".Translate());
            listing_Main.End();
            //    PostModOptions(listing_Main, inRect2, width, menu);
        }
        public override void WriteSettings()
        {
            base.WriteSettings();

        }
        #endregion

        public static IEnumerable<CodeInstruction> DrawHeadHairApparelTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo hairInfo = AccessTools.Property(typeof(PawnGraphicSet), "HairMeshSet").GetGetMethod();
            List<CodeInstruction> instructionList = instructions.ToList<CodeInstruction>();
            int num;
            for (int i = 0; i < instructionList.Count; i = num + 1)
            {
                CodeInstruction codeInstruction = instructionList[i];
                if (i + 4 < instructionList.Count && instructionList[i + 2].OperandIs(hairInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2, null)
                    {
                        labels = codeInstruction.ExtractLabels()
                    };
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer).GetNestedTypes(AccessTools.all)[0], "flags"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
                    yield return new CodeInstruction(OpCodes.Ldarg_2, null);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer).GetNestedTypes(AccessTools.all)[0], "headFacing"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0, null);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "graphics"));
                    //		codeInstruction = new CodeInstruction(OpCodes.Call, AccessTools.Method(HarmonyPatches.patchType, "GetPawnHairMesh", null, null));
                    i += 5;
                }
                yield return codeInstruction;
                num = i;
            }
            yield break;
        }

        public static bool LastLayerShellOrHigher(ApparelLayerDef LastLayer, ApparelLayerDef Shell = null)
        {
            if (!EasyApparelLayers_Main.settings.shellLastLayerPatch)
            {
                return LastLayer == ApparelLayerDefOf.Shell;
            }
            return LastLayer.drawOrder >= ApparelLayerDefOf.Shell.drawOrder && LastLayer.drawOrder < ApparelLayerDefOf.Overhead.drawOrder && !(LastLayer.defName.Contains("Head") || LastLayer.defName.Contains("head")) && !LastLayer.IsUtilityLayer;
        }
        public static bool LastLayerOverheadOrHigher(ApparelLayerDef LastLayer, ApparelLayerDef Overhead = null)
        {
            if (!EasyApparelLayers_Main.settings.overheadLastLayerPatch)
            {
                return LastLayer == ApparelLayerDefOf.Overhead;
            }
            return LastLayer.drawOrder >= ApparelLayerDefOf.Overhead.drawOrder && !LastLayer.IsUtilityLayer;
        }

        public static Harmony harmony = new Harmony("com.ogliss.rimworld.mod.ExtraApparelLayers");
        public static EasyApparelLayers_Main Instance;
        public static EasyApparelLayers_Settings settings;
        public static List<ThingDef> shellApparelDefs;
        public static List<ThingDef> overheadApparelDefs;

        private Vector2 pos = new Vector2(0f, 0f);
        private float menu = 0f;
    }

    public class EasyApparelLayers_Settings : ModSettings
    {
        public bool shellYSpacePatch = true;
        public bool shellLastLayerPatch = true;
        public bool bodyYSpacePatch = true;
        public bool overheadYSpacePatch = true;
        public bool overheadLastLayerPatch = true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.shellYSpacePatch, "shellYSpacePatch", true);
            Scribe_Values.Look(ref this.shellLastLayerPatch, "shellLastLayerPatch", true);
            Scribe_Values.Look(ref this.bodyYSpacePatch, "bodyYSpacePatch", true);
            Scribe_Values.Look(ref this.overheadYSpacePatch, "overheadYSpacePatch", true);
            Scribe_Values.Look(ref this.overheadLastLayerPatch, "overheadLastLayerPatch", true);

        }


    }

}
