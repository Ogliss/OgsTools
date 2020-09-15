using System;
using UnityEngine;
using Verse;

namespace CompTurret
{
	// Token: 0x0200030D RID: 781
	public class MoteCompTurretAttached : MoteDualAttached
	{
		public void Attach(TargetInfo a, CompTurretGun turretGun)
		{
			this.link1 = new MoteAttachLink(a);
			this.turretGun = turretGun;
		}
		private CompTurretGun turretGun = null;
		public override Vector3 DrawPos => turretGun?.TurretPos ?? base.DrawPos;

	}
}
