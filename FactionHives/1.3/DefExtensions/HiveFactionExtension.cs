using RimWorld;
using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace ExtraHives
{
    public class HiveFactionExtension : DefModExtension
	{
		public bool overrideBaseGen = false;
		public string baseGenOverride = string.Empty;
		public bool baseDamage = true;
		public bool randomHives = false;
		public bool noPawnPointsCurve = false;
		public IntRange sizeRange = new IntRange(44, 60);

		public ThingDef smallCaveHive = null;
		public ThingDef largeCaveHive = null;
		public ThingDef centerCaveHive = null;
		public ThingDef cultivatedPlantDef;

		public string hiveStartMessageKey = string.Empty;
		public string hiveActiveMessageKey = string.Empty;
		public string hiveStageProgressionKey = string.Empty;
		public List<HiveStage> stages = new List<HiveStage>();

		public bool HasStages => !stages.NullOrEmpty();
		public bool showStageInName = false;
		public string stageKey = ": Stage {0}";
		public HiveStage CurStage
		{
			get
			{
				HiveStage cur = stages[0];
				if (HasStages)
				{
					List<HiveStage> st = stages;
					for (int i = 0; i < st.Count; i++)
					{
						HiveStage stage = st[i];
						if (stage.DaysPassed > GenDate.DaysPassed)
						{
							break;
						}
						cur = stage;
					}

				}
				return cur;
			}
		}
		public int ActiveStage
		{
			get
			{
				int stage = 0;
				if (this.HasStages)
				{
					stage = stages.IndexOf(CurStage)+1;
				}
				return stage;
			}
		}

	}
	public class HiveStage
	{
		public int DaysPassed = -1;
		public float pointMultipler = 1f;
		public IntRange sizeRange = new IntRange(44, 60);

		public SimpleCurve maxPawnCostPerTotalPointsCurve;
		public List<PawnGroupMaker> pawnGroupMakers;
		public ThingDef smallCaveHive = null;
		public ThingDef largeCaveHive = null;
		public ThingDef centerCaveHive = null;
	}
}
