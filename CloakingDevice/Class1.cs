using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CloakingDevice
{
    [StaticConstructorOnStartup]
    internal class Gizmo_CompCloakGenerator_Status : Gizmo
    {
        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(1221392, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect2;
                Rect rect = rect2 = overRect.AtZero().ContractedBy(6f);
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, Translator.Translate("AvP_CloakGenerator_Energy"));
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = this.Energy / this.TotalEnergy;
                Widgets.FillableBar(rect3, fillPercent, Gizmo_CompCloakGenerator_Status.FullShieldBarTex, Gizmo_CompCloakGenerator_Status.EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, (this.Energy * 100f).ToString("F0") + " / " + (this.TotalEnergy * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }

        public string Label
        {
            get
            {
                string str = this.compCloak.Apparel.def.LabelCap;
                foreach (var item in Generators.Where(x => x != compCloak))
                {
                    str += "\n" + item.Apparel.def.LabelCap;
                }
                return str;
            }
        }

        public float Energy
        {
            get
            {
                float r = 0f;
                for (int i = 0; i < Generators.Count; i++)
                {
                    CompCloakGenerator gen = Generators[i] as CompCloakGenerator;
                    r += gen.Energy;
                }
                return r;
            }
        }

        public float TotalEnergy
        {
            get
            {
                float r = 0f;
                for (int i = 0; i < Generators.Count; i++)
                {
                    CompCloakGenerator gen = Generators[i] as CompCloakGenerator;
                    r += gen.Apparel.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
                }
                return r;
            }
        }
        public Pawn Pawn;
        public List<CompCloakGenerator> Generators
        {
            get
            {
                List<CompCloakGenerator> generatorComps = new List<CompCloakGenerator>();
                if (Pawn != null)
                {
                    List<Apparel> generators = Pawn.apparel.WornApparel.FindAll(x => x.TryGetComp<CompCloakGenerator>() != null);
                    foreach (Apparel item in generators)
                    {
                        generatorComps.Add(item.TryGetComp<CompCloakGenerator>());
                    }
                }
                return generatorComps;
            }
        }

        public CompCloakGenerator compCloak;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.6f, 0.4f));

        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}
