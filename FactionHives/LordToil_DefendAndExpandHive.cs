using RimWorld;
using System;
using Verse.AI;

namespace ExtraHives
{
	// Token: 0x02000790 RID: 1936
	public class LordToil_DefendAndExpandHive : LordToil_HiveRelated
	{
		// Token: 0x06003280 RID: 12928 RVA: 0x00118CB0 File Offset: 0x00116EB0
		public override void UpdateAllDuties()
		{
			base.FilterOutUnspawnedHives();
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				Hive hiveFor = base.GetHiveFor(this.lord.ownedPawns[i]);
				PawnDuty duty = new PawnDuty(DutyDefOf.DefendAndExpandHive, hiveFor, this.distToHiveToAttack);
				this.lord.ownedPawns[i].mindState.duty = duty;
			}
		}

		// Token: 0x04001B58 RID: 7000
		public float distToHiveToAttack = 10f;
	}
}
