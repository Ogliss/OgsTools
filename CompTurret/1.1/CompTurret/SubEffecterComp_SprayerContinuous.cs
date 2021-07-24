using System;
using Verse;

namespace CompTurret
{
	// Token: 0x0200043A RID: 1082 MuvLuvBeta.SubEffecterComp_SprayerContinuous
	public class SubEffecterComp_SprayerContinuous : SubEffecterComp_Sprayer
	{
		// Token: 0x06002081 RID: 8321 RVA: 0x000C721D File Offset: 0x000C541D
		public SubEffecterComp_SprayerContinuous(SubEffecterDef def, Effecter parent) : base(def, parent)
		{
		}

		// Token: 0x06002082 RID: 8322 RVA: 0x000C7228 File Offset: 0x000C5428
		public override void SubEffectTick(CompTurretGun A, TargetInfo B)
		{
			if (this.moteCount >= this.def.maxMoteCount)
			{
				return;
			}
			this.ticksUntilMote--;
			if (this.ticksUntilMote <= 0)
			{
				base.MakeMote(A, B);
				this.ticksUntilMote = this.def.ticksBetweenMotes;
				this.moteCount++;
			}
		}

		// Token: 0x0400140A RID: 5130
		private int ticksUntilMote;

		// Token: 0x0400140B RID: 5131
		private int moteCount;
	}
}
