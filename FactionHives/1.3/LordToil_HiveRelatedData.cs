using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace ExtraHives
{
	// Token: 0x02000797 RID: 1943
	public class LordToil_HiveRelatedData : LordToilData
	{
		// Token: 0x060032A1 RID: 12961 RVA: 0x0011970C File Offset: 0x0011790C
		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Key.Destroyed);
			}
			Scribe_Collections.Look<Pawn, Hive>(ref this.assignedHives, "assignedHives", LookMode.Reference, LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.assignedHives.RemoveAll((KeyValuePair<Pawn, Hive> x) => x.Value == null);
			}
		}

		// Token: 0x04001B5C RID: 7004
		public Dictionary<Pawn, Hive> assignedHives = new Dictionary<Pawn, Hive>();
	}
}
