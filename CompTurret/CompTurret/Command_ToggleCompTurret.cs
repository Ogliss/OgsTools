using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace CompTurret
{
	// Token: 0x02000398 RID: 920
	public class Command_ToggleCompTurret : Command
	{
		public Command_ToggleCompTurret(CompTurret comp)
		{
			this.comp = comp;
		}
		public override SoundDef CurActivateSound
		{
			get
			{
				if (this.isActive())
				{
					return this.turnOffSound;
				}
				return this.turnOnSound;
			}
		}
		public override string TopRightLabel
		{
			get
			{
				return this.comp.UseAmmo ? this.comp.LabelRemaining : string.Empty;
			}
		}

		// Token: 0x06001B67 RID: 7015 RVA: 0x000A8A26 File Offset: 0x000A6C26
		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			this.toggleAction();
		}

		// Token: 0x06001B68 RID: 7016 RVA: 0x000A8A3C File Offset: 0x000A6C3C
		public override GizmoResult GizmoOnGUI(Vector2 loc, float maxWidth)
		{
			GizmoResult result = base.GizmoOnGUI(loc, maxWidth);
			Rect rect = new Rect(loc.x, loc.y, this.GetWidth(maxWidth), 75f);
			Rect position = new Rect(rect.x , rect.y, 24f, 24f);
			Texture2D image = this.isActive() ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex;
			GUI.DrawTexture(position, image);
			return result;
		}

		// Token: 0x06001B69 RID: 7017 RVA: 0x000A8ABC File Offset: 0x000A6CBC
		public override bool InheritInteractionsFrom(Gizmo other)
		{
			Command_ToggleCompTurret command_Toggle = other as Command_ToggleCompTurret;
			return command_Toggle != null && command_Toggle.isActive() == this.isActive();
		}

		public override void MergeWith(Gizmo other)
		{
			return;
			base.MergeWith(other);
		}

		// Token: 0x06005675 RID: 22133 RVA: 0x001CE734 File Offset: 0x001CC934
		public override bool GroupsWith(Gizmo other)
		{
			return false;
		}

		private readonly CompTurret comp;
		// Token: 0x04001038 RID: 4152
		public Func<bool> isActive;

		// Token: 0x04001039 RID: 4153
		public Action toggleAction;

		// Token: 0x0400103A RID: 4154
		public SoundDef turnOnSound = SoundDefOf.Checkbox_TurnedOn;

		// Token: 0x0400103B RID: 4155
		public SoundDef turnOffSound = SoundDefOf.Checkbox_TurnedOff;

		// Token: 0x0400103C RID: 4156
		public bool activateIfAmbiguous = true;
	}
}
