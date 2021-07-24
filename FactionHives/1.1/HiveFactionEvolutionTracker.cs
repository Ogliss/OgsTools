using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace ExtraHives
{
	public class HiveFactionEvolutionTracker : WorldComponent
	{

		public Dictionary<string, int> HiveFactionStages = new Dictionary<string, int>();

		private List<FactionDef> factionDefs;
		public List<FactionDef> FactionDefs
		{
			get
			{
				if (factionDefs.NullOrEmpty())
				{
					factionDefs = new List<FactionDef>();
					factionDefs = DefDatabase<FactionDef>.AllDefs.Where(x=> x.HasModExtension<HiveFactionExtension>()).ToList();
				}
				return factionDefs;
			}
		}

		public HiveFactionEvolutionTracker(World world) : base(world)
		{
			/*
			for (int i = 0; i < HiveFactions.Count; i++)
			{
				HiveFactionExtension extension = HiveFactions[i].def.GetModExtension<HiveFactionExtension>();
				if (!extension.HasStages)
				{
					continue;
				}
			//	Log.Message("setting inital stage for " + HiveFactions[i]);
				HiveFactionStages.SetOrAdd(HiveFactions[i], 1);
			}
			*/
			ticks = 0;
		}

		

		private List<Faction> hiveFactions = new List<Faction>();
		/*
		public List<Faction> HiveFactions
		{
			get
			{
				if (hiveFactions.NullOrEmpty())
				{
					hiveFactions = Find.FactionManager.AllFactionsListForReading.FindAll(x => x.def.HasModExtension<HiveFactionExtension>());
				}
				return hiveFactions;
			}
		}
		*/
		public List<Settlement> Settlements(Faction faction)
		{
			List<Settlement> list = new List<Settlement>();
			list = world.worldObjects.Settlements.FindAll(x => x.Faction == faction);
			return list;
		}
		public int DaysPassed => GenDate.DaysPassed;

		public override void WorldComponentTick()
		{

			if (ticks == 0)
			{
				/*
				for (int i = 0; i < HiveFactions.Count; i++)
				{
					HiveFactionExtension extension = HiveFactions[i].def.GetModExtension<HiveFactionExtension>();
					if (!extension.HasStages)
					{
						continue;
					}

				}
				*/
				for (int i = 0; i < Find.FactionManager.AllFactionsListForReading.Count; i++)
				{
					Faction f = Find.FactionManager.AllFactionsListForReading[i];
					if (f!=null)
					{
						
						if (f.def.HasModExtension<HiveFactionExtension>() && !f.defeated)
						{
							HiveFactionExtension ext = f.def.GetModExtension<HiveFactionExtension>();
						//	Log.Message(f+" hive faction");
							if (!ext.stages.NullOrEmpty())
							{
							//	Log.Message(f +" has " + ext.stages.Count + " stages");
							//	Log.Message(Find.TickManager.TicksGame + " day passed " + DaysPassed + " CurrentPhase: " + ext.CurStage + " " + ext.ActiveStage);

								if (CurrentPhase < ext.ActiveStage || !this.HiveFactionStages.ContainsKey(f.ToString()))
								{
									UpdatePhase(f, ext.ActiveStage);
								}
							}
							if (!ext.hiveStartMessageKey.NullOrEmpty())
							{
								if (!startMsg)
								{
									startMsg = true;
									NewGameDialogMessage(f, ext.hiveStartMessageKey);
								}
							}
							if (!ext.hiveActiveMessageKey.NullOrEmpty())
							{
								if (!activetMsg && DaysPassed > f.def.earliestRaidDays)
								{
									activetMsg = true;
									HiveActiveDialogMessage(f, ext.ActiveStage, ext.hiveActiveMessageKey, ext.stages[ext.ActiveStage].DaysPassed - DaysPassed);
								}
							}
						}
					}
				}
				ticks = tickInterval;
			}
			ticks--;
		}

		public void UpdatePhase(Faction f, int phase)
		{
			//	Log.Message(f+ " UpdatePhase Keyed: "+ this.HiveFactionStages.ContainsKey(f.ToString()));
			HiveFactionExtension hive = f.def.GetModExtension<HiveFactionExtension>();
			if (!hive.hiveStageProgressionKey.NullOrEmpty() && this.HiveFactionStages.ContainsKey(f.ToString()))
			{
				UpdatePhaseDialogMessage(f, phase, hive.hiveStageProgressionKey);
			}
			CurrentPhase = phase;
			HiveFactionStages.SetOrAdd(f.ToString(), phase);
		}

		public void UpdatePhaseDialogMessage(Faction f, int phase, string msg)
		{
			DiaNode diaNode = new DiaNode(msg.Translate(f, phase));
			DiaOption diaOption = new DiaOption("OK");
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
			Find.WindowStack.Add(dialog_NodeTree);
			Find.Archive.Add(new ArchivedDialog(diaNode.text, null, null));
		}

		public void NewGameDialogMessage(Faction f, string msg)
		{
			DiaNode diaNode = new DiaNode(msg.Translate(f));
			DiaOption diaOption = new DiaOption("OK");
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
			Find.WindowStack.Add(dialog_NodeTree);
			Find.Archive.Add(new ArchivedDialog(diaNode.text, null, null));
		}

		public void HiveActiveDialogMessage(Faction f, int phase, string msg, int daysLeft)
		{
			DiaNode diaNode = new DiaNode(msg.Translate(f, phase, daysLeft));
			DiaOption diaOption = new DiaOption("OK");
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			Dialog_NodeTree dialog_NodeTree = new Dialog_NodeTree(diaNode, true, false, null);
			Find.WindowStack.Add(dialog_NodeTree);
			Find.Archive.Add(new ArchivedDialog(diaNode.text, null, null));
		}


		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref CurrentPhase, "CurrentPhase");
		//	Scribe_Values.Look(ref ticks, "ticks");
			Scribe_Values.Look(ref startMsg, "startMsg");
			Scribe_Values.Look(ref activetMsg, "activetMsg");
			Scribe_Collections.Look<string, int>(ref this.HiveFactionStages, "HiveFactionStages", LookMode.Value, LookMode.Value, ref factions, ref stages);
			Scribe_Collections.Look<string>(ref this.factions, false, "Hivefactions", LookMode.Value, new List<string>());
			Scribe_Collections.Look<int>(ref this.stages, "Hivestages", LookMode.Value, new List<int>());
		}

		private List<string> factions;
		private List<int> stages;
		public int CurrentPhase = 1;
		private int ticks = 0;
		public bool startMsg = false;
		public bool activetMsg = false;


		private int tickInterval = 30000;
	}
}
