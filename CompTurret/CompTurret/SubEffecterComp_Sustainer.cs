using System;
using Verse;
using Verse.Sound;

namespace CompTurret
{
	// Token: 0x0200043E RID: 1086 MuvLuvBeta.SubEffecterComp_Sustainer
	public class SubEffecterComp_Sustainer : SubEffecterComp
	{
		// Token: 0x0600208A RID: 8330 RVA: 0x0001347D File Offset: 0x0001167D
		public SubEffecterComp_Sustainer(SubEffecterDef def, Effecter parent) : base(def, parent)
		{
		}

		// Token: 0x0600208B RID: 8331 RVA: 0x000C7350 File Offset: 0x000C5550
		public override void SubEffectTick(CompTurretGun A, TargetInfo B)
		{
			this.age++;
			if (this.age > this.def.ticksBeforeSustainerStart)
			{
				if (this.sustainer == null)
				{
					SoundInfo info = SoundInfo.InMap(A.Wearer, MaintenanceType.PerTick);
					this.sustainer = this.def.soundDef.TrySpawnSustainer(info);
					return;
				}
				this.sustainer.Maintain();
			}
		}

		// Token: 0x0400140D RID: 5133
		private int age;

		// Token: 0x0400140E RID: 5134
		private Sustainer sustainer;
	}
}
