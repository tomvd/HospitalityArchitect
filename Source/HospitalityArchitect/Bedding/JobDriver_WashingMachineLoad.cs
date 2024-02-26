using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

public class JobDriver_WashingMachineLoad : JobDriver
{
    public Building_WashingMachine Core => (Building_WashingMachine)job.GetTarget(TargetIndex.A).Thing;

    public Thing Fuel => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        AddFailCondition(() => !Core.HasSpace);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        Toil reserveFuel = Toils_Reserve.Reserve(TargetIndex.B);
        yield return reserveFuel;
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveFuel, TargetIndex.B, TargetIndex.None);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(10).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate
            {
                Core.LoadWashing(Fuel);
                pawn.carryTracker.innerContainer.Remove(Fuel);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}