using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CompTurret
{
	// Token: 0x02000399 RID: 921
	public class Command_CompTurretVerbTarget : Command
	{
		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x06001B6B RID: 7019 RVA: 0x000A8B12 File Offset: 0x000A6D12
		public override Color IconDrawColor
		{
			get
			{
				if (this.verb.EquipmentSource != null)
				{
					return this.verb.EquipmentSource.DrawColor;
				}
				return base.IconDrawColor;
			}
		}

		// Token: 0x06001B6C RID: 7020 RVA: 0x000A8B38 File Offset: 0x000A6D38
		public override void GizmoUpdateOnMouseover()
		{
			if (!this.drawRadius)
			{
				return;
			}
			this.verb.verbProps.DrawRadiusRing(this.verb.caster.Position);
			if (!this.groupedVerbs.NullOrEmpty<Verb>())
			{
				foreach (Verb verb in this.groupedVerbs)
				{
					verb.verbProps.DrawRadiusRing(verb.caster.Position);
				}
			}
		}

		// Token: 0x06001B6D RID: 7021 RVA: 0x000A8BD0 File Offset: 0x000A6DD0
		public override void MergeWith(Gizmo other)
		{
			return;
			base.MergeWith(other);
			Command_CompTurretVerbTarget command_VerbTarget = other as Command_CompTurretVerbTarget;
			if (command_VerbTarget == null)
			{
				Log.ErrorOnce("Tried to merge Command_VerbTarget with unexpected type", 73406263, false);
				return;
			}
			if (this.groupedVerbs == null)
			{
				this.groupedVerbs = new List<Verb>();
			}
			this.groupedVerbs.Add(command_VerbTarget.verb);
			if (command_VerbTarget.groupedVerbs != null)
			{
				this.groupedVerbs.AddRange(command_VerbTarget.groupedVerbs);
			}
		}

		// Token: 0x06001B6E RID: 7022 RVA: 0x000A8C3C File Offset: 0x000A6E3C
		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			Targeter targeter = Find.Targeter;
		//	Log.Message("CasterIsPawn: " + this.verb.CasterIsPawn);
		//	if (targeter.targetingSource != null) Log.Message("targetingSource: " + targeter.targetingSource);
		//	if (targeter.targetingSource?.GetVerb?.verbProps != null) Log.Message("verbProps: " + targeter.targetingSource.GetVerb.verbProps);
			if (this.verb.CasterIsPawn && targeter.targetingSource != null && targeter.targetingSource.GetVerb.verbProps == this.verb.verbProps)
			{
				Pawn casterPawn = this.verb.CasterPawn;
				if (!targeter.IsPawnTargeting(casterPawn))
				{
				//	Log.Message("targetingSourceAdditionalPawns: ");
					targeter.targetingSourceAdditionalPawns.Add(casterPawn);
					return;
				}
			}
			else
			{
			//	Log.Message("BeginTargeting: ");
				Find.Targeter.BeginTargeting(gunTurret, null);
			}
			/*
			else
			{

			//	Log.Message("BeginTargeting: ");
				Find.Targeter.BeginTargeting(this.verb.targetParams, delegate (LocalTargetInfo target)
				{
					this.action(target);
				}, null, null, this.verb.CasterPawn);
			}
*/
		}

		public CompTurretGun gunTurret;
		// Token: 0x0400103D RID: 4157
		public Verb verb;
		// Token: 0x0400103E RID: 4158
		private List<Verb> groupedVerbs;

		// Token: 0x0400103F RID: 4159
		public bool drawRadius = true;
	}
}
