using System;
using UnityEngine;
using Verse;

namespace CompTurret
{
	// Token: 0x02000DC8 RID: 3528
	public class Command_CompTurretReloadable : Command_VerbTarget
	{
		// Token: 0x06005671 RID: 22129 RVA: 0x001CE6CF File Offset: 0x001CC8CF
		public Command_CompTurretReloadable(CompTurret comp)
		{
			this.comp = comp;
		}

		// Token: 0x17000F3A RID: 3898
		// (get) Token: 0x06005672 RID: 22130 RVA: 0x001CE6DE File Offset: 0x001CC8DE
		public override string TopRightLabel
		{
			get
			{
				return this.comp.LabelRemaining;
			}
		}

		// Token: 0x17000F3B RID: 3899
		// (get) Token: 0x06005673 RID: 22131 RVA: 0x001CE6EC File Offset: 0x001CC8EC
		public override Color IconDrawColor
		{
			get
			{
				Color? color = this.overrideColor;
				if (color == null)
				{
					return base.IconDrawColor;
				}
				return color.GetValueOrDefault();
			}
		}

		// Token: 0x06005674 RID: 22132 RVA: 0x001CE717 File Offset: 0x001CC917
		public override void GizmoUpdateOnMouseover()
		{
			this.verb.DrawHighlight(this.verb.caster);
		}

		// Token: 0x06005675 RID: 22133 RVA: 0x001CE734 File Offset: 0x001CC934
		public override bool GroupsWith(Gizmo other)
		{
			return false;
			if (!base.GroupsWith(other))
			{
				return false;
			}
			Command_CompTurretReloadable command_Reloadable = other as Command_CompTurretReloadable;
			return command_Reloadable != null && this.comp.parent.def == command_Reloadable.comp.parent.def;
		}

		// Token: 0x04003007 RID: 12295
		private readonly CompTurret comp;

		// Token: 0x04003008 RID: 12296
		public Color? overrideColor;
	}
}
