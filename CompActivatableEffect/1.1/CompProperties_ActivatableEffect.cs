using UnityEngine;
using Verse;

namespace OgsCompActivatableEffect
{
    public class CompProperties_ActivatableEffect : CompProperties
    {
        public string ActivateLabel;

        public SoundDef activateSound;

        public AltitudeLayer altitudeLayer;

        public bool autoActivateOnDraft = true;
        public string DeactivateLabel;
        public SoundDef deactivateSound;

        public bool draftToUseGizmos = true;
        public bool drawAboveItem = false;

        public bool gizmosOnEquip = false;
        public bool allowUnactivedUse = true;
        public GraphicData graphicData;
        public SoundDef sustainerSound;

        public string uiIconPathActivate;
        public string uiIconPathDeactivate;

        public Vector3 northOffset = new Vector3(0, 0, 0);
        public Vector3 eastOffset = new Vector3(0, 0, 0);
        public Vector3 southOffset = new Vector3(0, 0, 0);
        public Vector3 westOffset = new Vector3(0, 0, 0);
        public Vector3 northOffsetOffhand = new Vector3(0, 0, 0);
        public Vector3 eastOffsetOffhand = new Vector3(0, 0, 0);
        public Vector3 southOffsetOffhand = new Vector3(0, 0, 0);
        public Vector3 westOffsetOffhand = new Vector3(0, 0, 0);
        public bool verticalFlipOutsideCombat = false;
        public bool verticalFlipNorth = false;
        public bool isDualWeapon = false;
        public float angleAdjustmentEast = 0f;
        public float angleAdjustmentWest = 0f;
        public float angleAdjustmentNorth = 0f;
        public float angleAdjustmentSouth = 0f;
        public float angleAdjustmentEastOffhand = 0f;
        public float angleAdjustmentWestOffhand = 0f;
        public float angleAdjustmentNorthOffhand = 0f;
        public float angleAdjustmentSouthOffhand = 0f;
        public bool useAlienRacesDrawsize = false;

        public GraphicData groundGraphic = null;
        public CompProperties_ActivatableEffect()
        {
            compClass = typeof(CompActivatableEffect);
        }

        public float Altitude => Altitudes.AltitudeFor(altitudeLayer);
    }
}