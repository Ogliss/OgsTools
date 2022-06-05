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
            harmony.Patch(AccessTools.GetDeclaredMethods(typeof(PawnRenderer)).First((MethodInfo mi) => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.Name.Contains("DrawHeadHair") && mi.Name.Contains("DrawApparel")), null, null, new HarmonyMethod(typeof(PawnRenderer_DrawHeadHair_EasyApparelLayers_Transpiler), "CompilerGenereatedTranspiler", null), null);
 
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
            listing_Main.CheckboxLabeled("EEAP.overheadYSpacePatch".Translate(), ref settings.overheadYSpacePatch, "EEAP.overheadYSpacePatchDesc".Translate());
            listing_Main.End();
            //    PostModOptions(listing_Main, inRect2, width, menu);
        }
        public override void WriteSettings()
        {
            base.WriteSettings();

        }
        #endregion

        public static Harmony harmony = new Harmony("com.ogliss.rimworld.mod.ExtraApparelLayers");
        public static EasyApparelLayers_Main Instance;
        public static EasyApparelLayers_Settings settings;

        private Vector2 pos = new Vector2(0f, 0f);
        private float menu = 0f;
    }

    public class EasyApparelLayers_Settings : ModSettings
    {
        public bool shellYSpacePatch = true;
        public bool shellLastLayerPatch = true;
        public bool bodyYSpacePatch = true;
        public bool overheadYSpacePatch = true;
        public bool overheadInFrontOfFaceYSpacePatch = true;
        public bool overheadLastLayerPatch = true;
        public bool eyeCoverLastLayerPatch = true;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.shellYSpacePatch, "shellYSpacePatch", true);
            Scribe_Values.Look(ref this.shellLastLayerPatch, "shellLastLayerPatch", true);
            Scribe_Values.Look(ref this.bodyYSpacePatch, "bodyYSpacePatch", true);
            Scribe_Values.Look(ref this.overheadYSpacePatch, "overheadYSpacePatch", true);
            Scribe_Values.Look(ref this.overheadInFrontOfFaceYSpacePatch, "overheadInFrontOfFaceYSpacePatch", true);
            Scribe_Values.Look(ref this.overheadLastLayerPatch, "overheadLastLayerPatch", true);
            Scribe_Values.Look(ref this.eyeCoverLastLayerPatch, "eyeCoverLastLayerPatch", true);

        }


    }

}
