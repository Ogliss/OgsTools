using System;
using Verse;

namespace CompTurret
{
	// Token: 0x0200043C RID: 1084 MuvLuvBeta.SubEffecterComp_SprayerTriggered
	public class SubEffecterComp_SprayerTriggered : SubEffecterComp_Sprayer
	{
		// Token: 0x06002085 RID: 8325 RVA: 0x000C721D File Offset: 0x000C541D
		public SubEffecterComp_SprayerTriggered(SubEffecterDef def, Effecter parent) : base(def, parent)
		{
		}

		// Token: 0x06002086 RID: 8326 RVA: 0x000C72F9 File Offset: 0x000C54F9
		public override void SubTrigger(CompTurretGun A, TargetInfo B)
		{
			base.MakeMote(A, B);
		}
	}
}
