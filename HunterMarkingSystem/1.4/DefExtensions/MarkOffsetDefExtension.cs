using System;
using Verse;

namespace HunterMarkingSystem
{
    public class MarkOffsetDefExtension : DefModExtension
    {
        public HediffDef hediff;
        // North
        public float NorthXOffset = 0f;
        public float NorthZOffset = 0f;
        public float NorthYOffset = 0f;
        public float NorthXDrawSize = 1f;
        public float NorthZDrawSize = 1f;
        public float NorthAngle = 0f;

        // South
        public float SouthXOffset = 0f;
        public float SouthZOffset = 0.267f;
        public float SouthYOffset = 0f;
        public float SouthXDrawSize = 1f;
        public float SouthZDrawSize = 1f;
        public float SouthAngle = 0f;

        // East
        public float EastXOffset = 0f;
        public float EastZOffset = 0f;
        public float EastYOffset = 0f;
        public float EastXDrawSize = 1f;
        public float EastZDrawSize = 1f;
        public float EastAngle = 0f;

        // West
        public float WestXOffset = 0f;
        public float WestZOffset = 0f;
        public float WestYOffset = 0f;
        public float WestXDrawSize = 1f;
        public float WestZDrawSize = 1f;
        public float WestAngle = 0f;

        public bool ApplyBaseHeadOffset = false;
    }
}
