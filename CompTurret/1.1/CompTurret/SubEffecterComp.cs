using System;
using Verse;

namespace CompTurret
{
	// Token: 0x02000435 RID: 1077
	public class SubEffecterComp : SubEffecter
	{
		// Token: 0x06002074 RID: 8308 RVA: 0x000C6D64 File Offset: 0x000C4F64
		public SubEffecterComp(SubEffecterDef subDef, Effecter parent) : base(subDef, parent)
		{
			this.def = subDef;
			this.parent = parent;
		}

		// Token: 0x06002075 RID: 8309 RVA: 0x00002681 File Offset: 0x00000881
		public virtual void SubEffectTick(CompTurretGun A, TargetInfo B)
		{
		}

		// Token: 0x06002076 RID: 8310 RVA: 0x00002681 File Offset: 0x00000881
		public virtual void SubTrigger(CompTurretGun A, TargetInfo B)
		{
		}
	}
}
