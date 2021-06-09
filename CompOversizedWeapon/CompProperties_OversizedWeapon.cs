using UnityEngine;
using Verse;

namespace OgsCompOversizedWeapon
{
    public class CompProperties_OversizedWeapon : CompProperties
    {

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

        public CompProperties_OversizedWeapon()
        {
            compClass = typeof(CompOversizedWeapon);
        }
    }
}
