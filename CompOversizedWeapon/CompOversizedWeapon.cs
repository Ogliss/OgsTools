using HarmonyLib;
using System;
using System.Linq;
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

    public class CompOversizedWeapon : ThingComp
    {
        public CompProperties_OversizedWeapon Props => props as CompProperties_OversizedWeapon;

        public CompOversizedWeapon()
        {
            if (!(props is CompProperties_OversizedWeapon))
                props = new CompProperties_OversizedWeapon();
        }

        private bool initComps = false;
        private CompEquippable compEquippable;
        private Func<bool> compDeflectorIsAnimatingNow;
        private Func<bool> compActivatableEffectIsActiveNow;
        public Quaternion renderAngle;
        public Vector3 renderPos;
        public Quaternion renderAngleDual;
        public Vector3 renderPosDual;
        public Vector2 drawScale;


        private void InitCompsAsNeeded()
        {
            if (!initComps)
            {
                if (parent == null) return;
                compEquippable = parent.GetComp<CompEquippable>();
                var deflector = parent.AllComps.FirstOrDefault(y =>
                    y.GetType().ToString() == "CompDeflector.CompDeflector" ||
                    y.GetType().BaseType.ToString() == "CompDeflector.CompDeflector");
                if (deflector != null)
                {
                    compDeflectorIsAnimatingNow =
                        (Func<bool>)AccessTools.PropertyGetter(deflector.GetType(), "IsAnimatingNow").CreateDelegate(
                            typeof(Func<bool>), deflector);
                }
                var activateable = parent.AllComps.FirstOrDefault(y =>
                    y.GetType().ToString() == "OgsCompActivatableEffect.CompActivatableEffect" ||
                    y.GetType().BaseType.ToString() == "OgsCompActivatableEffect.CompActivatableEffect");
                if (activateable != null)
                {
                    compActivatableEffectIsActiveNow =
                        (Func<bool>)AccessTools.PropertyGetter(deflector.GetType(), "IsActiveNow").CreateDelegate(
                            typeof(Func<bool>), activateable);


                }
                initComps = true;
            }
        }

        public CompEquippable GetEquippable
        {
            get
            {
                InitCompsAsNeeded();
                return compEquippable;
            }
        }

        public Pawn GetPawn => GetEquippable?.verbTracker?.PrimaryVerb?.CasterPawn;

        public bool CompDeflectorIsAnimatingNow
        {
            get
            {
                InitCompsAsNeeded();
                if (compDeflectorIsAnimatingNow != null)
                    return compDeflectorIsAnimatingNow();
                return false;
            }
        }

        private bool isEquipped = false;
        public bool IsEquipped
        {
            get
            {
                if (Find.TickManager.TicksGame % 60 != 0) return isEquipped;
                isEquipped = GetPawn != null;
                return isEquipped;
            }
        }

        private bool firstAttack = false;
        public bool FirstAttack
        {
            get => firstAttack;
            set => firstAttack = value;
        }
        public Vector3 AdjustRenderOffsetFromDir(Pawn pawn, bool Offhand = false)
        {
            var curDir = pawn.Rotation;

            Vector3 curOffset = Vector3.zero;

            if (this.Props != null)
            {

                curOffset = Offhand ? -this.Props.southOffset : this.Props.southOffset;
                if (curDir == Rot4.North)
                {
                    curOffset = Offhand ? -this.Props.northOffset : this.Props.northOffset;
                }
                else if (curDir == Rot4.East)
                {
                    curOffset = Offhand ? -this.Props.eastOffset : this.Props.eastOffset;
                }
                else if (curDir == Rot4.West)
                {
                    curOffset = Offhand ? -this.Props.westOffset : this.Props.westOffset;
                }
            }

            return curOffset;
        }

    }
}
