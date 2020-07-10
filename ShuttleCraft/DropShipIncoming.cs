using RimWorld;
using System;
using Verse;

namespace Dropships
{
	// Token: 0x02001598 RID: 5528
	[StaticConstructorOnStartup]
	public class DropShipIncoming : DropPodIncoming, IActiveDropPod, IThingHolder
	{
		protected override void SpawnThings()
		{
			/*
			if (this.Contents.spawnWipeMode == null)
			{
				base.SpawnThings();
				return;
			}
			for (int i = this.innerContainer.Count - 1; i >= 0; i--)
			{
				GenSpawn.Spawn(this.innerContainer[i], base.Position, base.Map, this.Contents.spawnWipeMode.Value);
			}
			*/
		}

		// Token: 0x06007932 RID: 31026 RVA: 0x00239D78 File Offset: 0x00237F78
		protected override void Impact()
		{
			for (int i = 0; i < 6; i++)
			{
				MoteMaker.ThrowDustPuff(base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), base.Map, 1.2f);
			}
			MoteMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
			GenClamor.DoClamor(this, 15f, ClamorDefOf.Impact);
			base.Impact();
		}
	}
}
