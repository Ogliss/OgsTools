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
	public class Hive : RimWorld.Hive
	{
		public HiveDefExtension Ext => this.def.HasModExtension<HiveDefExtension>() ? this.def.GetModExtension<HiveDefExtension>() : null;

		private CompSpawnerPawn pawnSpawner;
		public new CompSpawnerPawn PawnSpawner => pawnSpawner ??= base.GetComp<CompSpawnerPawn>();

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
			if (this.comps != null)
			{
				for (int i = 0; i < this.comps.Count; i++)
				{
					this.comps[i].PostPostApplyDamage(dinfo, totalDamageDealt);
				}
			}
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

		public static readonly string MemoAssaultOnSpawn = "AssaultOnSpawn";
	}
}
