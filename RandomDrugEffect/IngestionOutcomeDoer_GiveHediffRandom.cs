using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace RandomDrugEffect.IngestionOutcomeDoer_GiveHediffRandom
{
	// Token: 0x02000896 RID: 2198
	public class IngestionOutcomeDoer_GiveHediffRandom : IngestionOutcomeDoer
	{
		// Token: 0x06003563 RID: 13667 RVA: 0x00123774 File Offset: 0x00121974
		public override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			this.hediffDef = this.hediffDefs.RandomElement();
			Hediff hediff = HediffMaker.MakeHediff(this.hediffDef, pawn, null);
			float num;
			if (this.severity > 0f)
			{
				num = this.severity;
			}
			else
			{
				num = this.hediffDef.initialSeverity;
			}
			if (this.divideByBodySize)
			{
				num /= pawn.BodySize;
			}
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
			hediff.Severity = num;
			pawn.health.AddHediff(hediff, null, null, null);
		}

		// Token: 0x06003564 RID: 13668 RVA: 0x001237EE File Offset: 0x001219EE
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			if (parentDef.IsDrug && this.chance >= 1f)
			{
				foreach (StatDrawEntry statDrawEntry in this.hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
				{
					yield return statDrawEntry;
				}
				IEnumerator<StatDrawEntry> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x04001D13 RID: 7443
		public HediffDef hediffDef;
		public List<HediffDef> hediffDefs;

		// Token: 0x04001D14 RID: 7444
		public float severity = -1f;

		// Token: 0x04001D15 RID: 7445
		public ChemicalDef toleranceChemical;

		// Token: 0x04001D16 RID: 7446
		private bool divideByBodySize;
	}
}
