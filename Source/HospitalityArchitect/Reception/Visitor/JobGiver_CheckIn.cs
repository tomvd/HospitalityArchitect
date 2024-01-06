using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect.Reception
{
    public class JobGiver_CheckIn : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn guest)
        {
            var guestComp = guest.GetComp<CompGuest>();
            if (guestComp == null) return null;
            if (guestComp.HasBed) return null;
            var hotelGuestComp = guest.GetComp<CompHotelGuest>();
            if (hotelGuestComp == null) return null;
            if (hotelGuestComp.dayVisit) return null;
            

            // Wait longer if we have more money, but do it as soon as otherwise possible
            if (GenTicks.TicksGame < guestComp.lastBedCheckTick  + guest.GetMoney()*4) return null;
            
            guestComp.lastBedCheckTick = GenTicks.TicksGame + 500; // Recheck ever x ticks

            //var bed = guest.FindBedFor();
            //if (bed == null) return null;
            ReceptionController reception = guest.GetAllReceptions().RandomElement(); // TODO closestby
            if (reception == null)
            {
                var thoughtDef = ThoughtDef.Named("HospitalityArchitect_NoReception");
                var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef,0);
                guest.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory);
                var bed = guest.FindBedFor();
                if (bed == null)
                {
                    Log.Message(guest.LabelShort+ " found no bed and is leaving.");
                    guest.Leave();
                    return null;
                }
                return new Job(HADefOf.ClaimBed, bed) {takeExtraIngestibles = bed.GetRentalFee()}; // Store RentalFee to avoid cheating
            }

            return new Job(HADefOf.Reception_CheckIn, reception.Register);
        }
    }
}
