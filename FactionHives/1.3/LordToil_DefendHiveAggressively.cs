using RimWorld;
using System;
using Verse.AI;

namespace ExtraHives
{
	// Token: 0x02000792 RID: 1938
	public class LordToil_DefendHiveAggressively : LordToil_HiveRelated
	{
		// Token: 0x06003285 RID: 12933 RVA: 0x00118DB0 File Offset: 0x00116FB0
		public override void UpdateAllDuties()
		{
			base.FilterOutUnspawnedHives();
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Hive hiveFor = base.GetHiveFor(this.lord.ownedPawns[i]);
				PawnDuty duty = new PawnDuty(DutyDefOf.DefendHiveAggressively, hiveFor, this.distToHiveToAttack);
				this.lord.ownedPawns[i].mindState.duty = duty;
			}
		}

		// Token: 0x04001B5A RID: 7002
		public float distToHiveToAttack = 40f;
	}
}
