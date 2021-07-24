using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AbilitesExtended
{
    // AbilitesExtended.Verb_EquipmentLaunchProjectile
    public class Verb_EquipmentLaunchProjectile : Verb_UseEquipment
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return base.ValidateTarget(target, showMessages);
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
        public override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            ThingDef projDef = Projectile;
            if (projDef == null)
            {
                return false;
            }
            ShootLine line;
            bool gotLine = TryFindShootLineFromTo(caster.Position, currentTarget, out line);
            if (verbProps.stopBurstWithoutLos && !gotLine)
            {
                return false;
            }
            if (base.EquipmentSource != null)
            {
                base.EquipmentSource.TryGetCompFast<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                base.EquipmentSource.TryGetCompFast<CompReloadable>()?.UsedOnce();
            }
            Thing attackOwner = caster;
            Thing attackWeapon = base.EquipmentSource;
            CompMannable mannable = caster.TryGetCompFast<CompMannable>();
            if (mannable != null && mannable.ManningPawn != null)
            {
                attackOwner = mannable.ManningPawn;
                attackWeapon = caster;
            }
            Vector3 shotOrigin = caster.DrawPos;
            Projectile proj = (Projectile)GenSpawn.Spawn(projDef, line.Source, caster.Map);
            if (verbProps.ForcedMissRadius > 0.5f)
            {
                float adjustedMissRadius = VerbUtility.CalculateAdjustedForcedMiss(verbProps.ForcedMissRadius, currentTarget.Cell - caster.Position);
                if (adjustedMissRadius > 0.5f)
                {
                    int maxCells = GenRadial.NumCellsInRadius(adjustedMissRadius);
                    int cellInd = Rand.Range(0, maxCells);
                    if (cellInd > 0)
                    {
                        IntVec3 dest = currentTarget.Cell + GenRadial.RadialPattern[cellInd];
                        ThrowDebugText("ToRadius");
                        ThrowDebugText("Rad\nDest", dest);
                        ProjectileHitFlags hitFlags3 = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(0.5f))
                        {
                            hitFlags3 = ProjectileHitFlags.All;
                        }
                        if (!canHitNonTargetPawnsNow)
                        {
                            hitFlags3 &= ~ProjectileHitFlags.NonTargetPawns;
                        }
                        proj.Launch(attackOwner, shotOrigin, dest, currentTarget, hitFlags3, preventFriendlyFire, attackWeapon);
                        return true;
                    }
                }
            }
            ShotReport hitReport = ShotReport.HitReportFor(caster, this, currentTarget);
            Thing hitCover = hitReport.GetRandomCoverToMissInto();
            ThingDef hitCoverDef = hitCover?.def;
            if (!Rand.Chance(hitReport.AimOnTargetChance_IgnoringPosture))
            {
                line.ChangeDestToMissWild(hitReport.AimOnTargetChance_StandardTarget);
                ThrowDebugText("ToWild" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
                ThrowDebugText("Wild\nDest", line.Dest);
                ProjectileHitFlags hitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f) && canHitNonTargetPawnsNow)
                {
                    hitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                proj.Launch(attackOwner, shotOrigin, line.Dest, currentTarget, hitFlags2, preventFriendlyFire, attackWeapon, hitCoverDef);
                return true;
            }
            if (currentTarget.Thing != null && currentTarget.Thing.def.category == ThingCategory.Pawn && !Rand.Chance(hitReport.PassCoverChance))
            {
                ThrowDebugText("ToCover" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
                ThrowDebugText("Cover\nDest", hitCover.Position);
                ProjectileHitFlags hitFlags = ProjectileHitFlags.NonTargetWorld;
                if (canHitNonTargetPawnsNow)
                {
                    hitFlags |= ProjectileHitFlags.NonTargetPawns;
                }
                proj.Launch(attackOwner, shotOrigin, hitCover, currentTarget, hitFlags, preventFriendlyFire, attackWeapon, hitCoverDef);
                return true;
            }
            ProjectileHitFlags directHitFlags = ProjectileHitFlags.IntendedTarget;
            if (canHitNonTargetPawnsNow)
            {
                directHitFlags |= ProjectileHitFlags.NonTargetPawns;
            }
            if (!currentTarget.HasThing || currentTarget.Thing.def.Fillage == FillCategory.Full)
            {
                directHitFlags |= ProjectileHitFlags.NonTargetWorld;
            }
            ThrowDebugText("ToHit" + (canHitNonTargetPawnsNow ? "\nchntp" : ""));
            if (currentTarget.Thing != null)
            {
                proj.Launch(attackOwner, shotOrigin, currentTarget, currentTarget, directHitFlags, preventFriendlyFire, attackWeapon, hitCoverDef);
                ThrowDebugText("Hit\nDest", currentTarget.Cell);
            }
            else
            {
                proj.Launch(attackOwner, shotOrigin, line.Dest, currentTarget, directHitFlags, preventFriendlyFire, attackWeapon, hitCoverDef);
                ThrowDebugText("Hit\nDest", line.Dest);
            }
            return true;
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
                float num = this.HighlightFieldRadiusAroundTarget(out bool flag);
                if (num > 0.2f && this.TryFindShootLineFromTo(this.caster.Position, target, out ShootLine shootLine))
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
