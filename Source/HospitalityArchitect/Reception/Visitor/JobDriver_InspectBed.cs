using System.Collections.Generic;
using System.Linq;
using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

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
        yield return ClaimBed();
    }

    private bool BedHasBeenClaimed(Toil toil)
    {
        return !(TargetA.Thing is Building_GuestBed {AnyUnownedSleepingSlot: true});
    }

    private Toil ClaimBed()
    {
        return new Toil
        {
            initAction = () => {
                var actor = GetActor();
                var silver = actor.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
                var money = silver?.stackCount ?? 0;
                
                // Check the stored RentalFee (takeExtraIngestibles)... if it was increased, cancel!
                if (!(TargetA.Thing is Building_GuestBed newBed)) 
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }

                if (!newBed.AnyUnownedSleepingSlot)
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }

                CompGuest compGuest = actor.GetComp<CompGuest>();
                if (compGuest.HasBed) Log.Error($"{actor.LabelShort} already has a bed ({compGuest.bed.Label})");
                compGuest.ClaimBed(newBed);

                // we paid already
                //if (newBed.RentalFee > 0)
                //{
//                        actor.inventory.innerContainer.TryDrop(silver, actor.Position, Map, ThingPlaceMode.Near, newBed.RentalFee, out silver);
//                    }
                actor.ThoughtAboutClaimedBed(newBed, money + newBed.GetRentalFee());
                actor.ThoughtAboutRoomCleanliness(newBed);
            }
        }.FailOnDespawnedNullOrForbidden(TargetIndex.A);
    }
}