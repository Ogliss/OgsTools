using System;
using UnityEngine;
using Verse;

namespace CompTurret
{
	// Token: 0x02000439 RID: 1081 MuvLuvBeta.SubEffecter_CompSprayer
	public abstract class SubEffecterComp_Sprayer : SubEffecterComp
	{
		// Token: 0x0600207F RID: 8319 RVA: 0x0001347D File Offset: 0x0001167D
		public SubEffecterComp_Sprayer(SubEffecterDef def, Effecter parent) : base(def, parent)
		{
		}

		// Token: 0x06002080 RID: 8320 RVA: 0x000C6E58 File Offset: 0x000C5058
		protected void MakeMote(CompTurretGun A, TargetInfo B)
		{
			Vector3 vector = Vector3.zero;
			switch (this.def.spawnLocType)
			{
				case MoteSpawnLocType.OnSource:
					vector = A.TurretPos;
					break;
				case MoteSpawnLocType.BetweenPositions:
					{
						Vector3 vector2 = A.isWorn ? A.TurretPos : A.parent.DrawPos;
						Vector3 vector3 = B.HasThing ? B.Thing.DrawPos : B.Cell.ToVector3Shifted();
						if (A.isWorn && !A.Wearer.Spawned)
						{
							vector = vector3;
						}
						else if (B.HasThing && !B.Thing.Spawned)
						{
							vector = vector2;
						}
						else
						{
							vector = vector2 * this.def.positionLerpFactor + vector3 * (1f - this.def.positionLerpFactor);
						}
						break;
					}
				case MoteSpawnLocType.BetweenTouchingCells:
					vector = A.Wearer.Position.ToVector3Shifted() + (B.Cell - A.Wearer.Position).ToVector3().normalized * 0.5f;
					break;
				case MoteSpawnLocType.RandomCellOnTarget:
					{
						CellRect cellRect;
						if (B.HasThing)
						{
							cellRect = B.Thing.OccupiedRect();
						}
						else
						{
							cellRect = CellRect.CenteredOn(B.Cell, 0);
						}
						vector = cellRect.RandomCell.ToVector3Shifted();
						break;
					}
			}
			if (this.parent != null)
			{
				Rand.PushState(this.parent.GetHashCode());
				if (A.TurretPos != B.CenterVector3)
				{
					vector += (B.CenterVector3 - A.TurretPos).normalized * this.parent.def.offsetTowardsTarget.RandomInRange;
				}
				vector += Gen.RandomHorizontalVector(this.parent.def.positionRadius) + this.parent.offset;
				Rand.PopState();
			}
			Map map = A.Wearer.Map ?? B.Map;
			float num = this.def.absoluteAngle ? 0f : (B.Cell - A.Wearer.Position).AngleFlat;
			float num2 = (this.parent != null) ? this.parent.scale : 1f;
			if (map != null && vector.ShouldSpawnMotesAt(map))
			{
				int randomInRange = this.def.burstCount.RandomInRange;
				for (int i = 0; i < randomInRange; i++)
				{
					Mote mote = (Mote)ThingMaker.MakeThing(this.def.moteDef, null);
					GenSpawn.Spawn(mote, vector.ToIntVec3(), map, WipeMode.Vanish);
					mote.Scale = this.def.scale.RandomInRange * num2;
					mote.exactPosition = vector + this.def.positionOffset * num2 + Gen.RandomHorizontalVector(this.def.positionRadius) * num2;
					mote.rotationRate = this.def.rotationRate.RandomInRange;
					mote.exactRotation = this.def.rotation.RandomInRange + num;
					mote.instanceColor = this.def.color;
					MoteThrown moteThrown = mote as MoteThrown;
					if (moteThrown != null)
					{
						moteThrown.airTimeLeft = this.def.airTime.RandomInRange;
						moteThrown.SetVelocity(this.def.angle.RandomInRange + num, this.def.speed.RandomInRange);
					}
				}
			}
		}
	}
}
