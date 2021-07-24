using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
namespace ExtraHives
{

//	[StaticConstructorOnStartup]
	public class TunnelHiveSpawner : ThingWithComps
	{

		public TunnelExtension Ext => this.def.HasModExtension<TunnelExtension>() ? this.def.GetModExtension<TunnelExtension>() : null;
		public Faction SpawnedFaction
		{
			get
			{
				if (faction == null && (Ext != null && Ext.Faction != null))
				{
					faction = Find.FactionManager.FirstFactionOfDef(Ext.Faction);
				}
				if (faction == null && factiondef!=null)
				{
					faction = Find.FactionManager.FirstFactionOfDef(factiondef);
				}
				return faction;
			}
			set
			{
				faction = value;
			}
		}

		public float TimeRemaining
        {
            get
            {
				return Math.Max(Mathf.InverseLerp(secondarySpawnTick, spawnTick, Find.TickManager.TicksGame), 0.0001f);
            }
        }

		public override void Tick()
		{

			if (!base.Spawned)
			{
				return;
			}
			sustainer.Maintain();
			Vector3 vector = base.Position.ToVector3Shifted();
			TargetInfo localTarget = new TargetInfo(this);
			Rand.PushState();
			if (Rand.MTBEventOccurs(FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableCellNear(base.Position, base.Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out IntVec3 result) && !filthTypes.NullOrEmpty())
			{
				FilthMaker.TryMakeFilth(result, base.Map, filthTypes.RandomElement());
			}
			if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
			{
				Vector3 loc = new Vector3(vector.x, 0f, vector.z);
				loc.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				FleckMaker.ThrowDustPuffThick(loc, base.Map, Rand.Range(1.5f, 3f), Ext.dustColor ?? new Color(1f, 1f, 1f, 2.5f));
                if (Ext.thowSparksinDust)
				{
                    if (Rand.MTBEventOccurs((EMPMoteSpawnMTB * TimeRemaining), 1f, 0.25f))
					{
						FleckMaker.ThrowMicroSparks(loc, base.Map);
					}
				}
			}
			if (Ext.effecter != null)
			{
				if (Rand.MTBEventOccurs((EMPMoteSpawnMTB * TimeRemaining), 0.5f, 0.25f))
				{
					if (this.Effecter == null && Ext.effecter != null)
					{
						this.Effecter = new Effecter(Ext.effecter);
					}
					if (Effecter != null)
					{
						Effecter.EffectTick(localTarget, localTarget);
					}
					else
					{
						this.Effecter.EffectTick(localTarget, localTarget);
					}
				}
			}
			Rand.PopState();
			if (secondarySpawnTick > Find.TickManager.TicksGame)
			{
				return;
			}
            if (this.Effecter!=null)
			{
				this.Effecter.Cleanup();
			}
			sustainer.End();
			Map map = base.Map;
			IntVec3 position = base.Position;
			Destroy();
			if (Ext.strikespreexplode)
			{
				FireEvent(map, position);
			}
			if (Ext.explodesprespawn)
			{
				GenExplosion.DoExplosion(position, map, Ext.blastradius, Ext.damageDef, null, -1, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false);

			}
			Hive obj = null;
			if (spawnHive)
			{
				obj = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(this.Ext.HiveDef), position, map);
				obj.SetFaction(SpawnedFaction);
				obj.questTags = questTags;
				foreach (CompSpawner comp in obj.GetComps<CompSpawner>())
				{
					if (comp.PropsSpawner.thingToSpawn == RimWorld.ThingDefOf.InsectJelly)
					{
						comp.TryDoSpawn();
						break;
					}
				}
			}
			List<Pawn> list = new List<Pawn>();
			if (initialPoints > 0f)
			{
				initialPoints = Mathf.Max(initialPoints, this.Ext.HiveDef.GetCompProperties<CompProperties_SpawnerPawn>().spawnablePawnKinds.Min((PawnGenOption x) => x.Cost));
				float pointsLeft = initialPoints;
				int num = 0;
				PawnGenOption result2;
				for (; pointsLeft > 0f; pointsLeft -= result2.Cost)
				{
					num++;
					if (num > 1000)
					{
						Log.Error("Too many iterations.");
						break;
					}
					if (!this.Ext.HiveDef.GetCompProperties<CompProperties_SpawnerPawn>().spawnablePawnKinds.Where((PawnGenOption x) => x.Cost <= pointsLeft).TryRandomElementByWeight(x => x.selectionWeight, out result2))
					{
						break;
					}
					Pawn pawn = PawnGenerator.GeneratePawn(result2.kind, SpawnedFaction);
					GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(position, map, 2), map);
					pawn.mindState.spawnedByInfestationThingComp = spawnedByInfestationThingComp;
					list.Add(pawn);
				}
			}
			if (list.Any())
			{
				this.MakeLord(lordJobType, list);
			}
		}

		// Token: 0x0600139E RID: 5022 RVA: 0x00096118 File Offset: 0x00094518
		public void FireEvent(Map map, IntVec3 strikeLoc)
		{
			if (!strikeLoc.IsValid)
			{
				strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
			}
			Mesh boltMesh = LightningBoltMeshPool.RandomBoltMesh;
			if (!strikeLoc.Fogged(map))
			{
				Vector3 loc = strikeLoc.ToVector3Shifted();
				for (int i = 0; i < 4; i++)
				{
					FleckMaker.ThrowSmoke(loc, map, 1.5f);
					FleckMaker.ThrowMicroSparks(loc, map);
					FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
				}
			}
			SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map, false), MaintenanceType.None);
			SoundDefOf.Thunder_OnMap.PlayOneShot(info);
			EventDraw(map, strikeLoc, boltMesh);
		}


		public override void Draw()
		{
			Rand.PushState();
			Rand.Seed = thingIDNumber;
			for (int i = 0; i < 6; i++)
			{
				DrawDustPart(Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f) * (float)Rand.Sign * 4f, Rand.Range(1f, 1.5f));
			}
			Rand.PopState();
		}

		// Token: 0x0600139F RID: 5023 RVA: 0x00096229 File Offset: 0x00094629
		public void EventDraw(Map map, IntVec3 strikeLoc, Mesh boltMesh)
		{
			Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(TunnelHiveSpawnerStatic.LightningMat, 3f), 0);
		}

		private void DrawDustPart(float initialAngle, float speedMultiplier, float scale)
		{
			float num = (Find.TickManager.TicksGame - secondarySpawnTick).TicksToSeconds();
			Vector3 pos = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
			Rand.PushState();
			pos.y += 0.0454545468f * Rand.Range(0f, 1f);
			Rand.PopState();
			Color value = new Color(0.470588237f, 98f / 255f, 83f / 255f, 0.7f);
			TunnelHiveSpawnerStatic.matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
			Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale);
			Graphics.DrawMesh(MeshPool.plane10, matrix, TunnelHiveSpawnerStatic.TunnelMaterial, 0, null, 0, TunnelHiveSpawnerStatic.matPropertyBlock);
		}

		public virtual void MakeLord(Type lordJobType, List<Pawn> list)
		{

			Map map = base.Map;
			IntVec3 position = base.Position;
			if (list.Any())
			{
				LordMaker.MakeNewLord(SpawnedFaction, Activator.CreateInstance(lordJobType, new object[]
				{
				   SpawnedFaction, false, false, false, false, false
				}) as LordJob, map, null);

			}
		}

		private void CreateSustainer()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				SoundDef tunnel = Ext.soundSustainer ?? SoundDefOf.Tunnel;
				sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			});
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			ResetStaticData();
			if (!respawningAfterLoad)
			{
				if (Ext != null)
				{
					if (Ext.spawnWavePoints > 0)
					{
						this.initialPoints = Ext.spawnWavePoints;
					}
				}
				secondarySpawnTick = Find.TickManager.TicksGame + ResultSpawnDelay.RandomInRange.SecondsToTicks();
				spawnTick = Find.TickManager.TicksGame;
			}
			CreateSustainer();
		}
		public static void ResetStaticData()
		{
			filthTypes.Clear();
			filthTypes.Add(RimWorld.ThingDefOf.Filth_Dirt);
			filthTypes.Add(RimWorld.ThingDefOf.Filth_Dirt);
			filthTypes.Add(RimWorld.ThingDefOf.Filth_Dirt);
			filthTypes.Add(RimWorld.ThingDefOf.Filth_RubbleRock);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref secondarySpawnTick, "secondarySpawnTick", 0);
			Scribe_Values.Look(ref spawnTick, "spawnTick", 0);
			Scribe_Values.Look(ref spawnHive, "spawnHive", defaultValue: true);
			Scribe_Values.Look(ref initialPoints, "insectsPoints", 0f);
			Scribe_Values.Look(ref spawnedByInfestationThingComp, "spawnedByInfestationThingComp", defaultValue: false);
			Scribe_Defs.Look(ref factiondef, "factiondef");
			Scribe_References.Look(ref faction, "faction");
			Scribe_Deep.Look(ref hive, "hive", null);
		}

		public Type lordJobType = typeof(LordJob_AssaultColony);
		public Faction faction = null;
		public FactionDef factiondef = null;
		public FloatRange ResultSpawnDelay = new FloatRange(26f, 30f);
		public bool spawnHive = true;
		public Hive hive = null;
		public float initialPoints;
		public bool spawnedByInfestationThingComp;
		private Sustainer sustainer;
		private Effecter Effecter;
		private int secondarySpawnTick;
		private int spawnTick;
		private static List<ThingDef> filthTypes = new List<ThingDef>();

		[TweakValue("Gameplay", 0f, 1f)]
		private static float DustMoteSpawnMTB = 0.2f;

		[TweakValue("Gameplay", 0f, 1f)]
		private static float EMPMoteSpawnMTB = 1f;

		[TweakValue("Gameplay", 0f, 1f)]
		private static float FilthSpawnMTB = 0.3f;

		[TweakValue("Gameplay", 0f, 10f)]
		private static float FilthSpawnRadius = 3f;

	}

	[StaticConstructorOnStartup]
	public static class TunnelHiveSpawnerStatic
	{
		// Token: 0x04000C09 RID: 3081
		public static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt", -1);
		// Token: 0x04001579 RID: 5497
		public static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();
		// Token: 0x0400157E RID: 5502
		public static readonly Material TunnelMaterial = MaterialPool.MatFrom("Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent);
	}
}