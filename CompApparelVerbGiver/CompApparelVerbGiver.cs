using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CompApparelVerbGiver
{
	public class CompProperties_ApparelVerbGiver : CompProperties
	{
		CompProperties_ApparelVerbGiver()
		{
			this.compClass = typeof(CompApparelVerbGiver);
		}
		public List<Tool> tools;
		public List<VerbProperties> verbs;
	}

	public class CompApparelVerbGiver : ThingComp, IVerbOwner
	{
		CompProperties_ApparelVerbGiver Props => this.props as CompProperties_ApparelVerbGiver;
		// (get) Token: 0x06001708 RID: 5896 RVA: 0x00083DCC File Offset: 0x00081FCC
		private Pawn Wearer
		{
			get
			{
				return this.PrimaryVerb.CasterPawn;
			}
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06001709 RID: 5897 RVA: 0x00083DD9 File Offset: 0x00081FD9
		public List<Verb> AllVerbs
		{
			get
			{
				return this.verbTracker.AllVerbs;
			}
		}

		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x0600170A RID: 5898 RVA: 0x00083DE6 File Offset: 0x00081FE6
		public Verb PrimaryVerb
		{
			get
			{
				return this.verbTracker.PrimaryVerb;
			}
		}

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x0600170B RID: 5899 RVA: 0x00083DF3 File Offset: 0x00081FF3
		public VerbTracker VerbTracker
		{
			get
			{
				return this.verbTracker;
			}
		}

		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x0600170C RID: 5900 RVA: 0x00083DFB File Offset: 0x00081FFB
		public List<VerbProperties> VerbProperties
		{
			get
			{
				return this.parent.def.Verbs;
			}
		}

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x0600170D RID: 5901 RVA: 0x00083E0D File Offset: 0x0008200D
		public List<Tool> Tools
		{
			get
			{
				return this.parent.def.tools;
			}
		}

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x0600170E RID: 5902 RVA: 0x00019975 File Offset: 0x00017B75
		Thing IVerbOwner.ConstantCaster
		{
			get
			{
				return Wearer;
			}
		}

		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x0600170F RID: 5903 RVA: 0x00083E1F File Offset: 0x0008201F
		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef
		{
			get
			{
				return ImplementOwnerTypeDefOf.Weapon;
			}
		}

		// Token: 0x06001710 RID: 5904 RVA: 0x00083E26 File Offset: 0x00082026
		public CompApparelVerbGiver()
		{
			this.verbTracker = new VerbTracker(this);
		}

		// Token: 0x06001711 RID: 5905 RVA: 0x00083E3A File Offset: 0x0008203A
		public IEnumerable<Command> GetVerbsCommands()
		{
			return this.verbTracker.GetVerbsCommands(KeyCode.None);
		}

		// Token: 0x06001712 RID: 5906 RVA: 0x00083E48 File Offset: 0x00082048
		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (this.Wearer != null && this.Wearer.equipment != null && this.Wearer.equipment.Primary == this.parent)
			{
				this.Wearer.equipment.Notify_PrimaryDestroyed();
			}
		}

		// Token: 0x06001713 RID: 5907 RVA: 0x00083E9A File Offset: 0x0008209A
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[]
			{
				this
			});
		}

		// Token: 0x06001714 RID: 5908 RVA: 0x00083EBC File Offset: 0x000820BC
		public override void CompTick()
		{
			base.CompTick();
			if (this.verbTracker == null)
			{
				this.verbTracker = new VerbTracker(this);
			}
			this.verbTracker.VerbsTick();
		}

		// Token: 0x06001715 RID: 5909 RVA: 0x00083ED0 File Offset: 0x000820D0
		public void Notify_EquipmentLost()
		{
			List<Verb> allVerbs = this.AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				allVerbs[i].Notify_EquipmentLost();
			}
		}

		// Token: 0x06001716 RID: 5910 RVA: 0x00083F01 File Offset: 0x00082101
		string IVerbOwner.UniqueVerbOwnerID()
		{
			return "CompEquippable_" + this.parent.ThingID;
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x00083F18 File Offset: 0x00082118
		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			Apparel apparel = this.parent as Apparel;
			if (apparel != null)
			{
				return p.apparel.WornApparel.Contains(apparel);
			}
			return p.equipment.AllEquipmentListForReading.Contains(this.parent);
		}

		public override string CompInspectStringExtra()
		{
			string str = "OG_ApparelTool".Translate();

			return str;
		}
		public override string GetDescriptionPart()
		{

			string str = "OG_ApparelToolDesc".Translate();
			for (int i = 0; i < Tools.Count; i++)
			{
				str += "\n"+Tools[i].LabelCap + " Power: " + Tools[i].power + " Cooldown: " + Tools[i].cooldownTime;
			}
			return str;
		}
		// Token: 0x04000E81 RID: 3713
		public VerbTracker verbTracker;
	}
}
