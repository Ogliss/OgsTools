using RimWorld;
using System;
using Verse;
using Verse.AI.Group;

namespace ExtraHives
{
	// Token: 0x0200076B RID: 1899 ExtraHives.LordJob_DefendAndExpandHive
	public class LordJob_DefendAndExpandHive : LordJob
	{
		// Token: 0x170008F9 RID: 2297
		// (get) Token: 0x06003196 RID: 12694 RVA: 0x00010306 File Offset: 0x0000E506
		public override bool CanBlockHostileVisitors
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170008FA RID: 2298
		// (get) Token: 0x06003197 RID: 12695 RVA: 0x00010306 File Offset: 0x0000E506
		public override bool AddFleeToil
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06003198 RID: 12696 RVA: 0x000F3C7A File Offset: 0x000F1E7A
		public LordJob_DefendAndExpandHive()
		{
		}

		// Token: 0x06003199 RID: 12697 RVA: 0x0011434C File Offset: 0x0011254C
		public LordJob_DefendAndExpandHive(SpawnedPawnParams parms)
		{
			this.aggressive = parms.aggressive;
		}

		// Token: 0x0600319A RID: 12698 RVA: 0x00114360 File Offset: 0x00112560
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_DefendAndExpandHive lordToil_DefendAndExpandHive = new LordToil_DefendAndExpandHive();
			lordToil_DefendAndExpandHive.distToHiveToAttack = 10f;
			stateGraph.StartingToil = lordToil_DefendAndExpandHive;
			LordToil_DefendHiveAggressively lordToil_DefendHiveAggressively = new LordToil_DefendHiveAggressively();
			lordToil_DefendHiveAggressively.distToHiveToAttack = 40f;
			stateGraph.AddToil(lordToil_DefendHiveAggressively);
			LordToil_AssaultColony lordToil_AssaultColony = new LordToil_AssaultColony(false);
			stateGraph.AddToil(lordToil_AssaultColony);
			Transition transition = new Transition(lordToil_DefendAndExpandHive, this.aggressive ? (LordToil)lordToil_AssaultColony : lordToil_DefendHiveAggressively, false, true);
			transition.AddTrigger(new Trigger_Memo(Hive.MemoAssaultOnSpawn));
			transition.AddTrigger(new Trigger_PawnHarmed(0.5f, true, null));
			transition.AddTrigger(new Trigger_PawnLostViolently(false));
			transition.AddTrigger(new Trigger_Memo(RimWorld.Hive.MemoAttackedByEnemy));
			transition.AddTrigger(new Trigger_Memo(Hive.MemoAttackedByEnemy));
			transition.AddTrigger(new Trigger_Memo(RimWorld.Hive.MemoBurnedBadly));
			transition.AddTrigger(new Trigger_Memo(Hive.MemoBurnedBadly));
			transition.AddTrigger(new Trigger_Memo(RimWorld.Hive.MemoDestroyedNonRoofCollapse));
			transition.AddTrigger(new Trigger_Memo(Hive.MemoDestroyedNonRoofCollapse));
			transition.AddTrigger(new Trigger_Memo(HediffGiver_Heat.MemoPawnBurnedByAir));
			transition.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition, false);
			Transition transition2 = new Transition(lordToil_DefendAndExpandHive, lordToil_AssaultColony, false, true);
			transition2.AddTrigger(new Trigger_PawnHarmed(0.5f, false, base.Map.ParentFaction));
			transition2.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition2, false);
			Transition transition3 = new Transition(lordToil_DefendHiveAggressively, lordToil_AssaultColony, false, true);
			transition3.AddTrigger(new Trigger_PawnHarmed(0.5f, false, base.Map.ParentFaction));
			transition3.AddPostAction(new TransitionAction_EndAllJobs());
			stateGraph.AddTransition(transition3, false);
			Transition transition4 = new Transition(lordToil_DefendAndExpandHive, lordToil_DefendAndExpandHive, true, true);
			transition4.AddTrigger(new Trigger_Memo(RimWorld.Hive.MemoDeSpawned));
			transition4.AddTrigger(new Trigger_Memo(Hive.MemoDeSpawned));
			stateGraph.AddTransition(transition4, false);
			Transition transition5 = new Transition(lordToil_DefendHiveAggressively, lordToil_DefendHiveAggressively, true, true);
			transition5.AddTrigger(new Trigger_Memo(RimWorld.Hive.MemoDeSpawned));
			transition5.AddTrigger(new Trigger_Memo(Hive.MemoDeSpawned));
			stateGraph.AddTransition(transition5, false);
			Transition transition6 = new Transition(lordToil_AssaultColony, lordToil_DefendAndExpandHive, false, true);
			transition6.AddSource(lordToil_DefendHiveAggressively);
			transition6.AddTrigger(new Trigger_TicksPassedWithoutHarmOrMemos(1200, new string[]
			{
                RimWorld.Hive.MemoAttackedByEnemy,
                Hive.MemoAttackedByEnemy,
                RimWorld.Hive.MemoBurnedBadly,
                Hive.MemoBurnedBadly,
                RimWorld.Hive.MemoDestroyedNonRoofCollapse,
                Hive.MemoDestroyedNonRoofCollapse,
                RimWorld.Hive.MemoDeSpawned,
                Hive.MemoDeSpawned,

				HediffGiver_Heat.MemoPawnBurnedByAir
			}));
			transition6.AddPostAction(new TransitionAction_EndAttackBuildingJobs());
			stateGraph.AddTransition(transition6, false);
			return stateGraph;
		}
		// Token: 0x0600319B RID: 12699 RVA: 0x0011456C File Offset: 0x0011276C
		public override void ExposeData()
		{
			Scribe_Values.Look<bool>(ref this.aggressive, "aggressive", false, false);
		}

		// Token: 0x04001B0E RID: 6926
		private bool aggressive;
	}
}
