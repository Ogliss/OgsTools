using RimWorld;
using System;
using Verse.AI;

namespace ExtraHives
{
	public class LordToil_DefendHiveAggressively : LordToil_HiveRelated
	{
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

		public float distToHiveToAttack = 40f;
	}
}
