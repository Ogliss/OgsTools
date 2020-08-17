using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
namespace ExtraHives
{

	[StaticConstructorOnStartup]
	public class TunnelHiveSpawner : ThingWithComps
	{
		public TunnelExtension Ext => this.def.HasModExtension<TunnelExtension>() ? this.def.GetModExtension<TunnelExtension>() : null;
		private int secondarySpawnTick;
		public bool spawnHive = true;
		public Hive hive = null;

		public float initialPoints;

		public bool spawnedByInfestationThingComp;

		private Sustainer sustainer;

		private static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();

		public FloatRange ResultSpawnDelay = new FloatRange(26f, 30f);

		[TweakValue("Gameplay", 0f, 1f)]
		private static float DustMoteSpawnMTB = 0.2f;

		[TweakValue("Gameplay", 0f, 1f)]
		private static float FilthSpawnMTB = 0.3f;

		[TweakValue("Gameplay", 0f, 10f)]
		private static float FilthSpawnRadius = 3f;

		private static readonly Material TunnelMaterial = MaterialPool.MatFrom("Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent);

		private static List<ThingDef> filthTypes = new List<ThingDef>();

		public Faction faction = null;
		public FactionDef factiondef = null;
		public new Faction Faction
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
			Scribe_Values.Look(ref spawnHive, "spawnHive", defaultValue: true);
			Scribe_Values.Look(ref initialPoints, "insectsPoints", 0f);
			Scribe_Values.Look(ref spawnedByInfestationThingComp, "spawnedByInfestationThingComp", defaultValue: false);
			Scribe_Defs.Look(ref factiondef, "factiondef");
			Scribe_References.Look(ref faction, "faction");
			Scribe_Deep.Look(ref hive, "hive", null);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			ResetStaticData();
			if (!respawningAfterLoad)
			{
				if (Ext!=null)
				{
					if (Ext.spawnWavePoints>0)
					{
						this.initialPoints = Ext.spawnWavePoints;
					}
				}
				secondarySpawnTick = Find.TickManager.TicksGame + ResultSpawnDelay.RandomInRange.SecondsToTicks();
			}
			CreateSustainer();
		}

		public override void Tick()
		{
			
			if (!base.Spawned)
			{
				return;
			}
			sustainer.Maintain();
			Vector3 vector = base.Position.ToVector3Shifted();
			if (Rand.MTBEventOccurs(FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableCellNear(base.Position, base.Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null, out IntVec3 result) && !filthTypes.NullOrEmpty())
			{
				FilthMaker.TryMakeFilth(result, base.Map, filthTypes.RandomElement());
			}
			if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
			{
				Vector3 loc = new Vector3(vector.x, 0f, vector.z);
				loc.y = AltitudeLayer.MoteOverhead.AltitudeFor();
				MoteMaker.ThrowDustPuffThick(loc, base.Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
			}
			if (secondarySpawnTick > Find.TickManager.TicksGame)
			{
				return;
			}
			sustainer.End();
			Map map = base.Map;
			IntVec3 position = base.Position;
			Destroy();
			Hive obj = null;
			if (spawnHive)
			{
				obj = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(this.Ext.HiveDef), position, map);
				obj.SetFaction(Faction);
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
			if ((initialPoints > 0f))
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
					if (!this.Ext.HiveDef.GetCompProperties<CompProperties_SpawnerPawn>().spawnablePawnKinds.Where((PawnGenOption x) => x.Cost <= pointsLeft).TryRandomElementByWeight(x=> x.selectionWeight, out result2))
					{
						break;
					}
					Pawn pawn = PawnGenerator.GeneratePawn(result2.kind, Faction);
					GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(position, map, 2), map);
					pawn.mindState.spawnedByInfestationThingComp = spawnedByInfestationThingComp;
					list.Add(pawn);
				}
			}
			if (list.Any())
			{


			//	Log.Message("make new lord of " + Faction + " for " + obj);
				LordMaker.MakeNewLord(Faction, new LordJob_AssaultColony(new SpawnedPawnParams
				{
					aggressive = false,
					defendRadius = 50,
					defSpot = position,
					spawnerThing = (Thing)obj ?? this
				}), map, list);
			//	Log.Message("made attacking lord");
			}
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

		private void DrawDustPart(float initialAngle, float speedMultiplier, float scale)
		{
			float num = (Find.TickManager.TicksGame - secondarySpawnTick).TicksToSeconds();
			Vector3 pos = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
			pos.y += 0.0454545468f * Rand.Range(0f, 1f);
			Color value = new Color(0.470588237f, 98f / 255f, 83f / 255f, 0.7f);
			matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
			Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale);
			Graphics.DrawMesh(MeshPool.plane10, matrix, TunnelMaterial, 0, null, 0, matPropertyBlock);
		}

		private void CreateSustainer()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				SoundDef tunnel = SoundDefOf.Tunnel;
				sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
			});
		}
	}

}