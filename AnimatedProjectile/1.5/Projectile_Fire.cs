using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AnimatedProjectile
{
    public class Projectile_Fire : Projectile_Anim
    {
        public bool SetsFires => this.Props != null && this.Props.setsFire;
        public bool Ignites => this.Props != null && this.Props.ignites;
        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            //    base.Impact(hitThing);
            Ignite();
        }

        public override void Tick()
        {
            base.Tick();

            pos = this.Position;
            checked
            {
                this.TicksforAppearence--;
                bool flag = this.TicksforAppearence == 0 && base.Map != null;
                if (flag)
                {
                    if (pos != IntVec3.Invalid)
                    {
                        if (pos != this.Position && SetsFires)
                        {
                            Rand.PushState();
                            if (Rand.Chance(this.Props.setFireChance * Traveled))
                            {
                                TrySpread();
                            }
                            Rand.PopState();
                        }
                    }
                    Rand.PushState();
                    if (Rand.Chance(0.75f * Traveled))
                    {
                        ThrowSmoke(this.DrawPos, base.Map, 0.5f * Traveled);
                        if (Traveled > 0.5f && Ignites)
                        {
                            if (Rand.Chance(this.Props.igniteChance * Traveled))
                            {
                                Ignite();
                            }
                        }
                    }
                    Rand.PopState();
                    this.TicksforAppearence = 6;
                }
            }
        }

        protected virtual void Ignite()
        {
            Map map = Map;
            Destroy();
            float ignitionChance = def.projectile.explosionChanceToStartFire;
            var radius = def.projectile.explosionRadius;
            var cellsToAffect = SimplePool<List<IntVec3>>.Get();
            cellsToAffect.Clear();
            cellsToAffect.AddRange(def.projectile.damageDef.Worker.ExplosionCellsToHit(Position, map, radius));

            FleckMaker.Static(Position, map, FleckDefOf.ExplosionFlash, radius * 4f);
            for (int i = 0; i < 4; i++)
            {
                FleckMaker.ThrowSmoke(Position.ToVector3Shifted() + Gen.RandomHorizontalVector(radius * 0.7f), map, radius * 0.6f);
            }

            Rand.PushState();
            if (Rand.Chance(ignitionChance))
                foreach (var vec3 in cellsToAffect)
                {
                    var fireSize = radius - vec3.DistanceTo(Position);
                    if (fireSize > 0.1f)
                    {
                        FireUtility.TryStartFireIn(vec3, map, fireSize, this.Launcher);
                    }
                }

            Rand.PopState();
            if (this.def.projectile.explosionEffect != null)
            {
                Effecter effecter = this.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(this.Position, map, false), new TargetInfo(this.Position, map, false));
                effecter.Cleanup();
            }
            GenExplosion.DoExplosion(this.Position, map, this.def.projectile.explosionRadius, this.def.projectile.damageDef, this.launcher, this.def.projectile.GetDamageAmount(1, null), this.def.projectile.GetArmorPenetration(1, null), this.def.projectile.soundExplode, this.equipmentDef, this.def, null, this.def.projectile.postExplosionSpawnThingDef, this.def.projectile.postExplosionSpawnChance, this.def.projectile.postExplosionSpawnThingCount, GasType.BlindSmoke, this.def.projectile.applyDamageToExplosionCellsNeighbors, this.def.projectile.preExplosionSpawnThingDef, this.def.projectile.preExplosionSpawnChance, this.def.projectile.preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff);
        }

        protected void TrySpread()
        {
            IntVec3 intVec = base.Position;
            bool flag;
            Rand.PushState();
            if (Rand.Chance(0.8f))
            {
                intVec = base.Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(1, 8)];
                flag = true;
            }
            else
            {
                intVec = base.Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(10, 20)];
                flag = false;
            }
            Rand.PopState();
            if (!intVec.InBounds(base.Map))
            {
                return;
            }
            Rand.PushState();
            if (Rand.Chance(FireUtility.ChanceToStartFireIn(intVec, base.Map)))
            {
                if (!flag)
                {
                    CellRect startRect = CellRect.SingleCell(base.Position);
                    CellRect endRect = CellRect.SingleCell(intVec);
                    if (!GenSight.LineOfSight(base.Position, intVec, base.Map, startRect, endRect, null))
                    {
                        return;
                    }
                    Spark spark = (Spark)GenSpawn.Spawn(ThingDefOf.Spark, base.Position, base.Map, WipeMode.Vanish);
                    spark.Launch(this, intVec, intVec, ProjectileHitFlags.All);
                }
                else
                {
                    FireUtility.TryStartFireIn(intVec, base.Map, 0.1f, this.Launcher);
                }
            }
            Rand.PopState();
        }

        public override Quaternion ExactRotation
        {
            get
            {
                var forward = destination - origin;
                forward.y = 0;
                return Quaternion.LookRotation(forward);
            }
        }

        public static void ThrowSmoke(Vector3 loc, Map map, float size)
        {
            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority)
            {
                return;
            }
            MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Mote_Smoke"), null);
            Rand.PushState();
            moteThrown.Scale = Rand.Range(1.5f, 2.5f) * size;
            moteThrown.rotationRate = Rand.Range(-30f, 30f);
            moteThrown.exactPosition = loc;
            moteThrown.SetVelocity((float)Rand.Range(30, 40), Rand.Range(0.5f, 0.7f));
            Rand.PopState();
            GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            if (Props.drawGlow)
            {
                string motename = !Props.drawGlowMote.NullOrEmpty() ? Props.drawGlowMote : "Mote_FireGlow";
                Mesh mesh2 = MeshPool.GridPlane(DefDatabase<ThingDef>.GetNamed(motename).graphicData.drawSize * (Drawsize * Props.drawGlowSizeFactor));
                Graphics.DrawMesh(mesh2, this.DrawPos, this.ExactRotation, DefDatabase<ThingDef>.GetNamed(motename).graphic.MatSingle, 0);
            }
            base.DrawAt(drawLoc, flip);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }
        private int TicksforAppearence = 15;
        private IntVec3 pos = IntVec3.Invalid;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref TicksforAppearence, "TicksforAppearence");
        }
    }
}
