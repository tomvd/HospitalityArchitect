namespace HospitalityArchitect;

using System.Collections.Generic;
using System.Linq;
using DubsBadHygiene;
using RimWorld;
using Verse;
using Verse.AI;

internal class JobDriver_MakeBed : JobDriver
{
	private const TargetIndex BedInd = TargetIndex.A;

	private const TargetIndex BeddingInd = TargetIndex.B;

	public const int RefuelingDuration = 240;

	protected Thing Bed => job.GetTarget(TargetIndex.A).Thing;

	protected CompHotelGuestBed BedComp => Bed.TryGetComp<CompHotelGuestBed>();

	protected Thing Bedding => job.GetTarget(TargetIndex.B).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(Bed, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(Bedding, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		AddEndCondition(() => BedComp.needBedding ? JobCondition.Ongoing : JobCondition.Succeeded);
		AddFinishAction((x) => Bedding.Destroy());
		yield return Toils_General.DoAtomic(delegate
		{
			job.count = 1;
		});
		Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
		yield return reserveFuel;
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
		yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None, takeFromValidStorage: true);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.Wait(RefuelingDuration).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A)
			.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
			.WithProgressBarToilDelay(TargetIndex.A);
		yield return Toils_General.DoAtomic(delegate
		{
			BedComp.needBedding = false;
			
		});
		
		//yield return FinalizeMakingBed(TargetIndex.A, TargetIndex.B);
	}
	
	public static Toil FinalizeMakingBed(TargetIndex refuelableInd, TargetIndex fuelInd)
	{
		Toil toil = ToilMaker.MakeToil("FinalizeMakingBed");
		toil.initAction = delegate
		{
			Job curJob = toil.actor.CurJob;
			Thing thing = curJob.GetTarget(refuelableInd).Thing;
			if (toil.actor.CurJob.placedThings.NullOrEmpty())
			{
				thing.TryGetComp<CompRefuelable>().Refuel(new List<Thing> { curJob.GetTarget(fuelInd).Thing });
			}
			else
			{
				thing.TryGetComp<CompRefuelable>().Refuel(toil.actor.CurJob.placedThings.Select((ThingCountClass p) => p.thing).ToList());
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}
}
