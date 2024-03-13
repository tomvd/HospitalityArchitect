using System;
using System.Linq;
using CashRegister;
using HarmonyLib;
using Hospitality;
using RimWorld;
using Storefront.Store;
using Storefront.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using Hospitality.Utilities;
using Verse.AI.Group;

namespace HospitalityArchitect.Reception
{
    public static class Toils_Hosting
    {
        public static Toil WaitForBetterJob(TargetIndex registerInd)
        {
            // Talk to patron
            var toil = new Toil();

            toil.initAction = InitAction;
            toil.tickAction = TickAction;
            toil.socialMode = RandomSocialMode.Normal;
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOnDestroyedOrNull(registerInd);
            //toil.FailOnMentalState(patronInd);

            return toil;

            void InitAction()
            {
                toil.actor.pather.StopDead();
                if(toil.actor.CurJob?.GetTarget(registerInd).Thing is Building_CashRegister register)
                {
                    var offset = register.InteractionCell - register.Position;
                    toil.actor.rotationTracker.FaceCell(toil.actor.Position + offset);
                }
            }

            void TickAction()
            {
                if (toil.actor.CurJob?.GetTarget(registerInd).Thing is Building_CashRegister register)
                {
                    if (!register.HasToWork(toil.actor) || !register.standby)
                    {
                        toil.actor.jobs.curDriver.ReadyForNextToil();
                        return;
                    }
                }
                else
                {
                    Log.Message($"Waiting - register disappeared.");
                    toil.actor.jobs.curDriver.ReadyForNextToil();
                    return;
                }

                toil.actor.GainComfortFromCellIfPossible();

                if (toil.actor.IsHashIntervalTick(35))
                {
                    toil.actor.jobs.CheckForJobOverride();
                }

                if (toil.actor.IsHashIntervalTick(113))
                {
                    if (toil.actor.Position.GetThingList(toil.actor.Map).OfType<Pawn>().Any(p => p != toil.actor))
                    {
                        toil.actor.jobs.curDriver.ReadyForNextToil();
                    }
                }
            }
        }
        
        public static Toil AnnounceSelling(TargetIndex customerInd)
        {
            var toil = Toils_Interpersonal.Interact(customerInd, InteractionDefOf.Chitchat);
            toil.defaultDuration = Rand.Range(50,100);
            toil.socialMode = RandomSocialMode.Off;
            toil.activeSkill = () => SkillDefOf.Social;
            toil.tickAction = TickAction;
            toil.initAction = InitAction;
            return toil;

            void InitAction()
            {
                Pawn actor = toil.actor;
                Job curJob = actor.CurJob;
                LocalTargetInfo targetCustomer = curJob.GetTarget(customerInd);
                //LocalTargetInfo targetItem = curJob.GetTarget(itemInd);

                var customer = targetCustomer.Pawn;
                if (customer == null)
                {
                    Log.Warning($"Can't announce selling. No customer.");
                    return;
                }
                
                if (customer.GetMoney() < 1) {
                    Log.Warning($"Can't announce selling. Customer is broke.");
                    return;
                }
                
                var buyJob = customer.jobs.curDriver as JobDriver_CheckIn;

                if (buyJob == null)
                {
                    Log.Warning($"Can't announce selling. Customer is not buying anymore.");
                    return;                    
                }
                
                if (!buyJob.WaitingInQueue)
                {
                    Log.Warning($"Can't announce selling. Customer is not waiting in queue.");
                    return;                    
                }

                buyJob.WaitingInQueue = false; // this makes the buyer stop waiting in queue
                JobUtility.GiveServiceThought(customer, toil.actor);

            }

            void TickAction()
            {
                toil.actor.rotationTracker.FaceCell(toil.actor.CurJob.GetTarget(customerInd).Cell);
            }
        }        
        public static Toil serveCustomer(TargetIndex customerInd, TargetIndex registerInd)
        {
            var toil = new Toil {atomicWithPrevious = true};
            toil.initAction = InitAction;
            return toil;

            void InitAction()
            {
                Pawn actor = toil.actor;
                //Log.Message($"{actor.NameShortColored} is selling.");
                var curJob = actor.CurJob;
                var targetCustomer = curJob.GetTarget(customerInd);
                var customer = targetCustomer.Pawn;
                //Log.Message($"{customer.NameShortColored} is customer.");
                //var targetItem = curJob.GetTarget(itemInd);
                //var item = targetItem.Thing;
                //Log.Message($"{item.Label} is product.");                
                var targetRegister = curJob.GetTarget(registerInd);
                Building_CashRegister register = targetRegister.Thing as Building_CashRegister;
                //Log.Message($"{register.Label} is register.");
                //SellThing(actor, customer, item, register as Building_CashRegister);
                // TODO replace this with claiming the bed and paying
                //var symbol = item.def.uiIcon;
                //if (symbol != null)
                    MoteMaker.MakeInteractionBubble(actor, customer, ThingDefOf.Mote_Speech, InteractionDefOf.BuildRapport.GetSymbol());
                //Log.Message($"{customer.jobs.curDriver} is curjob driver.");

                
                var bed = customer.FindBedFor();
                if (bed == null)
                {
                    // something went horribly wrong, sent the customer home
                    GuestUtility.HotelGuestLeaves(customer);
                    return;
                }
                var price = bed.GetRentalFee();
                foreach (var lordPawn in customer.GetLord().ownedPawns)
                {
                    // check also if the guest didn already pay for a bed (and it got destroyed?) - in which case they get their bed for free now
                    if (price > 0 && lordPawn.GetComp<CompHotelGuest>().totalSpent < price)
                    {
                        var inventory = lordPawn.inventory.innerContainer;                    
                        Thing silver = inventory.FirstOrDefault(i => i.def == ThingDefOf.Silver);
                        if (silver == null)                 
                        {
                            // something went horribly wrong, sent the customer home
                            GuestUtility.HotelGuestLeaves(lordPawn);
                            return;
                        }

                        if (register is not null)
                        {
                            int transferred = lordPawn.inventory.innerContainer.TryTransferToContainer(silver,
                                register.GetDirectlyHeldThings(), price);
                            // check how much silver we got, and if its less than we ask for, create some extra silver out of thin air :)
                            Log.Message($"transferred {transferred} silver from {lordPawn.NameFullColored} to register");
                            /*if (transferred < price)
                            {
                                int debt = price - transferred;
                                var silverThing = ThingMaker.MakeThing(ThingDefOf.Silver);
                                silverThing.stackCount = debt;
                                Log.Message($"transfer {debt}  extra silver to register");
                                register.GetDirectlyHeldThings().TryAdd(silverThing, true);
                            }*/
                        }
                        else
                        {
                            Log.Message($"register is gone? drop silver on the ground");
                            lordPawn.inventory.innerContainer.TryDrop(silver, lordPawn.Position, actor.Map, ThingPlaceMode.Near,
                                price, out silver);
                        }
                        actor.Map.GetReceptionManager().GetLinkedReception(register).AddToIncomeToday(price);
                        actor.Map.GetComponent<FinanceService>().bookIncome(FinanceReport.ReportEntryType.Beds, price);
                        //lordPawn.GetComp<CompHotelGuest>().totalSpent += price;
                    }
                }
                // if we are a couple, only the first one inspects/claims the bed
                if (customer.GetLord().ownedPawns.Count == 2)
                {
                    Pawn partner = customer.GetLord().ownedPawns.First(pawn => pawn != customer);
                    partner.jobs.EndCurrentJob(JobCondition.Succeeded);
                }
                actor.skills.GetSkill(SkillDefOf.Social).Learn(150);
                if (customer.jobs.curDriver is JobDriver_CheckIn buyJob)
                    buyJob.WaitingToBeServed = false; // this makes the buyer go on with his business - job is done                
                customer.jobs.EndCurrentJob(JobCondition.Succeeded);
                customer.jobs.StartJob(new Job(HADefOf.InspectBed, bed));
            }
        }
    }
}
