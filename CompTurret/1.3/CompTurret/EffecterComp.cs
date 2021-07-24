using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CompTurret
{
	// Token: 0x02000434 RID: 1076 MuvLuvBeta.EffecterComp
	public class EffecterComp : Effecter
	{
		public EffecterComp(EffecterDef def) : base(def)
		{
			this.def = def;
			for (int i = 0; i < def.children.Count; i++)
			{
				this.children.Add(def.children[i].Spawn(this));
			}
		}

		// Token: 0x06002071 RID: 8305 RVA: 0x000C6CC0 File Offset: 0x000C4EC0
		public void EffectTick(CompTurretGun A, TargetInfo B)
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				if (this.children[i] is SubEffecterComp subEffecterComp)
				{
					subEffecterComp.SubEffectTick(A, B);
				}
			}
		}

		// Token: 0x06002072 RID: 8306 RVA: 0x000C6CF8 File Offset: 0x000C4EF8
		public void Trigger(CompTurretGun A, TargetInfo B)
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				if (this.children[i] is SubEffecterComp subEffecterComp)
				{
					subEffecterComp.SubTrigger(A, B);
				}
			}
		}

	}
}
