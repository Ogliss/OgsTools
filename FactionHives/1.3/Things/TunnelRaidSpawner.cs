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
    // Token: 0x020006E5 RID: 1765
    [StaticConstructorOnStartup]
    public class TunnelRaidSpawner : ThingWithComps, IThingHolder
    {
        public TunnelRaidSpawner()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            ResetStaticData();
        }

        // Token: 0x060024F3 RID: 9459 RVA: 0x00116CE3 File Offset: 0x001150E3
        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        // Token: 0x060024F4 RID: 9460 RVA: 0x00116CEB File Offset: 0x001150EB
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        protected ThingOwner innerContainer;

        // Token: 0x06002625 RID: 9765 RVA: 0x001221C0 File Offset: 0x001205C0
        public static void ResetStaticData()
        {
            TunnelRaidSpawner.filthTypes.Clear();
            TunnelRaidSpawner.filthTypes.Add(RimWorld.ThingDefOf.Filth_Dirt);
            TunnelRaidSpawner.filthTypes.Add(RimWorld.ThingDefOf.Filth_Dirt);
            TunnelRaidSpawner.filthTypes.Add(RimWorld.ThingDefOf.Filth_Dirt);
            TunnelRaidSpawner.filthTypes.Add(RimWorld.ThingDefOf.Filth_RubbleRock);
        }

        // Token: 0x06002626 RID: 9766 RVA: 0x00122214 File Offset: 0x00120614
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.secondarySpawnTick, "secondarySpawnTick", 0, false);
            Scribe_Values.Look<bool>(ref this.spawnHive, "spawnHive", true, false);
            Scribe_Values.Look<float>(ref this.initialPoints, "insectsPoints", 0f, false);
            Scribe_Values.Look<bool>(ref this.spawnedByInfestationThingComp, "spawnedByInfestationThingComp", false, false);
            Scribe_Defs.Look(ref factiondef, "factiondef");
            Scribe_References.Look(ref faction, "faction");
            Scribe_Collections.Look(ref spawnablePawnKinds, "spawnablePawnKinds");
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }

        // Token: 0x06002627 RID: 9767 RVA: 0x00122274 File Offset: 0x00120674
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.secondarySpawnTick = Find.TickManager.TicksGame + this.ResultSpawnDelay.RandomInRange.SecondsToTicks();
            }
            this.CreateSustainer();
        }

        // Token: 0x06002628 RID: 9768 RVA: 0x001222BC File Offset: 0x001206BC
        public override void Tick()
        {
            if (base.Spawned)
            {
                this.sustainer.Maintain();
                Vector3 vector = base.Position.ToVector3Shifted();
                IntVec3 c;
                // throws dust and filth 
                Rand.PushState();
                if (Rand.MTBEventOccurs(TunnelRaidSpawner.FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableCellNear(base.Position, base.Map, TunnelRaidSpawner.FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c, 999999))
                {
                    FilthMaker.TryMakeFilth(c, base.Map, TunnelRaidSpawner.filthTypes.RandomElement<ThingDef>(), 1);
                }
                if (Rand.MTBEventOccurs(TunnelRaidSpawner.DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
                {
                    FleckMaker.ThrowDustPuffThick(new Vector3(vector.x, 0f, vector.z)
                    {
                        y = AltitudeLayer.MoteOverhead.AltitudeFor()
                    }, base.Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
                }
                Rand.PopState();
                if (this.secondarySpawnTick <= Find.TickManager.TicksGame)
                {
                    this.sustainer.End();
                    List<Pawn> list = new List<Pawn>();
                    SpawnThings(out list);
                    this.Destroy(DestroyMode.Vanish);
                }
            }
        }

        public virtual void SpawnThings(out List<Pawn> list)
        {
            list = new List<Pawn>();
            Map map = base.Map;
            IntVec3 position = base.Position;
            // iif initalPoints > 0 spawn until all points are used
            if ((initialPoints > 0f) && !this.spawnablePawnKinds.NullOrEmpty())
            {
            //    Log.Message("generating pawns");
                initialPoints = Mathf.Max(initialPoints, this.spawnablePawnKinds.Min((PawnGenOption x) => x.Cost));
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
                    if (!this.spawnablePawnKinds.Where((PawnGenOption x) => x.Cost <= pointsLeft).TryRandomElementByWeight(x => x.selectionWeight, out result2))
                    {
                        break;
                    }
                    Pawn pawn = PawnGenerator.GeneratePawn(result2.kind, SpawnedFaction);
                    GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(position, map, 2), map);
                    pawn.mindState.spawnedByInfestationThingComp = spawnedByInfestationThingComp;
                    list.Add(pawn);
                }
            }
            if (!this.innerContainer.NullOrEmpty())
            {
                /*
                if (this.innerContainer.Any(x => x is Pawn))
                {
                    foreach (var item in this.innerContainer.Where(x => x is Pawn))
                    {
                        list.Add(item as Pawn);
                    }
                }
                */

                this.innerContainer.TryDropAll(position, map, ThingPlaceMode.Near, null, x => x.Walkable(map) && position.DistanceTo(x) > 2);
            }
            if (list.Any())
            {
                MakeLord(lordJobType, list);
            }
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
                /*
                LordJob lordJob = new LordJob_AssaultColony(Faction, false, false, false, false, false);
                LordMaker.MakeNewLord(Faction, lordJob, map, list);
                */
            }
        }

        // Token: 0x06002629 RID: 9769 RVA: 0x001225E4 File Offset: 0x001209E4
        public override void Draw()
        {
            Rand.PushState();
            Rand.Seed = this.thingIDNumber;
            for (int i = 0; i < 6; i++)
            {
                this.DrawDustPart(Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f) * (float)Rand.Sign * 4f, Rand.Range(1f, 1.5f));
            }
            Rand.PopState();
        }

        // Token: 0x0600262A RID: 9770 RVA: 0x00122658 File Offset: 0x00120A58
        private void DrawDustPart(float initialAngle, float speedMultiplier, float scale)
        {
            float num = (Find.TickManager.TicksGame - this.secondarySpawnTick).TicksToSeconds();
            Vector3 pos = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
            Rand.PushState();
            pos.y += 0.046875f * Rand.Range(0f, 1f);
            Rand.PopState();
            Color value = new Color(0.470588237f, 0.384313732f, 0.3254902f, 0.7f);
            TunnelRaidSpawner.matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale);
            Graphics.DrawMesh(MeshPool.plane10, matrix, TunnelRaidSpawner.TunnelMaterial, 0, null, 0, TunnelRaidSpawner.matPropertyBlock);
        }

        // Token: 0x0600262B RID: 9771 RVA: 0x0012271A File Offset: 0x00120B1A
        private void CreateSustainer()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                SoundDef tunnel = SoundDefOf.Tunnel;
                this.sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            });
        }

        public int SpawnTick
        {
            get
            {
                return secondarySpawnTick;
            }
            set
            {
                secondarySpawnTick = value;
            }
        }
        public Faction SpawnedFaction
        {
            get
            {
                if (faction == null && factiondef != null)
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
        public FactionDef factiondef = null;
        private Faction faction = null;
        // Token: 0x04001574 RID: 5492
        private int secondarySpawnTick;

        // Token: 0x04001575 RID: 5493
        public bool spawnHive = true;

        // Token: 0x04001576 RID: 5494
        public float initialPoints;

        // Token: 0x04001577 RID: 5495
        public bool spawnedByInfestationThingComp;

        // Token: 0x04001578 RID: 5496
        private Sustainer sustainer;

        // Token: 0x04001579 RID: 5497
        private static MaterialPropertyBlock matPropertyBlock = new MaterialPropertyBlock();

        // Token: 0x0400157A RID: 5498
        public FloatRange ResultSpawnDelay = new FloatRange(3f, 6f);

        // Token: 0x0400157B RID: 5499
        [TweakValue("Gameplay", 0f, 1f)]
        private static float DustMoteSpawnMTB = 0.2f;

        // Token: 0x0400157C RID: 5500
        [TweakValue("Gameplay", 0f, 1f)]
        private static float FilthSpawnMTB = 0.3f;

        // Token: 0x0400157D RID: 5501
        [TweakValue("Gameplay", 0f, 10f)]
        private static float FilthSpawnRadius = 3f;
        public Type lordJobType = typeof(LordJob_AssaultColony);
        // Token: 0x0400157E RID: 5502
        private static readonly Material TunnelMaterial = MaterialPool.MatFrom("Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent);
        public List<PawnGenOption> spawnablePawnKinds = new List<PawnGenOption>();
        // Token: 0x0400157F RID: 5503
        private static List<ThingDef> filthTypes = new List<ThingDef>();
    }
}
