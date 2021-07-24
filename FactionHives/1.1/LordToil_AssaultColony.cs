using RimWorld;
using System;
using Verse.AI;
using Verse.AI.Group;

namespace ExtraHives
{
	// Token: 0x0200078C RID: 1932
	public class LordToil_AssaultColony : LordToil
	{
		// Token: 0x1700092B RID: 2347
		// (get) Token: 0x0600326C RID: 12908 RVA: 0x0001028D File Offset: 0x0000E48D
		public override bool ForceHighStoryDanger
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600326D RID: 12909 RVA: 0x00118801 File Offset: 0x00116A01
		public LordToil_AssaultColony(bool attackDownedIfStarving = false)
		{
			this.attackDownedIfStarving = attackDownedIfStarving;
		}

		// Token: 0x1700092C RID: 2348
		// (get) Token: 0x0600326E RID: 12910 RVA: 0x00010306 File Offset: 0x0000E506
		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600326F RID: 12911 RVA: 0x00118810 File Offset: 0x00116A10
		public override void Init()
		{
			base.Init();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
		}

		// Token: 0x06003270 RID: 12912 RVA: 0x00118824 File Offset: 0x00116A24
		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
				this.lord.ownedPawns[i].mindState.duty.attackDownedIfStarving = this.attackDownedIfStarving;
			}
		}

		// Token: 0x04001B54 RID: 6996
		private bool attackDownedIfStarving;
	}
}
