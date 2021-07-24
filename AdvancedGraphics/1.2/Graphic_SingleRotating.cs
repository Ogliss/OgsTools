using System;
using UnityEngine;
using Verse;

namespace AdvancedGraphics
{
	// AdvancedGraphics.Graphic_SingleRotating
	public class Graphic_SingleRotating : Graphic_Single
	{
		// Token: 0x060015AD RID: 5549 RVA: 0x0007F364 File Offset: 0x0007D564
		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Mesh mesh = this.MeshAt(rot);
			float num = 0f;
			Material matSingle = this.MatSingle;
			if (thing != null)
			{
				matSingle = this.MatSingleFor(thing);
				if (thing is Projectile p)
                {
					Log.Message("Graphic_SingleRotating were a spinning projectile!");
                }
                else
                {
					Log.Message("Graphic_SingleRotating were a spinning thing!");
				}
				num = -360 + (float)(thing.thingIDNumber * 542) % (360 * 2f);
			}
            else
			{
				Log.Message("Graphic_SingleRotating NO thing!");
			}
			num += extraRotation;
			Graphics.DrawMesh(mesh, loc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0, null, 0);
		}

		// Token: 0x060015AE RID: 5550 RVA: 0x0007F3CC File Offset: 0x0007D5CC
		public override string ToString()
		{
			return "SingleRotating(subGraphic=" + this.ToString() + ")";
		}

	}
}
