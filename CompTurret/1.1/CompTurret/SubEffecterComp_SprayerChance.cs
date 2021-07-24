using System;
using Verse;

namespace CompTurret
{
	// Token: 0x0200043B RID: 1083 MuvLuvBeta.SubEffecterComp_SprayerChance
	public class SubEffecterComp_SprayerChance : SubEffecterComp_Sprayer
	{
		// Token: 0x06002083 RID: 8323 RVA: 0x000C721D File Offset: 0x000C541D
		public SubEffecterComp_SprayerChance(SubEffecterDef def, Effecter parent) : base(def, parent)
		{
		}

		// Token: 0x06002084 RID: 8324 RVA: 0x000C7288 File Offset: 0x000C5488
		public override void SubEffectTick(CompTurretGun A, TargetInfo B)
		{
			float num = this.def.chancePerTick;
			if (this.def.spawnLocType == MoteSpawnLocType.RandomCellOnTarget && B.HasThing)
			{
				num *= (float)(B.Thing.def.size.x * B.Thing.def.size.z);
			}
			Rand.PushState();
			if (Rand.Value < num)
			{
				base.MakeMote(A, B);
			}
			Rand.PopState();
		}
	}
}
