using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace WeaponizedNanites 
{
	// Token: 0x02000009 RID: 9
	public class NaniteCloud : Gas
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600002A RID: 42 RVA: 0x00002AE9 File Offset: 0x00000CE9
		// (set) Token: 0x0600002B RID: 43 RVA: 0x00002AF1 File Offset: 0x00000CF1
		public object cachedLabelMouseover { get; private set; }

		// Token: 0x0600002C RID: 44 RVA: 0x00002AFA File Offset: 0x00000CFA
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002B04 File Offset: 0x00000D04
		public override void Tick()
		{
			if (base.Destroyed)
			{
				return;
			}
			base.Tick();
			if (this.destroyTick <= Find.TickManager.TicksGame && !base.Destroyed)
			{
				this.Destroy(DestroyMode.Vanish);
				return;
			}
			this.graphicRotation += this.graphicRotationSpeed;
			this.Ticks--;
			if (this.Ticks <= 0)
			{
				this.TickTack();
				this.Ticks = this.TickRate;
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002B80 File Offset: 0x00000D80
		public void TickTack()
		{
			if (base.Destroyed)
			{
				return;
			}
			List<Thing> thingList = base.Position.GetThingList(base.Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] != null)
				{
					Thing thing = thingList[i];
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null)
					{
						this.touchingPawns.Add(pawn);
						if (!pawn.RaceProps.Animal)
						{
							this.doDamage(pawn);
							MoteMaker.ThrowDustPuff(pawn.Position, base.Map, 0.2f);
						}
					}
				}
			}
			for (int j = 0; j < this.touchingPawns.Count; j++)
			{
				Pawn pawn2 = this.touchingPawns[j];
				if (!pawn2.Spawned || pawn2.Position != base.Position)
				{
					this.touchingPawns.Remove(pawn2);
				}
				else if (!pawn2.RaceProps.Animal)
				{
					this.doDamage(pawn2);
				}
			}
			this.cachedLabelMouseover = null;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002E44 File Offset: 0x00001044
		public void doDamage(Pawn p)
		{
			List<BodyPartRecord> list = new List<BodyPartRecord>();
			List<Apparel> wornApparel = p.apparel.WornApparel;
			int num = Mathf.RoundToInt((float)this.AcidDamage * Rand.Range(0.5f, 1.25f));
			DamageInfo dinfo = default(DamageInfo);
			MoteMaker.ThrowDustPuff(p.Position, base.Map, 0.2f);
			foreach (BodyPartRecord bodyPartRecord in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null))
			{
				if (p.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(bodyPartRecord))
				{
					continue;
				}
				list.Add(bodyPartRecord);
			}
			for (int k = 0; k < list.Count; k++)
			{
				dinfo = new DamageInfo(DamageDefOf.Burn, (float)Mathf.RoundToInt((float)num * list[k].coverage), 0f, -1f, this, list[k], null, DamageInfo.SourceCategory.ThingOrUnknown, null);
				p.TakeDamage(dinfo);
			}
		}

		// Token: 0x04000024 RID: 36
		private List<Pawn> touchingPawns = new List<Pawn>();

		// Token: 0x04000025 RID: 37
		private List<Thing> touchingThings = new List<Thing>();

		// Token: 0x04000026 RID: 38
		private int Ticks = 100;

		// Token: 0x04000027 RID: 39
		private int TickRate = 100;

		// Token: 0x04000028 RID: 40
		private int AcidDamage = 3;
	}
}
