using Verse;

namespace AdeptusMechanicus
{
    public class CompOversizedWeapon : ThingComp
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public CompProperties_OversizedWeapon Props
		{
			get
			{
				return this.props as CompProperties_OversizedWeapon;
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002070 File Offset: 0x00000270
		public CompOversizedWeapon()
		{
			bool flag = !(this.props is CompProperties_OversizedWeapon);
			if (flag)
			{
				this.props = new CompProperties_OversizedWeapon();
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000003 RID: 3 RVA: 0x000020A8 File Offset: 0x000002A8
		public CompEquippable GetEquippable
		{
			get
			{
				ThingWithComps parent = this.parent;
				bool flag = parent == null;
				CompEquippable result;
				if (flag)
				{
					result = null;
				}
				else
				{
					result = parent.GetComp<CompEquippable>();
				}
				return result;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000004 RID: 4 RVA: 0x000020D4 File Offset: 0x000002D4
		public Pawn GetPawn
		{
			get
			{
				CompEquippable getEquippable = this.GetEquippable;
				bool flag = getEquippable == null;
				Pawn result;
				if (flag)
				{
					result = null;
				}
				else
				{
					VerbTracker verbTracker = getEquippable.verbTracker;
					bool flag2 = verbTracker == null;
					if (flag2)
					{
						result = null;
					}
					else
					{
						Verb primaryVerb = verbTracker.PrimaryVerb;
						bool flag3 = primaryVerb == null;
						if (flag3)
						{
							result = null;
						}
						else
						{
							result = primaryVerb.CasterPawn;
						}
					}
				}
				return result;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000005 RID: 5 RVA: 0x00002134 File Offset: 0x00000334
		public bool IsEquipped
		{
			get
			{
				bool flag = Find.TickManager.TicksGame % 60 != 0;
				bool result;
				if (flag)
				{
					result = this.isEquipped;
				}
				else
				{
					this.isEquipped = (this.GetPawn != null);
					result = this.isEquipped;
				}
				return result;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000006 RID: 6 RVA: 0x0000217C File Offset: 0x0000037C
		// (set) Token: 0x06000007 RID: 7 RVA: 0x00002194 File Offset: 0x00000394
		public bool FirstAttack
		{
			get
			{
				return this.firstAttack;
			}
			set
			{
				this.firstAttack = value;
			}
		}

		// Token: 0x04000001 RID: 1
		private bool isEquipped;

		// Token: 0x04000002 RID: 2
		private bool firstAttack;
	}
}
