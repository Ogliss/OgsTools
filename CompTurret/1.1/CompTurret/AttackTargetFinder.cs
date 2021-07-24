// Verse.AI.AttackTargetFinder
using CompTurret.ExtensionMethods;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace CompTurret
{

    public static class AttackTargetFinder
	{

		private static List<IAttackTarget> tmpTargets;

		private static List<Pair<IAttackTarget, float>> availableShootingTargets;

		private static List<float> tmpTargetScores;

		private static List<bool> tmpCanShootAtTarget;

		private static List<IntVec3> tempDestList;

		private static List<IntVec3> tempSourceList;

		public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDist = 0f, float maxDist = 9999f, IntVec3 locus = default(IntVec3), float maxTravelRadiusFromLocus = float.MaxValue, bool canBash = false, bool canTakeTargetsCloserThanEffectiveMinRange = true)
		{
			Thing searcherThing = searcher.Thing;
			Pawn searcherPawn = searcher as Pawn;
			Verb verb = searcher.CurrentEffectiveVerb;
			if (verb == null)
			{
				Log.Error("BestAttackTarget with " + searcher.ToStringSafe() + " who has no attack verb.");
				return null;
			}
			bool onlyTargetMachines = verb.IsEMP();
			float minDistSquared = minDist * minDist;
			float num = maxTravelRadiusFromLocus + verb.verbProps.range;
			float maxLocusDistSquared = num * num;
			Func<IntVec3, bool> losValidator = null;
			if ((flags & TargetScanFlags.LOSBlockableByGas) != 0)
			{
				losValidator = delegate (IntVec3 vec3)
				{
					Gas gas = vec3.GetGas(searcherThing.Map);
					return gas == null || !gas.def.gas.blockTurretTracking;
				};
			}
			Predicate<IAttackTarget> innerValidator = delegate (IAttackTarget t)
			{
				Thing thing = t.Thing;
				if (t == searcher)
				{
					return false;
				}
				if (minDistSquared > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < minDistSquared)
				{
					return false;
				}
				if (!canTakeTargetsCloserThanEffectiveMinRange)
				{
					float num2 = verb.verbProps.EffectiveMinRange(thing, searcherThing);
					if (num2 > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < num2 * num2)
					{
						return false;
					}
				}
				if (maxTravelRadiusFromLocus < 9999f && (float)(thing.Position - locus).LengthHorizontalSquared > maxLocusDistSquared)
				{
					return false;
				}
				if (!searcherThing.HostileTo(thing))
				{
					return false;
				}
				if (validator != null && !validator(thing))
				{
					return false;
				}
				if (searcherPawn != null)
				{
					Lord lord = searcherPawn.GetLord();
					if (lord != null && !lord.LordJob.ValidateAttackTarget(searcherPawn, thing))
					{
					//	Log.Messageage(thing + " lord ValidateAttackTarget: false");
						return false;
					}
				}
				if ((flags & TargetScanFlags.NeedNotUnderThickRoof) != 0)
				{
					RoofDef roof = thing.Position.GetRoof(thing.Map);
					if (roof != null && roof.isThickRoof)
					{
					//	Log.Messageage(thing + " isThickRoof: false");
						return false;
					}
				}
				if ((flags & TargetScanFlags.NeedLOSToAll) != 0)
				{
					if (losValidator != null && (!losValidator(searcherThing.Position) || !losValidator(thing.Position)))
					{
					//	Log.Messageage(thing + " LOSToAll: false");
						return false;
					}
					if (!searcherThing.CanSee(thing, losValidator))
					{
						if (t is Pawn)
						{
							if ((flags & TargetScanFlags.NeedLOSToPawns) != 0)
							{
							//	Log.Messageage(thing + " LOSToPawns: false");
								return false;
							}
						}
						else if ((flags & TargetScanFlags.NeedLOSToNonPawns) != 0)
						{
						//	Log.Messageage(thing + " LOSToNonPawns: false");
							return false;
						}
					}
				}
				if (((flags & TargetScanFlags.NeedThreat) != 0 || (flags & TargetScanFlags.NeedAutoTargetable) != 0) && t.ThreatDisabled(searcher))
				{
				//	Log.Messageage(thing + " NeedThreat: false");
					return false;
				}
				if ((flags & TargetScanFlags.NeedAutoTargetable) != 0 && !IsAutoTargetable(t))
				{
				//	Log.Messageage(thing + " NeedAutoTargetable: false");
					return false;
				}
				if ((flags & TargetScanFlags.NeedActiveThreat) != 0 && !GenHostility.IsActiveThreatTo(t, searcher.Thing.Faction))
				{
				//	Log.Messageage(thing + " NeedActiveThreat: false");
					return false;
				}
				Pawn pawn = t as Pawn;
				if (onlyTargetMachines && pawn != null && pawn.RaceProps.IsFlesh)
				{
				//	Log.Messageage(thing + " onlyTargetMachines: false");
					return false;
				}
				if ((flags & TargetScanFlags.NeedNonBurning) != 0 && thing.IsBurning())
				{
				//	Log.Messageage(thing + " NeedNonBurning: false");
					return false;
				}
				if (searcherThing.def.race != null && (int)searcherThing.def.race.intelligence >= 2)
				{
					CompExplosive compExplosive = thing.TryGetCompFast<CompExplosive>();
					if (compExplosive != null && compExplosive.wickStarted)
					{
					//	Log.Messageage(thing + " wickStarted: false");
						return false;
					}
				}
				if (thing.def.size.x == 1 && thing.def.size.z == 1)
				{
					if (thing.Position.Fogged(thing.Map))
					{
					//	Log.Messageage(thing + " Fogged: false");
						return false;
					}
				}
				else
				{
					bool flag2 = false;
					foreach (IntVec3 item in thing.OccupiedRect())
					{
						if (!item.Fogged(thing.Map))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
					//	Log.Messageage(thing + " Fogged: false");
						return false;
					}
				}
			//	Log.Messageage(thing + " valid: true");
				return true;
			};
			if (HasRangedAttack(searcher) && (searcherPawn == null || !searcherPawn.InAggroMentalState))
			{
			//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack  0");
				tmpTargets.Clear();
			//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack  1");
				tmpTargets.AddRange(searcherThing.Map.attackTargetsCache.GetPotentialTargetsFor(searcher));
			//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack  2 tmpTargets: " + tmpTargets.Count);
				if ((flags & TargetScanFlags.NeedReachable) != 0)
				{
					Predicate<IAttackTarget> oldValidator2 = innerValidator;
					innerValidator = ((IAttackTarget t) => oldValidator2(t) && CanReach(searcherThing, t.Thing, canBash));
				}
			//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack  3");
				bool flag = false;
				for (int i = 0; i < tmpTargets.Count; i++)
				{
					IAttackTarget attackTarget = tmpTargets[i];
					if (attackTarget.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) && innerValidator(attackTarget) && CanShootAtFromCurrentPosition(attackTarget, searcher, verb))
					{
						flag = true;
						break;
					}
				}
			//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack 4 ");
				IAttackTarget attackTarget2;
				if (flag)
				{
					tmpTargets.RemoveAll((IAttackTarget x) => !x.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) || !innerValidator(x));
					attackTarget2 = GetRandomShootingTargetByScore(tmpTargets, searcher, verb);
				}
				else
				{
					attackTarget2 = (IAttackTarget)GenClosest.ClosestThing_Global(validator: ((flags & TargetScanFlags.NeedReachableIfCantHitFromMyPos) == 0 || (flags & TargetScanFlags.NeedReachable) != 0) ? ((Predicate<Thing>)((Thing t) => innerValidator((IAttackTarget)t))) : ((Predicate<Thing>)((Thing t) => innerValidator((IAttackTarget)t) && (CanReach(searcherThing, t, canBash) || CanShootAtFromCurrentPosition((IAttackTarget)t, searcher, verb)))), center: searcherThing.Position, searchSet: tmpTargets, maxDistance: maxDist);
				}
			//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack  5");
				tmpTargets.Clear();
			//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack tgt found: " + (attackTarget2 != null).ToString());
				return attackTarget2;
			}
		//	Log.Messageage(searcher + "AttackTargetFinder.HasRangedAttack  no ranged attack found");
			if (searcherPawn != null && searcherPawn.mindState.duty != null && searcherPawn.mindState.duty.radius > 0f && !searcherPawn.InMentalState)
			{
				Predicate<IAttackTarget> oldValidator = innerValidator;
				innerValidator = ((IAttackTarget t) => oldValidator(t) && t.Thing.Position.InHorDistOf(searcherPawn.mindState.duty.focus.Cell, searcherPawn.mindState.duty.radius));
			}
			IAttackTarget attackTarget3 = (IAttackTarget)GenClosest.ClosestThingReachable(searcherThing.Position, searcherThing.Map, ThingRequest.ForGroup(ThingRequestGroup.AttackTarget), PathEndMode.Touch, TraverseParms.For(searcherPawn, Danger.Deadly, TraverseMode.ByPawn, canBash), maxDist, (Thing x) => innerValidator((IAttackTarget)x), null, 0, (maxDist > 800f) ? (-1) : 40);
			if (attackTarget3 != null && PawnUtility.ShouldCollideWithPawns(searcherPawn))
			{
				IAttackTarget attackTarget4 = FindBestReachableMeleeTarget(innerValidator, searcherPawn, maxDist, canBash);
				if (attackTarget4 != null)
				{
					float lengthHorizontal = (searcherPawn.Position - attackTarget3.Thing.Position).LengthHorizontal;
					float lengthHorizontal2 = (searcherPawn.Position - attackTarget4.Thing.Position).LengthHorizontal;
					if (Mathf.Abs(lengthHorizontal - lengthHorizontal2) < 50f)
					{
						attackTarget3 = attackTarget4;
					}
				}
			}
			return attackTarget3;
		}

		private static bool CanReach(Thing searcher, Thing target, bool canBash)
		{
			Pawn pawn = searcher as Pawn;
			if (pawn != null)
			{
				if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Some, canBash))
				{
					return false;
				}
			}
			else
			{
				TraverseMode mode = canBash ? TraverseMode.PassDoors : TraverseMode.NoPassClosedDoors;
				if (!searcher.Map.reachability.CanReach(searcher.Position, target, PathEndMode.Touch, TraverseParms.For(mode)))
				{
					return false;
				}
			}
			return true;
		}

		private static IAttackTarget FindBestReachableMeleeTarget(Predicate<IAttackTarget> validator, Pawn searcherPawn, float maxTargDist, bool canBash)
		{
			maxTargDist = Mathf.Min(maxTargDist, 30f);
			IAttackTarget reachableTarget = null;
			Func<IntVec3, IAttackTarget> bestTargetOnCell = delegate (IntVec3 x)
			{
				List<Thing> thingList = x.GetThingList(searcherPawn.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					IAttackTarget attackTarget2 = thing as IAttackTarget;
					if (attackTarget2 != null && validator(attackTarget2) && ReachabilityImmediate.CanReachImmediate(x, thing, searcherPawn.Map, PathEndMode.Touch, searcherPawn) && (searcherPawn.CanReachImmediate(thing, PathEndMode.Touch) || searcherPawn.Map.attackTargetReservationManager.CanReserve(searcherPawn, attackTarget2)))
					{
						return attackTarget2;
					}
				}
				return null;
			};
			searcherPawn.Map.floodFiller.FloodFill(searcherPawn.Position, delegate (IntVec3 x)
			{
				if (!x.Walkable(searcherPawn.Map))
				{
					return false;
				}
				if ((float)x.DistanceToSquared(searcherPawn.Position) > maxTargDist * maxTargDist)
				{
					return false;
				}
				if (!canBash)
				{
					Building_Door building_Door = x.GetEdifice(searcherPawn.Map) as Building_Door;
					if (building_Door != null && !building_Door.CanPhysicallyPass(searcherPawn))
					{
						return false;
					}
				}
				return (!PawnUtility.AnyPawnBlockingPathAt(x, searcherPawn, actAsIfHadCollideWithPawnsJob: true)) ? true : false;
			}, delegate (IntVec3 x)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 intVec = x + GenAdj.AdjacentCells[i];
					if (intVec.InBounds(searcherPawn.Map))
					{
						IAttackTarget attackTarget = bestTargetOnCell(intVec);
						if (attackTarget != null)
						{
							reachableTarget = attackTarget;
							break;
						}
					}
				}
				return reachableTarget != null;
			});
			return reachableTarget;
		}

		private static bool HasRangedAttack(IAttackTargetSearcher t)
		{
			Verb currentEffectiveVerb = t.CurrentEffectiveVerb;
			if (currentEffectiveVerb != null)
			{
				return !currentEffectiveVerb.verbProps.IsMeleeAttack;
			}
			return false;
		}

		private static bool CanShootAtFromCurrentPosition(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			return verb?.CanHitTargetFrom(searcher.Thing.Position, target.Thing) ?? false;
		}

		private static IAttackTarget GetRandomShootingTargetByScore(List<IAttackTarget> targets, IAttackTargetSearcher searcher, Verb verb)
		{
			if (GetAvailableShootingTargetsByScore(targets, searcher, verb).TryRandomElementByWeight((Pair<IAttackTarget, float> x) => x.Second, out Pair<IAttackTarget, float> result))
			{
				return result.First;
			}
			return null;
		}

		private static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(List<IAttackTarget> rawTargets, IAttackTargetSearcher searcher, Verb verb)
		{
			availableShootingTargets.Clear();
			if (rawTargets.Count == 0)
			{
				return availableShootingTargets;
			}
			tmpTargetScores.Clear();
			tmpCanShootAtTarget.Clear();
			float num = 0f;
			IAttackTarget attackTarget = null;
			for (int i = 0; i < rawTargets.Count; i++)
			{
				tmpTargetScores.Add(float.MinValue);
				tmpCanShootAtTarget.Add(item: false);
				if (rawTargets[i] == searcher)
				{
					continue;
				}
				bool flag = CanShootAtFromCurrentPosition(rawTargets[i], searcher, verb);
				tmpCanShootAtTarget[i] = flag;
				if (flag)
				{
					float shootingTargetScore = GetShootingTargetScore(rawTargets[i], searcher, verb);
					tmpTargetScores[i] = shootingTargetScore;
					if (attackTarget == null || shootingTargetScore > num)
					{
						attackTarget = rawTargets[i];
						num = shootingTargetScore;
					}
				}
			}
			if (num < 1f)
			{
				if (attackTarget != null)
				{
					availableShootingTargets.Add(new Pair<IAttackTarget, float>(attackTarget, 1f));
				}
			}
			else
			{
				float num2 = num - 30f;
				for (int j = 0; j < rawTargets.Count; j++)
				{
					if (rawTargets[j] != searcher && tmpCanShootAtTarget[j])
					{
						float num3 = tmpTargetScores[j];
						if (num3 >= num2)
						{
							float second = Mathf.InverseLerp(num - 30f, num, num3);
							availableShootingTargets.Add(new Pair<IAttackTarget, float>(rawTargets[j], second));
						}
					}
				}
			}
			return availableShootingTargets;
		}

		private static float GetShootingTargetScore(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			float num = 60f;
			num -= Mathf.Min((target.Thing.Position - searcher.Thing.Position).LengthHorizontal, 40f);
			if (target.TargetCurrentlyAimingAt == searcher.Thing)
			{
				num += 10f;
			}
			if (searcher.LastAttackedTarget == target.Thing && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
			{
				num += 40f;
			}
			num -= CoverUtility.CalculateOverallBlockChance(target.Thing.Position, searcher.Thing.Position, searcher.Thing.Map) * 10f;
			Pawn pawn = target as Pawn;
			if (pawn != null && pawn.RaceProps.Animal && pawn.Faction != null && !pawn.IsFighting())
			{
				num -= 50f;
			}
			num += FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, verb);
			num += FriendlyFireConeTargetScoreOffset(target, searcher, verb);
			return num * target.TargetPriorityFactor;
		}

		private static float FriendlyFireBlastRadiusTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			if (verb.verbProps.ai_AvoidFriendlyFireRadius <= 0f)
			{
				return 0f;
			}
			Map map = target.Thing.Map;
			IntVec3 position = target.Thing.Position;
			int num = GenRadial.NumCellsInRadius(verb.verbProps.ai_AvoidFriendlyFireRadius);
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = position + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map))
				{
					continue;
				}
				bool flag = true;
				List<Thing> thingList = intVec.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (!(thingList[j] is IAttackTarget) || thingList[j] == target)
					{
						continue;
					}
					if (flag)
					{
						if (!GenSight.LineOfSight(position, intVec, map, skipFirstCell: true))
						{
							break;
						}
						flag = false;
					}
					float num3 = (thingList[j] == searcher) ? 40f : ((!(thingList[j] is Pawn)) ? 10f : (thingList[j].def.race.Animal ? 7f : 18f));
					num2 = ((!searcher.Thing.HostileTo(thingList[j])) ? (num2 - num3) : (num2 + num3 * 0.6f));
				}
			}
			return num2;
		}

		private static float FriendlyFireConeTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
		{
			Pawn pawn = searcher.Thing as Pawn;
			if (pawn == null)
			{
				return 0f;
			}
			if ((int)pawn.RaceProps.intelligence < 1)
			{
				return 0f;
			}
			if (pawn.RaceProps.IsMechanoid)
			{
				return 0f;
			}
			Verb_Shoot verb_Shoot = verb as Verb_Shoot;
			if (verb_Shoot == null)
			{
				return 0f;
			}
			ThingDef defaultProjectile = verb_Shoot.verbProps.defaultProjectile;
			if (defaultProjectile == null)
			{
				return 0f;
			}
			if (defaultProjectile.projectile.flyOverhead)
			{
				return 0f;
			}
			Map map = pawn.Map;
			ShotReport report = ShotReport.HitReportFor(pawn, verb, (Thing)target);
			float radius = Mathf.Max(VerbUtility.CalculateAdjustedForcedMiss(verb.verbProps.forcedMissRadius, report.ShootLine.Dest - report.ShootLine.Source), 1.5f);
			IEnumerable<IntVec3> enumerable = (from dest in GenRadial.RadialCellsAround(report.ShootLine.Dest, radius, useCenter: true)
											   where dest.InBounds(map)
											   select new ShootLine(report.ShootLine.Source, dest)).SelectMany((ShootLine line) => line.Points().Concat(line.Dest).TakeWhile((IntVec3 pos) => pos.CanBeSeenOverFast(map))).Distinct();
			float num = 0f;
			foreach (IntVec3 item in enumerable)
			{
				float num2 = VerbUtility.InterceptChanceFactorFromDistance(report.ShootLine.Source.ToVector3Shifted(), item);
				if (!(num2 <= 0f))
				{
					List<Thing> thingList = item.GetThingList(map);
					for (int i = 0; i < thingList.Count; i++)
					{
						Thing thing = thingList[i];
						if (thing is IAttackTarget && thing != target)
						{
							float num3 = (thing == searcher) ? 40f : ((!(thing is Pawn)) ? 10f : (thing.def.race.Animal ? 7f : 18f));
							num3 *= num2;
							num3 = ((!searcher.Thing.HostileTo(thing)) ? (num3 * -1f) : (num3 * 0.6f));
							num += num3;
						}
					}
				}
			}
			return num;
		}

		public static IAttackTarget BestShootTargetFromCurrentPosition(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDistance = 0f, float maxDistance = 9999f)
		{
			Verb currentEffectiveVerb = searcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb == null)
			{
				Log.Error("BestShootTargetFromCurrentPosition with " + searcher.ToStringSafe() + " who has no attack verb.");
				return null;
			}
			return BestAttackTarget(searcher, flags, validator, Mathf.Max(minDistance, currentEffectiveVerb.verbProps.minRange), Mathf.Min(maxDistance, currentEffectiveVerb.verbProps.range), default(IntVec3), float.MaxValue, canBash: false, canTakeTargetsCloserThanEffectiveMinRange: false);
		}

		public static bool CanSee(this Thing seer, Thing target, Func<IntVec3, bool> validator = null)
		{
		//	Log.Messageage(seer+" CanSee "+ target+"?");
			ShootLeanUtility.CalcShootableCellsOf(tempDestList, target);
			for (int i = 0; i < tempDestList.Count; i++)
			{
				if (GenSight.LineOfSight(seer.Position, tempDestList[i], seer.Map, skipFirstCell: true, validator))
				{
					return true;
				}
			}
			ShootLeanUtility.LeanShootingSourcesFromTo(seer.Position, target.Position, seer.Map, tempSourceList);
            if (seer is Building)
			{
				IntVec2 size = new IntVec2(seer.def.Size.x + 2, seer.def.Size.z + 2);
				foreach (IntVec3 intVec2 in GenAdj.OccupiedRect(seer.Position, seer.Rotation, size))
				{
					if (!tempSourceList.Contains(intVec2))
					{
						tempSourceList.Add(intVec2);

					}
				}
			}
			for (int j = 0; j < tempSourceList.Count; j++)
			{
				for (int k = 0; k < tempDestList.Count; k++)
				{
					if (GenSight.LineOfSight(tempSourceList[j], tempDestList[k], seer.Map, skipFirstCell: true, validator))
					{
						return true;
					}
				}
			}
		//	Log.Messageage(seer + " CanSee " + target + ": FALSE");
			return false;
		}

		public static void DebugDrawAttackTargetScores_Update()
		{
			IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
			if (attackTargetSearcher == null || attackTargetSearcher.Thing.Map != Find.CurrentMap)
			{
				return;
			}
			Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb != null)
			{
				tmpTargets.Clear();
				List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				for (int i = 0; i < list.Count; i++)
				{
					tmpTargets.Add((IAttackTarget)list[i]);
				}
				List<Pair<IAttackTarget, float>> availableShootingTargetsByScore = GetAvailableShootingTargetsByScore(tmpTargets, attackTargetSearcher, currentEffectiveVerb);
				for (int j = 0; j < availableShootingTargetsByScore.Count; j++)
				{
					GenDraw.DrawLineBetween(attackTargetSearcher.Thing.DrawPos, availableShootingTargetsByScore[j].First.Thing.DrawPos);
				}
			}
		}

		public static void DebugDrawAttackTargetScores_OnGUI()
		{
			IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
			if (attackTargetSearcher == null || attackTargetSearcher.Thing.Map != Find.CurrentMap)
			{
				return;
			}
			Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
			if (currentEffectiveVerb == null)
			{
				return;
			}
			List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Tiny;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing != attackTargetSearcher)
				{
					string text;
					Color textColor;
					if (!CanShootAtFromCurrentPosition((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb))
					{
						text = "out of range";
						textColor = Color.red;
					}
					else
					{
						text = GetShootingTargetScore((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb).ToString("F0");
						textColor = new Color(0.25f, 1f, 0.25f);
					}
					GenMapUI.DrawThingLabel(thing.DrawPos.MapToUIPosition(), text, textColor);
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}

		public static bool IsAutoTargetable(IAttackTarget target)
		{
			CompCanBeDormant compCanBeDormant = target.Thing.TryGetCompFast<CompCanBeDormant>();
			if (compCanBeDormant != null && !compCanBeDormant.Awake)
			{
				return false;
			}
			CompInitiatable compInitiatable = target.Thing.TryGetCompFast<CompInitiatable>();
			if (compInitiatable != null && !compInitiatable.Initiated)
			{
				return false;
			}
			return true;
		}

		static AttackTargetFinder()
		{
			tmpTargets = new List<IAttackTarget>();
			availableShootingTargets = new List<Pair<IAttackTarget, float>>();
			tmpTargetScores = new List<float>();
			tmpCanShootAtTarget = new List<bool>();
			tempDestList = new List<IntVec3>();
			tempSourceList = new List<IntVec3>();
		}
	}

}