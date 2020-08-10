using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace ExtraHives
{
	// Token: 0x02000CA1 RID: 3233
	public class Hive : ThingWithComps, IAttackTarget, ILoadReferenceable
	{
		public HiveDefExtension Ext => this.def.HasModExtension<HiveDefExtension>() ? this.def.GetModExtension<HiveDefExtension>() : null;

		// Token: 0x17000DCE RID: 3534
		// (get) Token: 0x06004E1D RID: 19997 RVA: 0x001A404E File Offset: 0x001A224E
		public CompCanBeDormant CompDormant
		{
			get
			{
				return base.GetComp<CompCanBeDormant>();
			}
		}

		// Token: 0x17000DCF RID: 3535
		// (get) Token: 0x06004E1E RID: 19998 RVA: 0x00064602 File Offset: 0x00062802
		Thing IAttackTarget.Thing
		{
			get
			{
				return this;
			}
		}

		// Token: 0x17000DD0 RID: 3536
		// (get) Token: 0x06004E1F RID: 19999 RVA: 0x001A4056 File Offset: 0x001A2256
		public float TargetPriorityFactor
		{
			get
			{
				return 0.4f;
			}
		}

		// Token: 0x17000DD1 RID: 3537
		// (get) Token: 0x06004E20 RID: 20000 RVA: 0x001A405D File Offset: 0x001A225D
		public LocalTargetInfo TargetCurrentlyAimingAt
		{
			get
			{
				return LocalTargetInfo.Invalid;
			}
		}

		// Token: 0x17000DD2 RID: 3538
		// (get) Token: 0x06004E21 RID: 20001 RVA: 0x001A4064 File Offset: 0x001A2264
		public CompSpawnerPawn PawnSpawner
		{
			get
			{
				return base.GetComp<CompSpawnerPawn>();
			}
		}

		// Token: 0x06004E22 RID: 20002 RVA: 0x001A406C File Offset: 0x001A226C
		public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
		{
			if (!base.Spawned)
			{
				return true;
			}
			CompCanBeDormant comp = base.GetComp<CompCanBeDormant>();
			return comp != null && !comp.Awake;
		}

		// Token: 0x06004E23 RID: 20003 RVA: 0x001A4098 File Offset: 0x001A2298
		public static void ResetStaticData()
		{
			Hive.spawnablePawnKinds.Clear();
		}

		// Token: 0x06004E24 RID: 20004 RVA: 0x001A40D1 File Offset: 0x001A22D1
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			if (base.Faction == null)
			{
				Faction faction = Faction.OfInsects;
				if (Ext!=null)
				{
					if (Ext.Faction!=null)
					{
						Faction faction2 = Find.FactionManager.FirstFactionOfDef(Ext.Faction);
						if (faction2 != null)
						{
							faction = faction2;
						}
					}
				}
				this.SetFaction(faction, null);
				base.SpawnSetup(map, respawningAfterLoad);
			}
		}

		// Token: 0x06004E25 RID: 20005 RVA: 0x001A40EF File Offset: 0x001A22EF
		public override void Tick()
		{
			base.Tick();
			if (base.Spawned && !this.CompDormant.Awake && !base.Position.Fogged(base.Map))
			{
				this.CompDormant.WakeUp();
			}
		}

		// Token: 0x06004E26 RID: 20006 RVA: 0x001A412C File Offset: 0x001A232C
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			List<Lord> lords = map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].ReceiveMemo(Hive.MemoDeSpawned);
			}
			HiveUtility.Notify_HiveDespawned(this, map);
		}

		// Token: 0x06004E27 RID: 20007 RVA: 0x001A417C File Offset: 0x001A237C
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			if (!this.questTags.NullOrEmpty<string>())
			{
				bool flag = false;
				List<Thing> list = base.Map.listerThings.ThingsOfDef(this.def);
				for (int i = 0; i < list.Count; i++)
				{
					Hive hive;
					if ((hive = (list[i] as Hive)) != null && hive != this && hive.CompDormant.Awake && !hive.questTags.NullOrEmpty<string>() && QuestUtility.AnyMatchingTags(hive.questTags, this.questTags))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					QuestUtility.SendQuestTargetSignals(this.questTags, "AllHivesDestroyed");
				}
			}
			base.Destroy(mode);
		}

		// Token: 0x06004E28 RID: 20008 RVA: 0x001A4224 File Offset: 0x001A2424
		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
			{
				Lord lord = base.GetComp<CompSpawnerPawn>().Lord;
				if (lord != null)
				{
					lord.ReceiveMemo(Hive.MemoAttackedByEnemy);
				}
				if (Main.CrashedShipsExtension)
				{
					CrashedShipsExtensionMemoAttackedByEnemy();
				}
			}
			if (dinfo.Def == DamageDefOf.Flame && (float)this.HitPoints < (float)base.MaxHitPoints * 0.3f)
			{
				Lord lord2 = base.GetComp<CompSpawnerPawn>().Lord;
				if (lord2 != null)
				{
					lord2.ReceiveMemo(Hive.MemoBurnedBadly);
				}
				if (Main.CrashedShipsExtension)
				{
					CrashedShipsExtensionMemoBurnedBadly();
				}
			}
			base.PostApplyDamage(dinfo, totalDamageDealt);
		}

		public void CrashedShipsExtensionMemoAttackedByEnemy()
		{
			Lord lord = base.GetComp<CrashedShipsExtension.CompSpawnerOnDamaged>()?.Lord;
			if (lord != null)
			{
				lord.ReceiveMemo(Hive.MemoAttackedByEnemy);
			}
		}
		public void CrashedShipsExtensionMemoBurnedBadly()
		{
			Lord lord = base.GetComp<CrashedShipsExtension.CompSpawnerOnDamaged>()?.Lord;
			if (lord != null)
			{
				lord.ReceiveMemo(Hive.MemoBurnedBadly);
			}
		}

		// Token: 0x06004E29 RID: 20009 RVA: 0x001A42B8 File Offset: 0x001A24B8
		public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
		{
			if (base.Spawned && (dinfo == null || dinfo.Value.Category != DamageInfo.SourceCategory.Collapse))
			{
				List<Lord> lords = base.Map.lordManager.lords;
				for (int i = 0; i < lords.Count; i++)
				{
					lords[i].ReceiveMemo(Hive.MemoDestroyedNonRoofCollapse);
				}
			}
			base.Kill(dinfo, exactCulprit);
		}

		// Token: 0x06004E2A RID: 20010 RVA: 0x001A4328 File Offset: 0x001A2528
		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (this.PawnSpawner.spawnedPawns.Count > 0)
			{
				if (this.PawnSpawner.spawnedPawns.Any((Pawn p) => !p.Downed))
				{
					reason = this.def.label;
					return true;
				}
			}
			reason = null;
			return false;
		}

		// Token: 0x06004E2B RID: 20011 RVA: 0x001A438C File Offset: 0x001A258C
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			foreach (Gizmo gizmo2 in QuestUtility.GetQuestRelatedGizmos(this))
			{
				yield return gizmo2;
			}
			yield break;
		}

		// Token: 0x06004E2C RID: 20012 RVA: 0x001A439C File Offset: 0x001A259C
		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode != LoadSaveMode.Saving)
			{
				bool flag = false;
				Scribe_Values.Look<bool>(ref flag, "active", false, false);
				if (flag)
				{
					this.CompDormant.WakeUp();
				}
			}
		}

		// Token: 0x04002BDC RID: 11228
		public const int PawnSpawnRadius = 2;

		// Token: 0x04002BDD RID: 11229
		public const float MaxSpawnedPawnsPoints = 500f;

		// Token: 0x04002BDE RID: 11230
		public const float InitialPawnsPoints = 200f;

		// Token: 0x04002BDF RID: 11231
		public static List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();

		// Token: 0x04002BE0 RID: 11232
		public static readonly string MemoAttackedByEnemy = "HiveAttacked";

		// Token: 0x04002BE1 RID: 11233
		public static readonly string MemoDeSpawned = "HiveDeSpawned";

		// Token: 0x04002BE2 RID: 11234
		public static readonly string MemoBurnedBadly = "HiveBurnedBadly";

		// Token: 0x04002BE3 RID: 11235
		public static readonly string MemoDestroyedNonRoofCollapse = "HiveDestroyedNonRoofCollapse";

		public static readonly string MemoAssaultOnSpawn = "AssaultOnSpawn";
	}
}
