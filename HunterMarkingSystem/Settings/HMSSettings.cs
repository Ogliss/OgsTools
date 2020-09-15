using HunterMarkingSystem.ExtensionMethods;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HunterMarkingSystem.Settings
{
    static internal class SettingsHelper
    {
        public static HMSSettings latest;
    }

    class HMSMod : Mod
    {
        private HMSSettings settings;
        public HMSMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<HMSSettings>();
            SettingsHelper.latest = this.settings;

        }

        public override string SettingsCategory() => "Hunter Marking System";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            this.settings.MinWorthyKill = Widgets.HorizontalSlider(inRect.TopHalf().BottomHalf().TopHalf().BottomHalf().ContractedBy(4),
                this.settings.MinWorthyKill, 0f, 3f, true,
                "HMS_MinScoreFactor".Translate(this.settings.MinWorthyKill * 100, 10 *this.settings.MinWorthyKill)
                , "0%", "300%");

            Widgets.TextFieldNumeric<float>(inRect.TopHalf().TopHalf().TopHalf().BottomHalf().LeftHalf().LeftHalf().ContractedBy(4), ref settings.MinWorthyKill, ref settings.MinWorthyKillBuffer, 0.001f, 10f);

            List<ThingDef> WorthyKillDefs = HunterMarkingSystem.RaceDefaultMarkDict.Keys.ToList();
            List<string> listed = new List<string>();
            Rect listrect = inRect.BottomHalf();
            Rect markablerect = listrect.LeftHalf();
            Rect defscorerect = listrect.RightHalf();
            Widgets.Label(inRect.TopHalf().BottomHalf().BottomHalf().BottomHalf().RightHalf().ContractedBy(4), "HMS_KillMarksScores".Translate(WorthyKillDefs.Count));
            Widgets.BeginScrollView(defscorerect, ref this.pos2, new Rect(defscorerect.x, defscorerect.y, defscorerect.width, WorthyKillDefs.Count * 22f), true);
            float num1 = defscorerect.y;
            foreach (ThingDef td in WorthyKillDefs.OrderBy(xz=> xz.label))
            {
                if (!listed.Contains(td.label))
                {
                    listed.Add(td.label);
                    MarkData markData = HunterMarkingSystem.RaceDefaultMarkDict.TryGetValue(td);
                    Widgets.Label(new Rect(defscorerect.x, num1, defscorerect.ContractedBy(4).width, 22f), "HMS_KillMarksScore".Translate(markData.Label, markData.MarkScore, markData.MarkDef.stages[0].label));
                    num1 += 22f;
                }
            }
            Widgets.EndScrollView();
            Widgets.Label(inRect.TopHalf().BottomHalf().BottomHalf().BottomHalf().LeftHalf().ContractedBy(4), "HMS_MarkableScores".Translate(HunterMarkingSystem.MarkableRaceDict.Count));
            Widgets.BeginScrollView(markablerect.ContractedBy(4), ref this.pos, new Rect(markablerect.LeftHalf().x, markablerect.LeftHalf().y, markablerect.LeftHalf().width, HunterMarkingSystem.MarkableRaceDict.Count * 22f), true);
            float num2 = markablerect.y;
            foreach (ThingDef td in HunterMarkingSystem.MarkableRaceDict.OrderBy(xz => xz.label))
            {
                MarkData markData = HunterMarkingSystem.RaceDefaultMarkDict.TryGetValue(td);
                if (markData == null)
                {
                    markData = new MarkData(td);
                    HunterMarkingSystem.RaceDefaultMarkDict.SetOrAdd(td, markData);
                }
                 Widgets.Label(new Rect(markablerect.x, num2, markablerect.ContractedBy(4).width, 22f), "HMS_MarkableScore".Translate(td.LabelCap+(td.LabelCap=="Human"? "(" + td.defName + ")" : ""), markData.MarkScore * this.settings.MinWorthyKill, td.defName));
                
                num2 += 22f;
            }

            Widgets.EndScrollView();

            /* 
        //    Widgets.CheckboxLabeled(inRect.TopHalf().TopHalf().BottomHalf().TopHalf().ContractedBy(4), "setting3: Desc", ref settings.setting3);
        //    Widgets.CheckboxLabeled(inRect.TopHalf().TopHalf().BottomHalf().BottomHalf().ContractedBy(4), "setting4: Desc", ref settings.setting4);

            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().TopHalf().TopHalf().ContractedBy(4), "setting5: Desc", ref settings.setting5);
            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().TopHalf().BottomHalf().ContractedBy(4), "setting6: Desc", ref settings.setting6);
            
            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().BottomHalf().TopHalf().ContractedBy(4), "setting7: Desc", ref settings.setting7);
            Widgets.CheckboxLabeled(inRect.TopHalf().BottomHalf().BottomHalf().BottomHalf().ContractedBy(4), "setting8: Desc", ref settings.setting8);
            */
            this.settings.Write();
        }
        private Vector2 pos = new Vector2(0f, 0f);
        private Vector2 pos2 = new Vector2(0f, 0f);

    }
    
    class HMSSettings : ModSettings
    {
        public float MinWorthyKill= 0.35f;
        public string MinWorthyKillString = string.Empty;
        public string MinWorthyKillBuffer;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.MinWorthyKill, "MinWorthyKill", 0.75f);
        }


    }
}