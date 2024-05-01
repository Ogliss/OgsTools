using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace Dropships
{
	// Token: 0x02001596 RID: 5526
	public class ActiveDropShip : ActiveDropPod, IActiveDropPod, IThingHolder
	{
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.age, "age", 0, false);
			Scribe_Deep.Look<ActiveDropPodInfo>(ref this.contents, "contents", new object[]
			{
				this
			});
		}


		// Token: 0x06007928 RID: 31016 RVA: 0x00239A08 File Offset: 0x00237C08
		public override void Tick()
		{
			if (this.contents == null)
			{
				return;
			}
			this.contents.innerContainer.ThingOwnerTick(true);
			if (base.Spawned)
			{
				this.age++;
				if (this.age > this.contents.openDelay)
				{
					this.PodOpen();
				}
			}
		}

		// Token: 0x06007929 RID: 31017 RVA: 0x00239A60 File Offset: 0x00237C60
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (this.contents != null)
			{
				this.contents.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
			}
			Map map = base.Map;
			base.Destroy(mode);
		}

		public Thing dropship => this.contents.innerContainer.FirstOrFallback(x => x.TryGetComp<CompDropship>() != null);

		// Token: 0x0600792A RID: 31018 RVA: 0x00239AC8 File Offset: 0x00237CC8
		private void PodOpen()
		{
			Map map = base.Map;
			if (this.contents.despawnPodBeforeSpawningThing)
			{
				this.DeSpawn(DestroyMode.Vanish);
			}
			if (dropship != null)
			{
				this.contents.innerContainer.Remove(dropship);
			}
			else return;
			GenSpawn.Spawn(dropship, base.Position, map, this.contents.setRotation.Value, this.contents.spawnWipeMode.Value, false);
			for (int i = this.contents.innerContainer.Count - 1; i >= 0; i--)
			{
				Thing thing = this.contents.innerContainer[i];
				if (dropship.TryGetComp<CompTransporter>() != null)
				{
					CompDropship transporter = dropship.TryGetComp<CompDropship>();
					transporter.Transporter.innerContainer.TryAddOrTransfer(thing);
				}

			}
			this.contents.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
			//	SoundDefOf.DropPod_Open.PlayOneShot(new TargetInfo(base.Position, map, false));
			this.Destroy(DestroyMode.Vanish);
		}

		// Token: 0x04004DE3 RID: 19939
		public new int age;

		// Token: 0x04004DE4 RID: 19940
		private ActiveDropPodInfo contents;
	}
}
