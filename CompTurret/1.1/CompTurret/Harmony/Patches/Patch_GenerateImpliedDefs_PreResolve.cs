using HarmonyLib;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CompTurret.HarmonyInstance
{
	// Token: 0x02000019 RID: 25
	[HarmonyPatch(typeof(DefGenerator), "GenerateImpliedDefs_PreResolve", null)]
    public class Patch_GenerateImpliedDefs_PreResolve
	{
		// Token: 0x06000088 RID: 136 RVA: 0x00004224 File Offset: 0x00002424
		public static void Postfix()
		{
			JobDef jobDef;
			jobDef = new JobDef();
			jobDef.defName = "CompTurretReload";
			jobDef.driverClass = typeof(JobDriver_ReloadCompTurret);
			jobDef.reportString = "reloading.";
			DefGenerator.AddImpliedDef<JobDef>(jobDef);


		}
	}
}
