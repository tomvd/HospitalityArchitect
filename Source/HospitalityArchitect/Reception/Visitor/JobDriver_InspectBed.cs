using System.Collections.Generic;
using System.Linq;
using Hospitality;
using Hospitality.Utilities;
using HospitalityArchitect.Reception.Visitor;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HospitalityArchitect.Reception;

public class JobDriver_InspectBed : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!(TargetA.Thing is Building_GuestBed newBed)) return false;
        if (pawn.Reserve(TargetA, job, newBed.SleepingSlotsCount, 0, null, errorOnFailed)) return true;

        Log.Message($"{pawn.LabelShort} failed to reserve {TargetA.Thing.LabelShort}!");
        return false;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.EndOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(BedHasBeenClaimed);//.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        yield return ClaimBedToil.ClaimBed(false, GetActor(), TargetA.Thing, job);
    }

    private bool BedHasBeenClaimed(Toil toil)
    {
        return !(TargetA.Thing is Building_GuestBed {AnyUnownedSleepingSlot: true});
    }
}