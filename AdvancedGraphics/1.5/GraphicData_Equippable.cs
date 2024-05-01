using Verse;
using UnityEngine;

namespace AdvancedGraphics
{
    public class GraphicData_Equippable : GraphicData
    {
        public GraphicData groundGraphic = null;
        public Offsets offsets;
        public bool verticalFlipOutsideCombat = false;
        public bool verticalFlipNorth = false;
        public bool isDualWeapon = false;
        public bool useAlienRacesDrawsize = false;
        public bool meleeMirrored = true;
        public bool rangedMirrored = true;

        public float OffsetAngleFor(Rot4 rot, bool offhand = false)
		{
            OffsetRotation atRot = offsets.AtRot(rot);
            float result = atRot.angleAdjustment;
            bool isOffhand = false;
            bool invert = false;
            if (offhand)
            {
                if (!atRot.angleAdjustmentOffhand.HasValue)
                {
                    invert = true;
                }
                else result = atRot.angleAdjustmentOffhand.Value;
            }
            else
            {
                result = atRot.angleAdjustment;
            }
            result = invert ? -result : result;
            return result;

        }
		public Vector3 OffsetPosFor(Rot4 rot, bool offhand = false)
        {
            Vector3 result = new Vector3(0, 0, 0);
            OffsetRotation atRot = offsets.AtRot(rot);
            bool invert = false;
            bool invert2 = false;

            Vector3 tmp =  atRot.offset;
            Vector3 tmp2 = offsets.offset;
            if (offhand)
            {
                if (!atRot.offsetOffhand.HasValue)
                {
                    invert = true;
                }
                else tmp = atRot.offsetOffhand.Value;
                if (!offsets.offsetOffhand.HasValue)
                {
                    invert2 = true;
                }
                else tmp2 = offsets.offsetOffhand.Value;
            }
            if (tmp != null)
            {
                result += new Vector3(invert ? -tmp.x : tmp.x, tmp.y, tmp.z);
            }
            if (tmp2 != null)
            {
                result += new Vector3(invert2 ? -tmp2.x : tmp2.x, tmp2.y, tmp2.z);
            }
            return result;

        }

        public class Offsets
        {
            public Vector3 offset = new Vector3(0, 0, 0);
            public Vector3? offsetOffhand;
            public Vector2 size;
            public Vector2 sizeOffhand;
            public OffsetRotation south;
            public OffsetRotation north;
            public OffsetRotation east;
            public OffsetRotation west;

			public OffsetRotation AtRot(Rot4 rot)
			{
				if (rot == Rot4.North)
				{
					return north;
				}
				if (rot == Rot4.South)
				{
					return south;
				}
				if (rot == Rot4.East)
				{
					return east;
				}
				if (rot == Rot4.West)
				{
					return west;
                }
                return null;
            }
        }

        public class OffsetRotation
        {
            public Vector3 offset;
            public Vector3? offsetOffhand;
            public Vector2 size;
            public Vector2? sizeOffhand;
            public bool canFlip = false;
            public bool canFlipOffhand = false;
            public float angleAdjustment = 0f;
            public float? angleAdjustmentOffhand;
        }


    }
}
