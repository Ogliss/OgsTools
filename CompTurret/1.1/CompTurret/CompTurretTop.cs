using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace CompTurret
{
	public class CompTurretTop
	{
		private float CurRotation
		{
			get
			{
				return this.curRotationInt;
			}
			set
			{
				this.curRotationInt = value;
				if (this.curRotationInt > 360f)
				{
					this.curRotationInt -= 360f;
				}
				if (this.curRotationInt < 0f)
				{
					this.curRotationInt += 360f;
				}
			}
		}

		public void SetRotationFromOrientation()
		{
			this.CurRotation = this.parentTurret.Operator.Rotation.AsAngle;
		}

		public CompTurretTop(CompTurret ParentTurret)
		{
			this.parentTurret = ParentTurret;
		}
		public Vector3 DrawPos
		{
			get
			{
				Vector3 b = new Vector3(this.parentTurret.Props.TurretDef.building.turretTopOffset.x, 1f, this.parentTurret.Props.TurretDef.building.turretTopOffset.y).RotatedBy(this.CurRotation);
				Vector3 vector = this.parentTurret.Operator.DrawPos;
				if (this.parentTurret.Operator.ParentHolder as PawnFlyer is PawnFlyer flyer)
				{
					vector = flyer.DrawPos;
				}
				Vector3 drawPos = vector + Altitudes.AltIncVect + b; 

				Rot4 rot = this.parentTurret.Operator.Rotation;
				if (rot == Rot4.North)
				{
					drawPos += this.parentTurret.Props.offsetNorth;
				}
				if (rot == Rot4.South)
				{
					drawPos += this.parentTurret.Props.offsetSouth;
				}
				if (rot == Rot4.East)
				{
					drawPos += this.parentTurret.Props.offsetEast;
				}
				if (rot == Rot4.West)
				{
					drawPos += this.parentTurret.Props.offsetWest;
				}
				return drawPos;
			}
		}

		public void TurretTopTick()
		{
			LocalTargetInfo currentTarget = this.parentTurret.CurrentTarget;
			if (this.parentTurret.stunTicksLeft > 0)
			{
				/*
				if (this.parentTurret.showStunMote && (this.parentTurret.moteStun == null || this.parentTurret.moteStun.Destroyed))
				{
					this.parentTurret.moteStun = MakeStunOverlay(this.parentTurret.Wearer);
				}
				*/
				Pawn pawn = this.parentTurret.Operator as Pawn;
				if (pawn != null && pawn.Downed)
				{
					this.parentTurret.stunTicksLeft = 0;
				}

				if (this.parentTurret.moteStun != null)
				{
					this.parentTurret.moteStun.Maintain();
				}

				if (this.parentTurret.AffectedByEMP && this.parentTurret.stunFromEMP)
				{
					if (this.parentTurret.empEffecter == null)
					{
						this.parentTurret.empEffecter = new EffecterComp(DefDatabase<EffecterDef>.GetNamed("CompTurretDisabledByEMP"));
					}
					EffecterComp empEffecter = this.parentTurret.empEffecter as EffecterComp;
					if (empEffecter != null)
					{
					//	Log.Message("empEffecter EffecterComp");
						empEffecter.EffectTick(this.parentTurretGun, this.parentTurret.Operator);
					}
					else
					{
					//	Log.Message("empEffecter Effecter");
						this.parentTurret.empEffecter.EffectTick(this.parentTurret.Operator, this.parentTurret.Operator);
					}
					return;
				}
			}
			else if (this.parentTurret.empEffecter != null)
			{
				this.parentTurret.empEffecter.Cleanup();
				this.parentTurret.empEffecter = null;
				this.parentTurret.stunFromEMP = false;
			}
			if (parentTurretGun.Active && currentTarget.IsValid && parentTurretGun.HasAmmo)
			{
				float curRotation = (currentTarget.Cell.ToVector3Shifted() - DrawPos).AngleFlat();
				this.CurRotation = curRotation;
				Rand.PushState();
				this.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
				Rand.PopState();
				return;
			}
			else
			{
                if (parentTurret.CasterIsPawn)
				{
					SetRotationFromOrientation();
					return;
				}
                else
                {


					float rotmax = this.parentTurret.Operator.Rotation.AsAngle + 90;
					float rotmin = this.parentTurret.Operator.Rotation.AsAngle - 90;
					if (this.ticksUntilIdleTurn > 0)
					{
						this.ticksUntilIdleTurn--;
						if (this.ticksUntilIdleTurn == 0)
						{
							Rand.PushState();
							if (Rand.Value < 0.5f)
							{
								this.idleTurnClockwise = this.CurRotation < (rotmax - 37);
							}
							else
							{
								this.idleTurnClockwise = this.CurRotation > (rotmin + 37);
							}
							Rand.PopState();
							this.idleTurnTicksLeft = 140;
							return;
						}
					}
					else
					{

						if (this.idleTurnClockwise)
						{
							this.CurRotation += 0.26f;
                            if (CurRotation > rotmax)
                            {
								idleTurnClockwise = !idleTurnClockwise;

							}
						}
						else
						{
							this.CurRotation -= 0.26f;
							if (CurRotation > rotmin)
							{
								idleTurnClockwise = !idleTurnClockwise;

							}
						}
						this.idleTurnTicksLeft--;
						if (this.idleTurnTicksLeft <= 0)
						{
							Rand.PushState();
							this.ticksUntilIdleTurn = Rand.RangeInclusive(150, 350);
							Rand.PopState();
						}
					}
				//	this.CurRotation = Mathf.Clamp(this.CurRotation, rotmin, rotmax);
				}
			}
			



		}
		public Mote MakeStunOverlay(Thing stunnedThing)
		{
			MoteCompTurretAttached mote = (MoteCompTurretAttached)ThingMaker.MakeThing(ThingDef.Named("Mote_CompTurretStun"), null);
			mote.Attach(stunnedThing, parentTurretGun);
			GenSpawn.Spawn(mote, stunnedThing.Position, stunnedThing.Map, WipeMode.Vanish);
			return mote;
		}

		public void DrawTurret()
		{
			float turretTopDrawSize = this.parentTurret.Props.TurretDef.building.turretTopDrawSize;
			Matrix4x4 matrix = default(Matrix4x4);
			Quaternion quart = (this.CurRotation + (float)CompTurretTop.ArtworkRotation).ToQuat();
			matrix.SetTRS(DrawPos, quart, new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
			Graphics.DrawMesh(MeshPool.plane10, matrix, this.parentTurret.Props.TurretDef.building.turretTopMat, 0);
			if (this.parentTurret.TargetCurrentlyAimingAt != null && !this.parentTurret.Stunned && parentTurretGun !=null && parentTurretGun.HasAmmo)
			{
                if (parentTurretGun.LineMatRed != null)
				{
					GenDraw.DrawLineBetween(DrawPos, this.parentTurret.CurrentTarget.CenterVector3, parentTurretGun.LineMatRed);
				}
			}

		}
		private CompTurret parentTurret;
		private CompTurretGun parentTurretGun => parentTurret as CompTurretGun;
		private float curRotationInt;
		private int ticksUntilIdleTurn;
		private int idleTurnTicksLeft;
		private bool idleTurnClockwise;
		private const float IdleTurnDegreesPerTick = 0.26f;
		private const int IdleTurnDuration = 140;
		private const int IdleTurnIntervalMin = 150;
		private const int IdleTurnIntervalMax = 350;
		public static readonly int ArtworkRotation = -90;
	}
}
