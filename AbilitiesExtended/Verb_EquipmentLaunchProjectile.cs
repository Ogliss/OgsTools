using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AbilitesExtended
{

    // Token: 0x02000025 RID: 37
    public class Verb_EquipmentLaunchProjectile : Verb_UseEquipment
    {
        // Token: 0x06002188 RID: 8584 RVA: 0x0000FFF1 File Offset: 0x0000E1F1
        public override bool ValidateTarget(LocalTargetInfo target)
        {
            return true;
        }

        public virtual ThingDef Projectile
        {
            get
            {
                if (base.EquipmentSource != null)
                {
                    CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                    if (comp != null && comp.Loaded)
                    {
                        return comp.Projectile;
                    }
                }
                return this.verbProps.defaultProjectile;
            }
        }
        protected override bool TryCastShot()
        {
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
            {
                return false;
            }
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return false;
            }
            ShootLine shootLine;
            bool flag = base.TryFindShootLineFromTo(this.caster.Position, this.currentTarget, out shootLine);
            if (this.verbProps.stopBurstWithoutLos && !flag)
            {
                return false;
            }
            if (base.EquipmentSource != null)
            {
                CompChangeableProjectile comp = base.EquipmentSource.GetComp<CompChangeableProjectile>();
                if (comp != null)
                {
                    comp.Notify_ProjectileLaunched();
                }
            }
            Thing launcher = this.caster;
            Thing equipment = base.EquipmentSource;
            CompMannable compMannable = this.caster.TryGetComp<CompMannable>();
            if (compMannable != null && compMannable.ManningPawn != null)
            {
                launcher = compMannable.ManningPawn;
                equipment = this.caster;
            }
            if (equipment == null)
            {
                Log.Message("equipment==null");
            }
            else
            {
                Log.Message(equipment.LabelShortCap);
            }
            Vector3 drawPos = this.caster.DrawPos;
            Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, shootLine.Source, this.caster.Map, WipeMode.Vanish);
            if (this.verbProps.forcedMissRadius > 0.5f)
            {
                float num = VerbUtility.CalculateAdjustedForcedMiss(this.verbProps.forcedMissRadius, this.currentTarget.Cell - this.caster.Position);
                if (num > 0.5f)
                {
                    int max = GenRadial.NumCellsInRadius(num);
                    int num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        IntVec3 c = this.currentTarget.Cell + GenRadial.RadialPattern[num2];
                        this.ThrowDebugText("ToRadius");
                        this.ThrowDebugText("Rad\nDest", c);
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }
                        if (!this.canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                        }
                        projectile2.Launch(launcher, drawPos, c, this.currentTarget, projectileHitFlags, equipment, null);
                        return true;
                    }
                }
            }
            ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, this.currentTarget);
            Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            ThingDef targetCoverDef = (randomCoverToMissInto != null) ? randomCoverToMissInto.def : null;
            if (!Rand.Chance(shotReport.AimOnTargetChance_IgnoringPosture))
            {
                shootLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                this.ThrowDebugText("ToWild" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
                this.ThrowDebugText("Wild\nDest", shootLine.Dest);
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f) && this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags2, equipment, targetCoverDef);
                return true;
            }
            if (this.currentTarget.Thing != null && this.currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(shotReport.PassCoverChance))
            {
                this.ThrowDebugText("ToCover" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
                this.ThrowDebugText("Cover\nDest", randomCoverToMissInto.Position);
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                if (this.canHitNonTargetPawnsNow)
                {
                    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile2.Launch(launcher, drawPos, randomCoverToMissInto, this.currentTarget, projectileHitFlags3, equipment, targetCoverDef);
                return true;
            }
            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (this.canHitNonTargetPawnsNow)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!this.currentTarget.HasThing || this.currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }
            this.ThrowDebugText("ToHit" + (this.canHitNonTargetPawnsNow ? "\nchntp" : ""));
            if (this.currentTarget.Thing != null)
            {
                projectile2.Launch(launcher, drawPos, this.currentTarget, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
                this.ThrowDebugText("Hit\nDest", this.currentTarget.Cell);
            }
            else
            {
                projectile2.Launch(launcher, drawPos, shootLine.Dest, this.currentTarget, projectileHitFlags4, equipment, targetCoverDef);
                this.ThrowDebugText("Hit\nDest", shootLine.Dest);
            }
            return true;
            //    return this.ability.Activate(this.currentTarget, this.currentDestination);
        }
        // Token: 0x06002196 RID: 8598 RVA: 0x000CBF53 File Offset: 0x000CA153
        private void ThrowDebugText(string text)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, text, -1f);
            }
        }

        // Token: 0x06002197 RID: 8599 RVA: 0x000CBF7D File Offset: 0x000CA17D
        private void ThrowDebugText(string text, IntVec3 c)
        {
            if (DebugViewSettings.drawShooting)
            {
                MoteMaker.ThrowText(c.ToVector3Shifted(), this.caster.Map, text, -1f);
            }
        }

        // Token: 0x06002189 RID: 8585 RVA: 0x000CB488 File Offset: 0x000C9688
        public override void DrawHighlight(LocalTargetInfo target)
        {
            AbilityDef def = this.ability.def;
            this.DrawRadius();
            if (this.CanHitTarget(target) && this.IsApplicableTo(target, false))
            {
                if (def.HasAreaOfEffect)
                {
                    if (target.IsValid)
                    {
                        GenDraw.DrawTargetHighlight(target);
                        GenDraw.DrawRadiusRing(target.Cell, def.EffectRadius, Verb_CastAbility.RadiusHighlightColor, null);
                    }
                }
                else
                {
                    GenDraw.DrawTargetHighlight(target);
                }
            }
            if (target.IsValid)
            {
                this.ability.DrawEffectPreviews(target);
            }
            this.verbProps.DrawRadiusRing(this.caster.Position);
            if (target.IsValid)
            {
                GenDraw.DrawTargetHighlight(target);
                bool flag;
                float num = this.HighlightFieldRadiusAroundTarget(out flag);
                ShootLine shootLine;
                if (num > 0.2f && this.TryFindShootLineFromTo(this.caster.Position, target, out shootLine))
                {
                    if (flag)
                    {
                        GenExplosion.RenderPredictedAreaOfEffect(shootLine.Dest, num);
                        return;
                    }
                    GenDraw.DrawFieldEdges((from x in GenRadial.RadialCellsAround(shootLine.Dest, num, true)
                                            where x.InBounds(Find.CurrentMap)
                                            select x).ToList<IntVec3>());
                }
            }
        }
        // Token: 0x06002198 RID: 8600 RVA: 0x000CBFA4 File Offset: 0x000CA1A4
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;
            ThingDef projectile = this.Projectile;
            if (projectile == null)
            {
                return 0f;
            }
            return projectile.projectile.explosionRadius;
        }

        // Token: 0x06002199 RID: 8601 RVA: 0x000CBFD0 File Offset: 0x000CA1D0
        public override bool Available()
        {
            if (!base.Available())
            {
                return false;
            }
            if (this.CasterIsPawn)
            {
                Pawn casterPawn = this.CasterPawn;
                if (casterPawn.Faction != Faction.OfPlayer && casterPawn.mindState.MeleeThreatStillThreat && casterPawn.mindState.meleeThreat.Position.AdjacentTo8WayOrInside(casterPawn.Position))
                {
                    return false;
                }
            }
            return this.Projectile != null;
        }
    }
}
