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
	// ExtraHives.Hive
	public class Hive : Building, IAttackTarget, ILoadReferenceable
	{
		public HiveDefExtension Ext => this.def.HasModExtension<HiveDefExtension>() ? this.def.GetModExtension<HiveDefExtension>() : null;

		public CompCanBeDormant CompDormant
		{
			get
			{
				return base.GetComp<CompCanBeDormant>();
			}
		}

		Thing IAttackTarget.Thing
		{
			get
			{
				return this;
			}
		}

		public float TargetPriorityFactor
		{
			get
			{
				return 0.4f;
			}
		}

		public LocalTargetInfo TargetCurrentlyAimingAt
		{
			get
			{
				return LocalTargetInfo.Invalid;
			}
		}

		public CompSpawnerPawn PawnSpawner
		{
			get
			{
				return base.GetComp<CompSpawnerPawn>();
			}
		}

		public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
		{
			if (!base.Spawned)
			{
				return true;
			}
			CompCanBeDormant comp = base.GetComp<CompCanBeDormant>();
			return comp != null && !comp.Awake;
		}

		public static void ResetStaticData()
		{
			Hive.spawnablePawnKinds.Clear();
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
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
			}
		}

		public override void Tick()
		{
			base.Tick();
            if (this.CompDormant != null)
            {
				if (base.Spawned && !this.CompDormant.Awake && !base.Position.Fogged(base.Map))
				{
					this.CompDormant.WakeUp();
				}
            }
		}

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

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
			{
				Lord lord = base.GetComp<CompSpawnerPawn>()?.Lord;
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
				Lord lord = base.GetComp<CompSpawnerPawn>()?.Lord;
				if (lord != null)
				{
					lord.ReceiveMemo(Hive.MemoBurnedBadly);
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

		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
            if (this.PawnSpawner != null)
			{
				if (this.PawnSpawner.spawnedPawns.Count > 0)
				{
					if (this.PawnSpawner.spawnedPawns.Any((Pawn p) => !p.Downed))
					{
						reason = this.def.label;
						return true;
					}
				}
			}
			reason = null;
			return false;
		}

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

		public const int PawnSpawnRadius = 2;
		public const float MaxSpawnedPawnsPoints = 500f;
		public const float InitialPawnsPoints = 200f;
		public static List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();
		public static readonly string MemoAttackedByEnemy = "HiveAttacked";
		public static readonly string MemoDeSpawned = "HiveDeSpawned";
		public static readonly string MemoBurnedBadly = "HiveBurnedBadly";
		public static readonly string MemoDestroyedNonRoofCollapse = "HiveDestroyedNonRoofCollapse";
		public static readonly string MemoAssaultOnSpawn = "AssaultOnSpawn";
	}
}
