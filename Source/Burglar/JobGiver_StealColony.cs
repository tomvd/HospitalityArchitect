using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RT
{
	public class JobGiver_StealColony : ThinkNode_JobGiver
	{
		public static bool TryFindBestItemToSteal(IntVec3 root, Map map, float maxDist, out Thing item, Pawn thief, List<Thing> disallowed = null, Danger danger = Danger.Some)
		{
			if (map == null)
			{
				item = null;
				return false;
			}
			if (thief != null && !thief.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				item = null;
				return false;
			}
			if ((thief != null && !map.reachability.CanReachMapEdge(thief.Position, TraverseParms.For(thief, Danger.Some))) 
				|| (thief == null && !map.reachability.CanReachMapEdge(root, TraverseParms.For(TraverseMode.PassDoors, Danger.Some))))
			{
				item = null;
				return false;
			}
			Predicate<Thing> validator = delegate (Thing t)
			{
				if (t.def.defName.ToLower().Contains("chunk"))
                {
				 	return false;
                }
				if (t.def.IsCorpse)
                {
					return false;
                }
				if (thief != null && !thief.CanReserve(t))
				{
					return false;
				}
				if (disallowed != null && disallowed.Contains(t))
				{
					return false;
				}
				if (!t.def.stealable)
				{
					return false;
				}
				if (t.Faction == thief.Faction)
				{
					return false;
				}
				if (t.def.IsWeapon)
				{
					return false;
				}
				if (GetValue(t) < 10f)
                {
					return false;
                }
				return (!t.IsBurning()) ? true : false;
			};

			item = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(root, map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable),
					PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.PassDoors, danger), maxDist, validator, (Thing x) => GetValue(x), 15, 15);
			return item != null;
		}


		public static float GetValue(Thing thing)
		{
			return thing.MarketValue * (float)thing.stackCount;
		}
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.HostileTo(Faction.OfPlayer))
			{
				return null;
			}

			if (RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
			{
				if (TryFindBestItemToSteal(pawn.Position, pawn.Map, 50f, out Thing item, pawn))// && !GenAI.InDangerousCombat(pawn))
				{
					Job job = JobMaker.MakeJob(JobDefOf.Steal);
					job.targetA = item;
					job.targetB = spot;
					job.canBashDoors = true;
					job.canBashFences = true;
					job.count = Mathf.Min(item.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / item.def.VolumePerUnit));
					return job;
				}
			}
			bool flag = pawn.natives.IgniteVerb != null && pawn.natives.IgniteVerb.IsStillUsableBy(pawn) && pawn.HostileTo(Faction.OfPlayer);
			CellRect cellRect = CellRect.CenteredOn(pawn.Position, 5);
			for (int i = 0; i < 35; i++)
			{
				IntVec3 randomCell = cellRect.RandomCell;
				if (!randomCell.InBounds(pawn.Map))
				{
					continue;
				}
				Building edifice = randomCell.GetEdifice(pawn.Map);
				if (edifice != null && TrashUtility.ShouldTrashBuilding(pawn, edifice) && GenSight.LineOfSight(pawn.Position, randomCell, pawn.Map))
				{
					Job job = TrashJob(pawn, edifice);
					if (job != null)
					{
						return job;
					}
				}
				if (flag)
				{
					Plant plant = randomCell.GetPlant(pawn.Map);
					if (plant != null && TrashUtility.ShouldTrashPlant(pawn, plant) && GenSight.LineOfSight(pawn.Position, randomCell, pawn.Map))
					{
						Job job2 = TrashJob(pawn, plant);
						if (job2 != null)
						{
							return job2;
						}
					}
				}
			}

			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			if (allBuildingsColonist.Count == 0)
			{
				return null;
			}
			foreach (var building in allBuildingsColonist.OrderBy(x => IntVec3Utility.DistanceTo(x.Position, pawn.Position)).Take(10).InRandomOrder())
			{
				if (TrashUtility.ShouldTrashBuilding(pawn, building, true) && pawn.CanReach(building, PathEndMode.Touch, Danger.None))
				{
					Job job = TrashJob(pawn, building, true);
					if (job != null)
					{
						return job;
					}
				}
			}
			if (RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot2))
			{
				if (TryFindBestItemToSteal(pawn.Position, pawn.Map, 100f, out Thing item, pawn, danger: Danger.None))// && !GenAI.InDangerousCombat(pawn))
				{
					Job job = JobMaker.MakeJob(JobDefOf.Steal);
					job.targetA = item;
					job.targetB = spot2;
					job.canBashDoors = true;
					job.canBashFences = true;
					job.count = Mathf.Min(item.stackCount, (int)(pawn.GetStatValue(StatDefOf.CarryingCapacity) / item.def.VolumePerUnit));
					return job;
				}
			}
			return null;
		}

		public static Job TrashJob(Pawn pawn, Thing t, bool allowPunchingInert = false, bool killIncappedTarget = false)
		{
			if (t is Plant)
			{
				Job job = JobMaker.MakeJob(JobDefOf.Ignite, t);
				FinalizeTrashJob(job);
				return job;
			}
			Job job3 = JobMaker.MakeJob(JobDefOf.AttackMelee, t);
			job3.killIncappedTarget = killIncappedTarget;
			FinalizeTrashJob(job3);
			return job3;
		}
		private static void FinalizeTrashJob(Job job)
		{
			job.expiryInterval = TrashJobCheckOverrideInterval.RandomInRange;
			job.checkOverrideOnExpire = true;
			job.expireRequiresEnemiesNearby = true;
		}

		private static readonly IntRange TrashJobCheckOverrideInterval = new IntRange(450, 500);

	}
}
