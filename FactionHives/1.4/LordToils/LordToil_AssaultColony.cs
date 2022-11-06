﻿using RimWorld;
using System;
using Verse.AI;
using Verse.AI.Group;

namespace ExtraHives
{
	public class LordToil_AssaultColony : LordToil
	{
		public override bool ForceHighStoryDanger
		{
			get
			{
				return true;
			}
		}

		public LordToil_AssaultColony(bool attackDownedIfStarving = false)
		{
			this.attackDownedIfStarving = attackDownedIfStarving;
		}

		public override bool AllowSatisfyLongNeeds
		{
			get
			{
				return false;
			}
		}

		public override void Init()
		{
			base.Init();
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
				this.lord.ownedPawns[i].mindState.duty.attackDownedIfStarving = this.attackDownedIfStarving;
			}
		}

		private bool attackDownedIfStarving;
	}
}
