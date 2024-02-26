using System.Linq;
using Hospitality;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HospitalityArchitect.Reception.Visitor;

public static class ClaimBedToil
{
    public static Toil ClaimBed(bool needToPay, Pawn actor, Thing bed, Job job)
    {
        return new Toil
        {
            initAction = () => {
                var silver = actor.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
                var money = silver?.stackCount ?? 0;
                var sleepingInATent = false;
                var groupSize = actor.GetLord().ownedPawns.Count;

                if (needToPay && actor.GetComp<CompHotelGuest>().totalSpent >= job.takeExtraIngestibles)
                {
                    needToPay = false;
                }
                
                if (!(bed is Building_GuestBed newBed)) 
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }
                if (needToPay && ( 
                    newBed.GetRentalFee() > job.takeExtraIngestibles)) // precents cheating..
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }

                if (newBed.def.defName.Equals("CampingTentSpotGuest"))
                {
                    newBed = TentUtility.SetUpTent(newBed);
                    sleepingInATent = true;
                }
                
                if (!newBed.AnyUnownedSleepingSlot)
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }

                CompGuest compGuest = actor.GetComp<CompGuest>();
                if (compGuest.HasBed) Log.Error($"{actor.LabelShort} already has a bed ({compGuest.bed.Label})");
                compGuest.ClaimBed(newBed);
                if (actor.GetLord().ownedPawns.Count == 2)
                {
                    foreach (var pawn in actor.GetLord().ownedPawns)
                    {
                        if (pawn != actor)
                        {
                            pawn.GetComp<CompGuest>().ClaimBed(newBed);
                            pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                        }
                    }
                }                

                // we paid already
                if (needToPay && newBed.GetRentalFee() > 0)
                {
                        actor.inventory.innerContainer.TryDrop(silver, actor.Position, newBed.Map, ThingPlaceMode.Near, newBed.GetRentalFee(), out silver);
                        actor.Map.GetComponent<FinanceService>().bookIncome(FinanceReport.ReportEntryType.Beds, newBed.GetRentalFee());
                        //actor.GetComp<CompHotelGuest>().totalSpent += newBed.GetRentalFee();                        
                        if (groupSize == 2)
                        {
                            // get some money out of the other pocket :p
                            Pawn partner = actor.GetLord().ownedPawns.First(pawn => pawn != actor);
                            silver = partner.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
                            partner.inventory.innerContainer.TryDrop(silver, actor.Position, newBed.Map, ThingPlaceMode.Near, newBed.GetRentalFee(), out silver);
                            partner.Map.GetComponent<FinanceService>().bookIncome(FinanceReport.ReportEntryType.Beds, newBed.GetRentalFee());
                            //partner.GetComp<CompHotelGuest>().totalSpent += newBed.GetRentalFee();                        
                        }
                }
                if (!sleepingInATent)
                {
                    actor.ThoughtAboutRoomCleanliness(newBed);
                    actor.ThoughtAboutClaimedBed(newBed, money + newBed.GetRentalFee());
                }
                else
                {
                    actor.ThoughtAboutClaimedTent(newBed, money + newBed.GetRentalFee());
                }

            }
        }.FailOnDespawnedNullOrForbidden(TargetIndex.A);
    }
    
    public static void ThoughtAboutClaimedBed(this Pawn pawn, Building_GuestBed bed, int moneyBeforeClaiming)
    {
        var thoughtDef = ThoughtDef.Named("GuestClaimedBed");
        if (pawn == null || bed == null) return;

        var free = bed.GetRentalFee() <= 0;
        var score = bed.BedValue(pawn, moneyBeforeClaiming);
        int stage = free ? score switch
            {
                >= 100 => 6, // 5
                >= 50 => 5,  // 3
                _ => 0       // 2
            } 
            : score switch
            {
                >= 100 => 6, // 5
                >=  50 => 5, // 3
                >=   0 => 4, // 1
                >= -35 => 3, // -2
                >= -60 => 2, // -5
                _      => 1, // -10
            };
        //Log.Message($"{pawn.LabelCap} claimed bed at {bed.Position}. It scored {score:F2} for them.");

        var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef, stage);
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory); // *cough* Extra defensive
    }

    public static void ThoughtAboutClaimedTent(this Pawn pawn, Building_GuestBed bed, int moneyBeforeClaiming)
    {
        var thoughtDef = ThoughtDef.Named("GuestClaimedCampingSite");
        if (pawn == null || bed == null) return;

        bool free = bed.GetRentalFee() <= 0;
        var score = BedUtility.StaticBedValue(bed, out var room, out var quality, out var impressiveness, out var roomType, out var comfort, out var facilities);        
        int stage = free ? score switch
            {
                >= 100 => 5, // 5
                >= 50 => 4,  // 3
                _ => 0       // 2
            } 
            : score switch
            {
                >= 100 => 5, // 5
                >=  50 => 4, // 3
                >=   0 => 3, // 1
                >= -50 => 2, // -2
                _      => 1, // -10
            };
        //Log.Message($"{pawn.LabelCap} claimed bed at {bed.Position}. It scored {score:F2} for them.");

        var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef, stage);
        pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory); // *cough* Extra defensive        
    }
}