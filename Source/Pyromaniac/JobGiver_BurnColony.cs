using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RT
{
	public class JobGiver_BurnColony : ThinkNode_JobGiver
	{
		public static bool HasTorches(Pawn pawn)
        {
			var molotov = pawn.EquippedWornOrInventoryThings.Where(a => a.def.Equals(ThingDef.Named("Weapon_GrenadeMolotov"))).FirstOrDefault();
			if (molotov != null)
			{
				return true;
			}
			return true;
		}

		public static Verb GetVerbFromTorches(Pawn pawn)
		{
			var molotov = pawn.EquippedWornOrInventoryThings.Where(a => a.def.Equals(ThingDef.Named("Weapon_GrenadeMolotov"))).FirstOrDefault();
			if (molotov != null)
			{
				foreach (var verb in pawn.equipment.AllEquipmentVerbs)
                {
					return verb;
                }
			}
			return null;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
		 //Log.Message(pawn + " - " + pawn.mindState.duty);
			if (!pawn.HostileTo(Faction.OfPlayer))
			{
			 //Log.Message("0 - " + pawn + " - null", true);
				return null;
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
					 //Log.Message("1 - " + pawn + " - " + job, true);
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
						 //Log.Message("2 - " + pawn + " - " + job2, true);

							return job2;
						}
					}
				}
			}

			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			if (allBuildingsColonist.Count == 0)
			{
			 //Log.Message("4 - " + pawn + " - null", true);
				return null;
			}
			foreach (var building in allBuildingsColonist.OrderBy(x => IntVec3Utility.DistanceTo(x.Position, pawn.Position)).Take(10).InRandomOrder())
			{
				if (TrashUtility.ShouldTrashBuilding(pawn, building, true) && pawn.CanReach(building, PathEndMode.Touch, Danger.None))
				{
					Job job = TrashJob(pawn, building, true);
					if (job != null)
					{
					 //Log.Message("5 - " + pawn + " - " + job, true);
						return job;
					}
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
			Job job3 = null;
			if (pawn.natives.IgniteVerb != null && pawn.natives.IgniteVerb.IsStillUsableBy(pawn) && t.FlammableNow && !t.IsBurning() && !(t is Building_Door))
			{
				if (HasTorches(pawn) && Rand.Chance(0.5f))
                {
					job3 = JobMaker.MakeJob(RTDefOf.RT_IgniteWithTorches, t);
					job3.verbToUse = GetVerbFromTorches(pawn); 
				}
				else
                {
					job3 = JobMaker.MakeJob(JobDefOf.Ignite, t);
                }
			}
			else
			{
				job3 = JobMaker.MakeJob(JobDefOf.AttackMelee, t);
			}
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
