﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace ExtraHives
{
	public class LordToil_HiveRelatedData : LordToilData
	{
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
		public Dictionary<Pawn, Hive> assignedHives = new Dictionary<Pawn, Hive>();
	}
}
